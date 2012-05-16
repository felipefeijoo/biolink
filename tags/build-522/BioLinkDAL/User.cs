/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using BioLink.Client.Utilities;
using BioLink.Data.Model;
using Microsoft.VisualBasic;
using System.Security.Cryptography;

namespace BioLink.Data {

    public class User {

        private readonly String Password;
        private DbTransaction _transaction;
        private string _username;

        private Dictionary<PermissionCategory, Permission> _permissions;

        public User(string username, string password, ConnectionProfile profile) {
            this.Username = username;
            this.Password = password;
            this.ConnectionProfile = profile;
            if (profile.ConnectionType == ConnectionType.SQLServer) {
                ConnectionProvider = new SQLServerConnectionProvider();
            } else if (profile.ConnectionType == ConnectionType.Standalone) {
                ConnectionProvider = new SQLiteConnectionProvider();
            }
        }

        public IConnectionProvider ConnectionProvider { get; private set; }

        public bool Authenticate(out String message) {

            DbConnection connection = null;
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
                        _permissions = permissions.ToDictionary(p => (PermissionCategory)p.PermissionID);

                        if (user.CanCreateUsers) {
                            _permissions[PermissionCategory.USERMANAGER_USER] = new Permission { Mask1 = 199 };
                        }
                    }

                    return true;
                }

            } catch (DbException sqlex) {
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

        public bool HasPermission(PermissionCategory perm, PERMISSION_MASK mask) {

            if (IsSysAdmin) {
                return true;
            }

            if (_permissions != null && _permissions.ContainsKey(perm)) {
                var val = _permissions[perm];
                return (val.Mask1 & (int)mask) != 0;
            }

            return false;
        }

        public bool HasBiotaPermission(int taxonId, PERMISSION_MASK mask) {

            if (taxonId == 0) {
                return true;
            }

            // system administrator has full rights to all.
            if (IsSysAdmin) {
                return true;
            }

            // Ensure the permissions set at the user group level take precendence to the individual taxon based permissions.
            if (mask != PERMISSION_MASK.OWNER) {
                if (!HasPermission(PermissionCategory.SPIN_TAXON, mask)) {
                    return false;
                }
            }

            if (taxonId < 0) {
                // new items are automatically approved!
                return true;
            }

            var service = new SupportService(this);

            //if (service.HasBiotaPermission(taxonId, mask)) {
            //    return true;
            //}

            var perms = service.GetBiotaPermissions(Username, taxonId);
            if (perms == null) {
                return false;
            } else {
                if (perms.PermMask1 == 0) {
                    // If there are owners of this taxa then the user needs permissions...
                    return perms.NumOwners == 0;
                } else {
                    return (perms.PermMask1 & (int) mask) != 0;
                }
            }                            
        }

        public void CheckPermission(PermissionCategory perm, PERMISSION_MASK mask, string deniedMessage) {
            if (!HasPermission(perm, mask)) {
                throw new NoPermissionException(perm, mask, deniedMessage);
            }
        }

        public void BeginTransaction(DbConnection connection = null) {
            if (_transaction != null) {
                throw new Exception("A pending transaction already exists!");
            }
            var conn = connection;
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

        public DbConnection GetConnection() {
            if (_transaction != null) {
                Logger.Debug("Retrieving existing transaction connection to {0} (Database {1}) with username '{2}'...", ConnectionProfile.Server, ConnectionProfile.Database, Username);
                return _transaction.Connection;
            }

            Logger.Debug("Opening new connection to {0} (Database {1}) with username '{2}'...", ConnectionProfile.Server, ConnectionProfile.Database, Username);

            if (ConnectionProvider == null) {
                throw new Exception("Connection type " + ConnectionProfile.ConnectionType + " is not supported at this time!");
            }

            return ConnectionProvider.GetConnection(ConnectionProfile, Username, Password);
        }

        #region Properties

        public Boolean InTransaction {
            get { return _transaction != null; }
        }

        public DbTransaction CurrentTransaction {
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

        public int GetPermissionMask(PermissionCategory PermissionID) {

            if (Username.Equals("sa", StringComparison.CurrentCultureIgnoreCase)) {
                return 0xFFFFFF;
            }

            if (_permissions != null && _permissions.ContainsKey(PermissionID)) {
                return _permissions[PermissionID].Mask1;
            }

            return 0;
        }

        #endregion

        public static PERMISSION_TYPE GetPermissionType(PermissionCategory mask) {

            var boolTypes = new PermissionCategory[] { PermissionCategory.SPARC_EXPLORER,PermissionCategory.SPIN_EXPLORER, PermissionCategory.IMPORT_MATERIAL, PermissionCategory.IMPORT_REFERENCES, PermissionCategory.IMPORT_DELTA  };

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

        public static string GetDescriptionForPermission(PermissionCategory perm) {
            byte prefix = (byte) ((((int) perm) & 0xFF00) >> 8);
            if (_groupDescriptions.ContainsKey(prefix)) {
                return _groupDescriptions[prefix];
            }

            throw new Exception("Unrecognized permission prefix: " + prefix);
        }

    }

    public enum PermissionCategory {
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

        public NoPermissionException(string message) : base(message) {
            this.DeniedMessage = message;
        }

        public NoPermissionException(PermissionCategory permissionCategory, PERMISSION_MASK mask, string deniedMessage = "") : base(String.Format("You do not have permission to perform this operation: {0} :: {1}", permissionCategory.ToString(), mask.ToString())) {
            this.PermissionCategory = permissionCategory;
            this.RequestedMask = mask;
            this.DeniedMessage = deniedMessage;
        }

        public PermissionCategory PermissionCategory { get; private set; }
        public PERMISSION_MASK RequestedMask { get; private set; }
        public string DeniedMessage { get; private set; }
    }

}
