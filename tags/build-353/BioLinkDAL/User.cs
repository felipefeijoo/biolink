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

        private Dictionary<PermissionMask, Permission> _permissions;

        public bool Authenticate(out String message) {

            SqlConnection connection = null;
            try {
                Logger.Debug("Attemping to connect to {0} (Database {1}) with username '{2}'...", ConnectionProfile.Server, ConnectionProfile.Database, Username);
                using (GetConnection()) {
                    message = "Ok";

                    // Load permissions....
                    Logger.Debug("Retrieving User permissions");
                    var service = new SupportService(this);
                    var user = service.GetUser(Username);
                    if (user != null) {
                        var permissions = service.GetPermissions(user.GroupID);
                        _permissions = permissions.ToDictionary((p) => {
                            return (PermissionMask)p.PermissionID;
                        });

                        if (user.CanCreateUsers) {
                            _permissions[PermissionMask.USERMANAGER_USER] = new Permission { Mask1 = 199 };
                        }
                    }

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

        public bool IsSysAdmin {
            get { return _username.Equals("sa", StringComparison.CurrentCultureIgnoreCase); }
        }

        public bool HasPermission(PermissionMask perm, PERMISSION_MASK mask) {

            if (Username.Equals("sa", StringComparison.CurrentCultureIgnoreCase)) {
                return true;
            }

            if (_permissions.ContainsKey(perm)) {
                var val = _permissions[perm];
                return (val.Mask1 & (int)mask) != 0;
            }

            return false;
        }

        public void CheckPermission(PermissionMask perm, PERMISSION_MASK mask, string deniedMessage) {
            if (!HasPermission(perm, mask)) {
                throw new NoPermissionException(perm, mask, deniedMessage);
            }
        }

        public void BeginTransaction(SqlConnection connection = null) {
            if (_transaction != null) {
                throw new Exception("A pending transaction already exists!");
            }
            SqlConnection conn = connection;
            if (conn == null) {
                conn = GetConnection();
            }
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

        private string BuildConnectionString(bool oldMangleRoutine) {
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

        public static string MaskStr(int mask, string username) {
            if (!string.IsNullOrEmpty(username) && username.Equals("sa", StringComparison.CurrentCultureIgnoreCase)) {
                return "[All Rights - Sys Admin]";
            }

            if (mask == 0) {
                return "[No Rights]";
            }

            StringBuilder sBuf = new StringBuilder("[");

            if ((mask & (int)PERMISSION_MASK.ALLOW) != 0) {
                sBuf.Append("Allowed,");
            }

            if ((mask & (int)PERMISSION_MASK.READ) != 0) {
                sBuf.Append("Read,");
            }

            if ((mask & (int)PERMISSION_MASK.WRITE) != 0) {
                if ((mask & (int)PERMISSION_MASK.INSERT) != 0) {
                    sBuf.Append("Insert,");
                }
                if ((mask & (int)PERMISSION_MASK.UPDATE) != 0) {
                    sBuf.Append("Update,");
                }
                if ((mask & (int)PERMISSION_MASK.DELETE) != 0) {
                    sBuf.Append("Delete,");
                }
            }

            if (sBuf[sBuf.Length - 1] == ',') {
                sBuf.Remove(sBuf.Length - 1, 1);
            }
            sBuf.Append("]");

            var maskstr = sBuf.ToString();
            if (maskstr.Equals("[read]", StringComparison.CurrentCultureIgnoreCase)) {
                return "[Read Only]";
            } else {
                return sBuf.ToString();
            }

        }

        public int GetPermissionMask(PermissionMask PermissionID) {
            if (Username.Equals("sa", StringComparison.CurrentCultureIgnoreCase)) {
                return 0xFFFFFF;
            }
            if (_permissions.ContainsKey(PermissionID)) {
                return _permissions[PermissionID].Mask1;
            }

            return 0;
        }

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

        #endregion

        public static PERMISSION_TYPE GetPermissionType(PermissionMask mask) {

            var boolTypes = new PermissionMask[] { PermissionMask.SPARC_EXPLORER,PermissionMask.SPIN_EXPLORER, PermissionMask.IMPORT_MATERIAL, PermissionMask.IMPORT_REFERENCES, PermissionMask.IMPORT_DELTA  };

            if (boolTypes.Contains(mask)) {
                return PERMISSION_TYPE.ALLOWDISALLOW;
            }

            return PERMISSION_TYPE.RWDIU;
        }

    }

    public static class PermissionGroups {

        private static Dictionary<byte, string> _groupDescriptions = new Dictionary<byte, string>();

        static PermissionGroups() {
            AddDescription(0xFF, "User Manager");
            AddDescription(0xFE, "Specimens");
            AddDescription(0xFD, "Taxa");
            AddDescription(0xFC, "Support Data");
            AddDescription(0xFB, "Import");
        }

        private static void AddDescription(byte prefix, string desc) {
            _groupDescriptions[prefix] = desc;
        }

        public static Dictionary<byte, string> Descriptions {
            get {
                return _groupDescriptions;
            }
        }

        public static string GetDescriptionForPermission(PermissionMask perm) {
            byte prefix = (byte) ((((int) perm) & 0xFF00) >> 8);
            if (_groupDescriptions.ContainsKey(prefix)) {
                return _groupDescriptions[prefix];
            }

            throw new Exception("Unrecognized permission prefix: " + prefix);
        }

    }

    public enum PermissionMask {
        // User Manager --------------------------------------------------------
        USERMANAGER_USER = 0xFF00,
        USERMANAGER_GROUP = 0xFF01,
        // Sparc ---------------------------------------------------------------
        SPARC_SITE = 0xFE00,
        SPARC_TRAP = 0xFE01,
        SPARC_SITEVISIT = 0xFE02,
        SPARC_MATERIAL = 0xFE03,
        SPARC_EXPLORER = 0xFE04,
        SPARC_REGION = 0xFE05,
        SPARC_SITEGROUP = 0xFE06,
        // spIn ----------------------------------------------------------------
        SPIN_EXPLORER = 0xFD00,
        SPIN_TAXON = 0xFD01,
        // SupportData ---------------------------------------------------------
        SUPPORT_PHRASES = 0xFC00,
        SUPPORT_PHRASECATEGORIES = 0xFC01,
        SUPPORT_REFS = 0xFC02,
        SUPPORT_JOURNALS = 0xFC03,
        SUPPORT_CATEGORIES = 0xFC04,
        // Import ---------------------------------------------------------
        IMPORT_MATERIAL = 0xFB00,
        IMPORT_REFERENCES = 0xFB01,
        IMPORT_DELTA = 0xFB02
    };

    public enum PERMISSION_MASK {
        OWNER = 512,
        ALLOW = 256,
        READ = 128,
        WRITE = 64,
        UPDATE = 4,
        INSERT = 2,
        DELETE = 1
    };

    public enum PERMISSION_TYPE {
        RWDIU = 0,
        ALLOWDISALLOW = 1
    };

    public class NoPermissionException : Exception {

        public NoPermissionException(PermissionMask perm, PERMISSION_MASK mask, string deniedMessage = "") : base(String.Format("You do not have permission to perform this operation: {0} :: {1}", perm.ToString(), mask.ToString())) {
            this.RequestedPermission = perm;
            this.RequestedMask = mask;
            this.DeniedMessage = deniedMessage;
        }

        public PermissionMask RequestedPermission { get; private set; }
        public PERMISSION_MASK RequestedMask { get; private set; }
        public string DeniedMessage { get; private set; }
    }

}
