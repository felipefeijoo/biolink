using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using BioLink.Client.Utilities;
using BioLink.Data.Model;
using Microsoft.VisualBasic;
using System.Data;

namespace BioLink.Data {

    public class SQLServerConnectionProvider : IConnectionProvider {

        private String _connectionString;

        public DbConnection GetConnection(ConnectionProfile profile, String username, String password) {

            SqlConnection conn = null;
            if (_connectionString == null) {
                _connectionString = BuildConnectionString(profile, username, password, false);
                conn = new SqlConnection(_connectionString);
                try {
                    conn.Open();
                } catch (Exception) {
                    _connectionString = BuildConnectionString(profile, username, password, true);
                    conn = new SqlConnection(_connectionString);
                    try {
                        conn.Open();
                    } catch (Exception ex) {
                        _connectionString = null;
                        throw ex;
                    }
                }
                return conn;
            } else {
                conn = new SqlConnection(_connectionString);
                conn.Open();
                return conn;
            }

        }

        private string BuildConnectionString(ConnectionProfile profile, String username, String password, bool oldMangleRoutine) {
            StringBuilder s = new StringBuilder();
            if (profile.IntegratedSecurity) {
                s.Append(String.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;", profile.Server, profile.Database));
            } else {
                var mangledPassword = password;
                if (oldMangleRoutine) {
                    mangledPassword = PasswordUtilities.OldManglePassword(username, password);
                } else {
                    mangledPassword = PasswordUtilities.ManglePassword(username, password);
                }

                s.Append(String.Format("Data Source={0};User Id={2};Password=\'{3}\';Initial Catalog={1};", profile.Server, profile.Database, username, mangledPassword));
            }

            return s.ToString();
        }

        void IConnectionProvider.StoredProcReaderForEach(User user, DbCommand cmd, string proc, ServiceReaderAction action, Action<string> message, params DbParameter[] @params) {
            cmd.CommandText = proc;
            cmd.CommandType = CommandType.StoredProcedure;
            if (user.InTransaction && user.CurrentTransaction != null) {
                cmd.Transaction = user.CurrentTransaction;

            }
            foreach (DbParameter param in @params) {
                cmd.Parameters.Add(param);
            }

            using (var reader = cmd.ExecuteReader()) {
                message("Fetching records...");
                int count = 0;
                while (reader.Read()) {
                    if (action != null) {
                        action(reader);
                    }
                    if (++count % 1000 == 0) {
                        message(String.Format("{0} records retrieved", count));
                    }
                }
            }
        }


        public void StoredProcReaderFirst(User user, DbCommand cmd, string proc, ServiceReaderAction action, Action<string> message, params DbParameter[] @params) {
            cmd.CommandText = proc;
            cmd.CommandType = CommandType.StoredProcedure;
            if (user.InTransaction && user.CurrentTransaction != null) {
                cmd.Transaction = user.CurrentTransaction;
            }
            foreach (DbParameter param in @params) {
                cmd.Parameters.Add(param);
            }

            using (var reader = cmd.ExecuteReader()) {
                if (reader.Read()) {
                    if (action != null) {
                        action(reader);
                    }
                }
            }
        }

        public int StoredProcUpdate(User user, DbCommand cmd, string proc, params DbParameter[] @params) {
            cmd.CommandText = proc;
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            if (user.InTransaction && user.CurrentTransaction != null) {
                cmd.Transaction = user.CurrentTransaction;
            }
            foreach (SqlParameter param in @params) {
                cmd.Parameters.Add(param);
            }
            return cmd.ExecuteNonQuery();                                
        }

        public bool IsSysAdmin(User user) {
            if (String.IsNullOrEmpty(user.Username)) {
                return false;
            }
            bool isSysAdmin = false;
            // try and connect to find out...
            try {
                Command(user, (con, cmd) => {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT IS_SRVROLEMEMBER('sysadmin') as IsSysAdmin;";
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            var result = reader[0] as Int32?;
                            isSysAdmin = result.HasValue && result.Value == 1;
                        }
                    }
                });
            } catch (Exception) {
                isSysAdmin = false;
            }

            if (!isSysAdmin) {
                isSysAdmin = user.Username.Equals("sa", StringComparison.CurrentCultureIgnoreCase);
            }

            return isSysAdmin;
        }

