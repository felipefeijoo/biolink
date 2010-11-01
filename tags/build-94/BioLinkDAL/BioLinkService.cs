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

        protected void SQLReaderForEach(string SQL, ServiceReaderAction action, params SqlParameter[] @params) {
            Message("Executing query...");
            using (new CodeTimer(String.Format("SQLReaderForEach '{0}'", SQL))) {
                Logger.Debug("Calling stored procedure (reader): {0}", SQL);
                Command((con, cmd) => {
                    cmd.CommandText = SQL;
                    cmd.CommandType = System.Data.CommandType.Text;
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

        public List<String> GetDistinctValues(string table, string field) {
            var results = new List<string>();
            StoredProcReaderForEach("spSelectDistinct", (reader) => {
                results.Add(reader[0] as string);
            }, _P("vchrTableName", table), _P("vchrFieldName", field));

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
            var mapper = new GenericMapperBuilder<Multimedia>().Map("vchrname", "Name").build();
            Multimedia ret = null;
            StoredProcReaderFirst("spMultimediaGet", (reader) => {
                ret = mapper.Map(reader);
                ret.MultimediaID = mediaID;
            }, _P("intMMID", mediaID));
            return ret;
        }

        public void DeleteMultimediaLink(int? multimediaLinkId) {
            StoredProcUpdate("spMultimediaLinkDelete", _P("intMultimediaLinkID", multimediaLinkId.Value));
        }

        public int InsertMultimedia(string name, string extension, byte[] bytes) {
            var retval = ReturnParam("NewMultimediaID", SqlDbType.Int);
            StoredProcUpdate("spMultimediaInsert", _P("vchrName", name), _P("vchrFileExtension", extension), _P("intSizeInBytes", bytes.Length), retval);
            // Now insert the actual bytes...
            UpdateMultimediaBytes((int) retval.Value, bytes);

            return (int) retval.Value;
        }

        public int InsertMultimediaLink(string category, int intraCatID, string multimediaType, int multimediaID, string caption) {
            var retval = ReturnParam("[NewMultimediaLinkID]", SqlDbType.Int);

            if (multimediaType == null) {
                multimediaType = "";
            }

            StoredProcUpdate("spMultimediaLinkInsert",
                _P("vchrCategory", category.ToString()),
                _P("intIntraCatID", intraCatID),
                _P("vchrMultimediaType", multimediaType),
                _P("intMultimediaID", multimediaID),
                _P("vchrCaption", caption),
                retval);

            return (int) retval.Value;
        }

        public void UpdateMultimedia(int multimediaId, string name, string number, string artist, string dateRecorded, string owner, string copyright) {

            StoredProcUpdate("spMultimediaUpdateLong",
                _P("intMultimediaID", multimediaId),
                _P("vchrName", name),
                _P("vchrNumber", number),
                _P("vchrArtist", artist),
                _P("vchrDateRecorded", dateRecorded),
                _P("vchrOwner", owner),
                _P("txtCopyright", copyright)
            );

        }

        public void UpdateMultimediaBytes(int? multimediaId, byte[] bytes) {
            // Multimedia is the only place where we don't have a stored procedure for the insert/update. This is probably due to a 
            // limitation of ADO.NET or SQL Server back in the 90's or something like that. Either way, we need to insert the actual blob
            // "manually"...
            Command((conn, cmd) => {

                if (User.InTransaction && User.CurrentTransaction != null) {
                    cmd.Transaction = User.CurrentTransaction;
                }

                cmd.CommandText = "UPDATE [tblMultimedia] SET imgMultimedia = @blob, intSizeInBytes=@size WHERE intMultimediaID = @multimediaId";
                cmd.Parameters.Add(_P("blob", bytes));
                cmd.Parameters.Add(_P("size", bytes.Length));
                cmd.Parameters.Add(_P("multimediaId", (int)multimediaId));
                cmd.ExecuteNonQuery();
            });

        }

        public Multimedia FindDuplicateMultimedia(System.IO.FileInfo file, out int sizeInBytes) {
            var name = file.Name;
            if (file.Name.Contains(".")) {
                name = file.Name.Substring(0, file.Name.LastIndexOf("."));
            }
            sizeInBytes = 0;
            var extension = file.Extension.Substring(1);
            // Not that the following service all returns partial results (incomplete stored proc), so a two stage approach is required.
            // First we find candidates based on name.
            var candidates = FindMultimediaByName(name);
            if (candidates.Count > 0) {                
                // Look for matching names and extensions...
                foreach (Multimedia candidate in candidates) {
                    if (candidate.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) && candidate.FileExtension.Equals(extension, StringComparison.CurrentCultureIgnoreCase)) {
                        // Now we do a deeper analysis of each matching candidate, checking the filelength. Theoretically, if we kept a hash on the content in the database
                        // we should compare that, but for now we'll use name and size.
                        sizeInBytes = GetMultimediaSizeInBytes(candidate.MultimediaID);
                        if (sizeInBytes > -1) {
                            if (sizeInBytes == file.Length) {
                                return GetMultimedia(candidate.MultimediaID);
                            }
                        } else {
                            throw new Exception("Failed to get size of multimedia " + candidate.MultimediaID);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// TODO: should probably be made into a stored proc!
        /// </summary>
        /// <param name="multimediaId"></param>
        /// <returns></returns>
        public int GetMultimediaSizeInBytes(int multimediaId) {
            int result = -1;
            SQLReaderForEach("SELECT DATALENGTH(imgMultimedia) FROM [tblMultimedia] where intMultimediaID = @mmid", (reader) => {
                result = (int) reader[0];
            }, _P("mmid", multimediaId));

            return result;
        }

        public List<Multimedia> FindMultimediaByName(string name) {
            string searchTerm = name.Replace("*", "%") + "%";
            var mapper = new GenericMapperBuilder<Multimedia>().Map("Extension", "FileExtension").build();
            List<Multimedia> results = StoredProcToList("spMultimediaFindByName", mapper, _P("txtSearchTerm", searchTerm));
            return results;
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
