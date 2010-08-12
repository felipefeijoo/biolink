using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using BioLink.Client.Utilities;

namespace BioLink.Data {

    public class User {

        public String Username { get; private set; }
        private String Password;
        public ConnectionProfile ConnectionProfile { get; private set; }

        public User(string username, string password, ConnectionProfile profile) {
            this.Username = username;
            this.Password = password;
            this.ConnectionProfile = profile;
        }

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

        private string ConnectionString {
            get {
                StringBuilder s = new StringBuilder();
                if (ConnectionProfile.IntegratedSecurity) {
                    s.Append(String.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;", ConnectionProfile.Server, ConnectionProfile.Database));
                } else {
                    s.Append(String.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3}", ConnectionProfile.Server, ConnectionProfile.Database, Username, Password));
                }

                return s.ToString();

            }

        }

        public SqlConnection GetConnection() {
            Logger.Debug("Opening connection to {0} (Database {1}) with username '{2}'...", ConnectionProfile.Server, ConnectionProfile.Database, Username);
            SqlConnection connection = new SqlConnection(this.ConnectionString);
            connection.Open();            
            return connection;
        }

    }

}
