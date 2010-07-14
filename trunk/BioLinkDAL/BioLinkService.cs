using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace BioLink.Data{

    public abstract class BioLinkService {

        public User User { get; private set; }

        public BioLinkService(User user) {
            this.User = user;
        }

        protected void Command(SqlCommandDelegate commandfunc) {
            if (commandfunc == null) {
                return;
            }

            using (SqlConnection conn = User.GetConnection()) {
                using (SqlCommand command = conn.CreateCommand()) {
                    commandfunc(conn, command);
                }
            }
        }

        protected void StoredProcReaderForEach(string proc, BiolinkServiceReader func) {
            Command((con, cmd) => {
                cmd.CommandText = String.Format("exec {0}", proc);
                using (SqlDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        func(reader);
                    }
                }
            });
        }

        public delegate void SqlCommandDelegate(SqlConnection connection, SqlCommand command);

        public delegate void BiolinkServiceReader(SqlDataReader reader);

    }

}
