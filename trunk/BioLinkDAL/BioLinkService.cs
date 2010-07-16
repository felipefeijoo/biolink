using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Reflection;

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

        protected void StoredProcReaderForEach(string proc, BiolinkServiceReader func, params SqlParameter[] @params) {
            Command((con, cmd) => {
                cmd.CommandText = proc;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                foreach (SqlParameter param in @params) {
                    cmd.Parameters.Add(param);
                }

                using (SqlDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        func(reader);
                    }
                }
            });
        }

        protected void ReflectMap(object dest, SqlDataReader reader) {        
            PropertyInfo[] props = dest.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Dictionary<string, PropertyInfo> propMap = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo propInfo in props) {
                if (propInfo.CanWrite) {
                    propMap.Add(propInfo.Name, propInfo);
                }
            }

            for (int i = 0; i < reader.FieldCount; ++i) {
                string name = reader.GetName(i);
                if (propMap.ContainsKey(name)) {
                    object val = reader[i];
                    if (val is DBNull) {
                        val = null;
                    } else if (val is string) {
                        val = (val as string).TrimEnd();
                    }
                    propMap[name].SetValue(dest, val, null);
                }
            }                    
        }

        public delegate void SqlCommandDelegate(SqlConnection connection, SqlCommand command);

        public delegate void BiolinkServiceReader(SqlDataReader reader);

    }

}
