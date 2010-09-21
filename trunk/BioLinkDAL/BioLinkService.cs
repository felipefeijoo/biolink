using System;
using System.Data;
using System.Data.SqlClient;
using BioLink.Client.Utilities;
using System.Collections.Generic;
using BioLink.Data.Model;

namespace BioLink.Data {

    /// <summary>
    /// Base class for BioLink data services. It provides helper routines to ease the burden of
    /// extracting and mapping data from the BioLink database.
    /// </summary>
    public abstract class BioLinkService {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="user">The User instance used to connect to the database</param>
        public BioLinkService(User user) {
            this.User = user;            
        }

        /// <summary>
        /// Constructs a new SqlCommand instance, and invokes the supplied ServiceCommandAction delegate with it.
        /// The command instance is automatically cleaned up once the action has been executed
        /// </summary>
        /// <param name="commandfunc"></param>
        protected void Command(ServiceCommandAction action) {
            // If no action dont' bother with the connection or command
            if (action == null) {
                return;
            }

            // Get a connection
            
            SqlConnection connection = User.GetConnection();
            
            // and create a command instance
            try {
                using (SqlCommand command = connection.CreateCommand()) {

                    if (User.ConnectionProfile.Timeout.GetValueOrDefault(-1) > 0) {
                        command.CommandTimeout = User.ConnectionProfile.Timeout.Value;
                    }
                    
                    // invoke the action with the command
                    action(connection, command);
                }
            } finally {
                if (!User.InTransaction) {
                    connection.Dispose();
                }
            }
            
        }

        /// <summary>
        /// Convenience method for calling a stored procedure and iterating over each row retrieved. The supplied
        /// ServiceReaderAction is invoked for each row, and is expected to NOT advance the reader itself, but rather
        /// allow this method to act as a controller over the rowset
        /// </summary>
        /// <param name="proc">The name of the stored procedure</param>
        /// <param name="func">The action to be called for each row</param>
        /// <param name="params">A params array for the arguments of the stored proc</param>
        protected void StoredProcReaderForEach(string proc, ServiceReaderAction action, params SqlParameter[] @params) {
            Message("Executing query...");
            using (new CodeTimer(String.Format("StoredProcReaderForEach '{0}'", proc))) {
                Logger.Debug("Calling stored procedure (reader): {0}", proc);
                Command((con, cmd) => {
                    cmd.CommandText = proc;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (User.InTransaction && User.CurrentTransaction != null) {
                        cmd.Transaction = User.CurrentTransaction;
                    }
                    foreach (SqlParameter param in @params) {
                        cmd.Parameters.Add(param);
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        Message("Fetching records...");
                        int count = 0;
                        while (reader.Read()) {                            
                            if (action != null) {
                                action(reader);
                            }
                            if (++count % 1000 == 0) {
                                Message("{0} records retrieved", count);
                            }
                        }
                    }
                });
            }
        }

        protected void StoredProcReaderFirst(string proc, ServiceReaderAction action, params SqlParameter[] @params) {
            using (new CodeTimer(String.Format("StoredProcReaderFirst '{0}'", proc))) {
                Logger.Debug("Calling stored procedure (reader): {0}", proc);
                Command((con, cmd) => {
                    cmd.CommandText = proc;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (User.InTransaction && User.CurrentTransaction != null) {
                        cmd.Transaction = User.CurrentTransaction;
                    }
                    foreach (SqlParameter param in @params) {
                        cmd.Parameters.Add(param);
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        if (reader.Read()) {
                            if (action != null) {
                                action(reader);
                            }
                        }
                    }
                });
            }
        }