        protected void Command(User user, ServiceCommandAction action) {
            // If no action dont' bother with the connection or command
            if (action == null) {
                return;
            }

            // Get a connection

            var connection = user.GetConnection();

            // and create a command instance
            try {
                using (DbCommand command = connection.CreateCommand()) {

                    if (user.ConnectionProfile.Timeout.GetValueOrDefault(-1) > 0) {
                        if (user.ConnectionProfile.Timeout != null) {
                            command.CommandTimeout = user.ConnectionProfile.Timeout.Value;
                        }
                    }

                    // invoke the action with the command
                    action(connection, command);
                }
            } finally {
                if (!user.InTransaction) {
                    connection.Dispose();
                }
            }
        }

        public DbParameter CreateParameter(string name, object value) {
            return new SqlParameter(name, value);
        }
    }

    public static class PasswordUtilities {
        /// <summary>
        /// New Mangle Password routine that uses SHA256 + Salt to create a hash of the user password to be used as the actual password.
        /// This prevents the user from being able to use the password they selected to access the sql server database directly. Note that
        /// the SA password is not mangled - it is always just passed through.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string ManglePassword(string username, string password) {
            if (username.Equals("sa", StringComparison.CurrentCultureIgnoreCase)) {
                // Retain the direct password for sa. Note that semi-colons disrupt the login connection string so mask them
                return password.Replace(';', '_');
            }
            var sha1 = new SHA256Managed();
            var salt = "biolinksalt";
            string buf = String.Format("{0}{1}", password, salt);
            Encoding enc = Encoding.GetEncoding("UTF-8");
            return Convert.ToBase64String(sha1.ComputeHash(enc.GetBytes(buf)));
        }

        /// <summary>
        /// This function is dodgey as hell. It is supposed to mangle the password by using the original characters of the password and jumble them around
        /// a bit, which in itself is a problem because its reversable, but instead, due to a coding error (using instr instead of mid) it returns the same mangled password
        /// for each password of the same length. This is the mangle function using in the old biolink, however, so we have to use it...
        /// 
        ///  A better solution would be to simply take the SHA1 hash of the original password + a salt and use that as the mangled password, however
        ///  this would break backward compatibility with the old client, so we need to deal with this somehow...
        ///  
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string OldManglePassword(string UserID, string password) {

            //'
            //' Since a nasty 'black-hat' has published the original mangle routine, we need to
            //' make it a little harder to crack.
            //'
            //' NJF, 8 May, 2001, Error#660

            if (UserID.Equals("sa", StringComparison.CurrentCultureIgnoreCase)) {
                // Retain the direct password for sa. Note that semi-colons disrupt the login connection string so mask them
                return password.Replace(';', '_');
            } else {
                var s = new StringBuilder();
                // perform the crunch to get an ascii first up                  
                var instr = String.Format("{0}", Strings.InStr(password, string.Format("{0}", 0)));
                s.Append(Strings.Chr((byte)(((Strings.Asc(instr[0]) * 34) % 26) + 1) + Strings.Asc('a')));  // Chr((((Asc(InStr(password, i)) * 34) Mod 26) + 1) + Asc("a"))
                // perform the crunch to get an alpha numeric
                for (int i = 1; i < password.Length; ++i) {
                    // s.Append((char) (((password[i] * 23) % (10 + i) + 1) + (int) 'A'));         //    ManglePassword = ManglePassword & Chr((((Asc(InStr(password, i)) * 23) Mod (10 + i)) + 1) + Asc("A"))
                    instr = String.Format("{0}", Strings.InStr(password, string.Format("{0}", i + 1)));
                    var lMagicNumber = ((Strings.Asc(instr[0]) * 23) % (62 - password.Length + (i + 1))); // ((Asc(InStr(password, i)) * 23) Mod (62 - Len(password) + i))
                    if (lMagicNumber >= 0 && lMagicNumber <= 31) {
                        s.Append(Strings.Chr(lMagicNumber + 1));
                    } else if (lMagicNumber >= 32 && lMagicNumber <= 61) {
                        s.Append(Strings.Chr(lMagicNumber - 32 + 128));
                    } else {
                        Debug.Assert(false, "unexpected magic number during Password Mangle");
                    }
                }
                // Note that semi-colons disrupt the login connection string so mask them                
                s.Replace(';', '_');
                return s.ToString();
            }
        }

    }
}
