using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using BioLink.Client.Utilities;
using BioLink.Data.Model;
using Microsoft.VisualBasic;
using System.Security.Cryptography;

namespace BioLink.Data {

    public class User {

        private String Password;
        private SqlTransaction _transaction;
        private string _username;
        private string _connectionString;

        public bool Authenticate(out String message) {

            SqlConnection connection = null;
            try {
                Logger.Debug("Attemping to connect to {0} (Database {1}) with username '{2}'...", ConnectionProfile.Server, ConnectionProfile.Database, Username);
                using (GetConnection()) {
                    message = "Ok";
                    return true;
                }
            } catch (SqlException sqlex) {
                message = sqlex.Message;
                Logger.Warn("SQL Exception: {0}", sqlex.Message);
            } catch (Exception ex) {
                message = ex.Message;
                Logger.Error("Unexpected error connecting to database!", ex);
            } finally {
                if (connection != null) {
                    connection.Close();
                    connection.Dispose();
                }
            }

            return false;
        }

        public void BeginTransaction() {
            if (_transaction != null) {
                throw new Exception("A pending transaction already exists!");
            }

            SqlConnection conn = GetConnection();
            _transaction = conn.BeginTransaction();
        }

        public void RollbackTransaction() {
            if (_transaction != null) {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void CommitTransaction() {
            if (_transaction != null) {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public SqlConnection GetConnection() {
            if (_transaction != null) {
                Logger.Debug("Retrieving existing transaction connection to {0} (Database {1}) with username '{2}'...", ConnectionProfile.Server, ConnectionProfile.Database, Username);
                return _transaction.Connection;
            }

            Logger.Debug("Opening new connection to {0} (Database {1}) with username '{2}'...", ConnectionProfile.Server, ConnectionProfile.Database, Username);

            SqlConnection conn = null;

            if (_connectionString == null) {
                _connectionString = BuildConnectionString(false);
                conn = new SqlConnection(_connectionString);
                try {
                    conn.Open();
                } catch (Exception) {
                    _connectionString = BuildConnectionString(true);
                    conn = new SqlConnection(_connectionString);
                    try {
                        conn.Open();
                    } catch (Exception) {
                        _connectionString = null;
                    }
                }
                return conn;
            } else {
                conn = new SqlConnection(_connectionString);
                conn.Open();
                return conn;
            }
        }

        #region Properties

        public Boolean InTransaction {
            get { return _transaction != null; }
        }

        public SqlTransaction CurrentTransaction {
            get { return _transaction; }
        }

        public String Username {
            get {
                if (ConnectionProfile.IntegratedSecurity || String.IsNullOrEmpty(_username)) {
                    return Environment.UserName;
                } else {
                    return _username;
                }
            }
            private set {
                _username = value;
            }
        }

        public ConnectionProfile ConnectionProfile { get; private set; }

        public User(string username, string password, ConnectionProfile profile) {
            this.Username = username;
            this.Password = password;
            this.ConnectionProfile = profile;
        }

        private string BuildConnectionString(bool oldMangleRoutine)  {
            StringBuilder s = new StringBuilder();
            if (ConnectionProfile.IntegratedSecurity) {
                s.Append(String.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;", ConnectionProfile.Server, ConnectionProfile.Database));
            } else {
                var mangledPassword = Password;
                if (oldMangleRoutine) {
                    mangledPassword = OldManglePassword(Username, Password);
                } else {
                    mangledPassword = ManglePassword(Username, Password);
                }

                s.Append(String.Format("Data Source={0};User Id={2};Password=\'{3}\';Initial Catalog={1};", ConnectionProfile.Server, ConnectionProfile.Database, Username, mangledPassword));
            }

            return s.ToString();
        }

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
        /// a bit, which in itself is a problem, but instead, due to a coding error (using instr instead of mid) it returns the same mangled password
        /// for each password of the same length. This is the mangle function using in the old biolink, however, so we have to use it...
        /// 
        ///  A better solution would be to simply take the SHA1 hash of the original password + a salt and use that as the mangled password, however
        ///  this would break backward compatibility with the old client, so we need to deal with this somehow...
        ///  
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private string OldManglePassword(string UserID, string password) {
            
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
                s.Append(Strings.Chr((byte)(((Strings.Asc('0') * 34) % 26) + 1) + Strings.Asc('a')));  // Chr((((Asc(InStr(password, i)) * 34) Mod 26) + 1) + Asc("a"))
                // perform the crunch to get an alpha numeric
                for (int i = 1; i < password.Length; ++i) {
                    // s.Append((char) (((password[i] * 23) % (10 + i) + 1) + (int) 'A'));         //    ManglePassword = ManglePassword & Chr((((Asc(InStr(password, i)) * 23) Mod (10 + i)) + 1) + Asc("A"))
                    var instr = String.Format("{0}", Strings.InStr(password, string.Format("{0}", i + 1)));
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

        #endregion

    }

}