        protected T StoredProcReturnVal<T>(string proc, params SqlParameter[] @params) {
            using (new CodeTimer(String.Format("StoredProcReaderFirst '{0}'", proc))) {
                
                Array.Resize(ref @params, @params.Length + 1);
                SqlParameter ret = ReturnParam("Return Value", SqlDbType.Variant);
                @params[@params.Length - 1] = ret;

                Command((con, cmd) => {
                    cmd.CommandText = proc;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (User.InTransaction && User.CurrentTransaction != null) {
                        cmd.Transaction = User.CurrentTransaction;
                    }
                    foreach (SqlParameter param in @params) {
                        cmd.Parameters.Add(param);
                    }
                    cmd.ExecuteNonQuery();                    

                });

                return (T) ret.Value;
            }
        }

        protected List<T> StoredProcToList<T>(string storedproc, GenericMapper<T> mapper, params SqlParameter[] @params) where T : new() {
            List<T> list = new List<T>();

            StoredProcReaderForEach(storedproc, (reader) => {
                list.Add(mapper.Map(reader));
            }, @params);

            return list;
        }

        protected DataTable StoredProcDataTable(string proc, params SqlParameter[] @params) {

            DataTable table = null;
            StoredProcReaderForEach(proc, (reader) => {

                if (table == null) {
                    table = new DataTable();
                    for (int i = 0; i < reader.FieldCount; ++i) {
                        table.Columns.Add(reader.GetName(i));
                    }
                }

                DataRow row = table.NewRow();

                for (int i = 0; i < reader.FieldCount; ++i) {
                    row[i] = reader[i];
                }
                table.Rows.Add(row);

            }, @params);

            return table;
        }

        protected DataMatrix StoredProcDataMatrix(string proc, params SqlParameter[] @params) {

            DataMatrix matrix = null;
            StoredProcReaderForEach(proc, (reader) => {

                if (matrix == null) {
                    matrix = new DataMatrix();
                    for (int i = 0; i < reader.FieldCount; ++i) {
                        matrix.Columns.Add(new MatrixColumn { Name=reader.GetName(i) });
                    }
                }

                MatrixRow row = matrix.AddRow();
                for (int i = 0; i < reader.FieldCount; ++i) {
                    if (!reader.IsDBNull(i)) {
                        row[i] = reader[i];
                    }
                }
                

            }, @params);

            if (matrix == null) {
                matrix = new DataMatrix();
            }

            return matrix;
        }


        protected int StoredProcUpdate(string proc, params SqlParameter[] @params) {
            int rowsAffected = -1;
            using (new CodeTimer(String.Format("StoredProcUpdate '{0}'", proc))) {
                Logger.Debug("Calling stored procedure (update): {0}", proc);
                Command((con, cmd) => {
                    cmd.CommandText = proc;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (User.InTransaction && User.CurrentTransaction != null) {
                        cmd.Transaction = User.CurrentTransaction;
                    }
                    foreach (SqlParameter param in @params) {
                        cmd.Parameters.Add(param);
                    }
                    rowsAffected = cmd.ExecuteNonQuery();                    
                });
            }
            return rowsAffected;
        }

        protected SqlParameter _P(string name, object value, object defIfNull = null) {
            if (value == null) {
                if (defIfNull == null) {
                    value = DBNull.Value;
                } else {
                    value = defIfNull;
                }
            } 
            return new SqlParameter(name, value);
        }

        protected SqlParameter ReturnParam(string name, SqlDbType type) {
            SqlParameter param = new SqlParameter(name, type);
            param.Direction = ParameterDirection.ReturnValue;
            return param;
        }

        private void Message(string format, params object[] args) {
            if (ServiceMessage != null) {
                ServiceMessage(String.Format(format, args));
            }
        }

        protected HashSet<string> SplitCSV(string list) {
            String[] items = list.Split(',');
            HashSet<string> set = new HashSet<string>();
            foreach (string item in items) {
                if (item.StartsWith("'") && item.EndsWith("'")) {
                    set.Add(item.Substring(1, item.Length - 2));
                } else {
                    set.Add(item);
                }
            }

            return set;
        }

        #region Traits

        public TraitCategory GetTraitCategory(string category) {
            TraitCategory cat = null;
            int catId = StoredProcReturnVal<int>("spTraitCategoryGetSet",_P("vchrTraitCategory", category), ReturnParam("intTraitCategoryID", SqlDbType.Int));
            cat = new TraitCategory();
            cat.Category = category;
            cat.TraitCategoryID = catId;
            return cat;
        }

        public List<Trait> GetTraits(string category, int intraCategoryID) {
            var mapper = new GenericMapperBuilder<Trait>().Map("Trait", "Name").PostMapAction((t) => {
                t.Category = category;
            }).build();
            return StoredProcToList("spTraitList", mapper, _P("vchrCategory", category), _P("vchrIntraCatID", intraCategoryID+""));
        }

        public List<String> GetTraitDistinctValues(string traitName, string category) {
            var results = new List<string>();
            StoredProcReaderForEach("spTraitDistinctValues", (reader) => {
                results.Add(reader[0] as string);
            }, _P("vchrTraitType", traitName), _P("vchrCategory", category)); 

            return results;
        }

        public void DeleteTrait(int traitId) {
            StoredProcUpdate("spTraitDelete", _P("intTraitID", traitId));
        }

        public List<String> GetTraitNamesForCategory(string traitCategory) {
            var results = new List<string>();
            StoredProcReaderForEach("spTraitTypeListForCategory", (reader) => {
                results.Add(reader["Trait"] as string);
            }, _P("vchrCategory", traitCategory));
            return results;
        }

        public int InsertOrUpdateTrait(Trait trait) {
            if (trait.TraitID < 0) {
                var retval = ReturnParam("NewTraitId", SqlDbType.Int);
                StoredProcUpdate("spTraitInsert",
                    _P("vchrCategory", trait.Category),
                    _P("intIntraCatID", trait.IntraCatID),
                    _P("vchrTrait", trait.Name),
                    _P("vchrValue", trait.Value ?? ""),
                    _P("vchrComment", trait.Comment ?? ""),
                    retval);
                return (int)retval.Value;
            } else {
                StoredProcUpdate("spTraitUpdate",
                    _P("intTraitID", trait.TraitID),
                    _P("vchrCategory", trait.Category),
                    _P("vchrTrait", trait.Name),
                    _P("vchrValue", trait.Value),
                    _P("vchrComment", trait.Comment));

                return trait.TraitID;
            }
        }

        #endregion


        #region Multimedia

        public List<MultimediaLink> GetMultimediaItems(string category, int intraCatID) {
            var mapper = new GenericMapperBuilder<MultimediaLink>().Map("FileExtension", "Extension").build();
            List<MultimediaLink> ret = StoredProcToList("spMultimediaList", mapper, _P("vchrCategory", category), _P("intIntraCatID", intraCatID));
            return ret;
        }

        public byte[] GetMultimediaBytes(int mediaId) {
            byte[] ret = null;
            StoredProcReaderFirst("spMultimediaGetOne", (reader) => {
                var x = reader.GetSqlBinary(0);
                ret = x.Value;
            }, _P("intMultimediaID", mediaId));
            return ret;
        }

        public List<MultimediaType> GetMultimediaTypes() {
            var mapper = new GenericMapperBuilder<MultimediaType>().Map("MultimediaType", "Name").build();
            return StoredProcToList("spMultimediaTypeList", mapper);
        }

        public Multimedia GetMultimedia(int mediaID) {
            var mapper = new GenericMapperBuilder<Multimedia>().build();
            Multimedia ret = null;
            StoredProcReaderFirst("spMultimediaGet", (reader) => {
                ret = mapper.Map(reader);
            });
            return ret;
        }

        #endregion
        /// <summary>
        /// Holds user credentials, and is the conduit to gaining a Connection object
        /// </summary>
        public User User { get; private set; }

        public event ServiceMessageDelegate ServiceMessage;

        public delegate void ServiceCommandAction(SqlConnection connection, SqlCommand command);

        public delegate void ServiceReaderAction(SqlDataReader reader);

        public delegate void ServiceMessageDelegate(string message);

    }

}
