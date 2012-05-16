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
using System.Data;
using System.Data.SqlClient;
using BioLink.Client.Utilities;
using System.Collections.Generic;
using System.Data.Common;

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
        protected BioLinkService(User user) {
            User = user;            
        }

        /// <summary>
        /// Constructs a new SqlCommand instance, and invokes the supplied ServiceCommandAction delegate with it.
        /// The command instance is automatically cleaned up once the action has been executed
        /// </summary>
        protected void Command(ServiceCommandAction action) {
            // If no action dont' bother with the connection or command
            if (action == null) {
                return;
            }

            // Get a connection
            
            var connection = User.GetConnection();
            
            // and create a command instance
            try {
                using (DbCommand command = connection.CreateCommand()) {

                    if (User.ConnectionProfile.Timeout.GetValueOrDefault(-1) > 0) {
                        if (User.ConnectionProfile.Timeout != null) {
                            command.CommandTimeout = User.ConnectionProfile.Timeout.Value;
                        }
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

        internal void SQLReaderForEach(string SQL, ServiceReaderAction action, params DbParameter[] @params) {
            Message("Executing query...");
            using (new CodeTimer(String.Format("SQLReaderForEach '{0}'", SQL))) {
                Logger.Debug("Calling stored procedure (reader): {0}({1})", SQL, GetParamsString(@params));
                Command((con, cmd) => {
                    cmd.CommandText = SQL;
                    cmd.CommandType = CommandType.Text;
                    if (User.InTransaction && User.CurrentTransaction != null) {
                        cmd.Transaction = User.CurrentTransaction;
                    }
                    foreach (DbParameter param in @params) {
                        cmd.Parameters.Add(param);
                    }

                    using (var reader = cmd.ExecuteReader()) {
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
        /// <param name="action">The action to take with the reader</param>
        /// <param name="params">A params array for the arguments of the stored proc</param>
        internal void StoredProcReaderForEach(string proc, ServiceReaderAction action, params DbParameter[] @params) {
            Message("Executing query...");
            using (new CodeTimer(String.Format("StoredProcReaderForEach '{0}'", proc))) {
                Logger.Debug("Calling stored procedure (reader): {0}({1})", proc, GetParamsString(@params));
                Command((con, cmd) => {
                    User.ConnectionProvider.StoredProcReaderForEach(User, cmd, proc, action, (msg) => Message(msg), @params);
                });
            }
        }

        internal void StoredProcReaderFirst(string proc, ServiceReaderAction action, params DbParameter[] @params) {
            using (new CodeTimer(String.Format("StoredProcReaderFirst '{0}'", proc))) {
                Logger.Debug("Calling stored procedure (reader): {0}({1})", proc, GetParamsString(@params));
                Command((con, cmd) => {
                    User.ConnectionProvider.StoredProcReaderFirst(User, cmd, proc, action, (msg) => Message(msg), @params);
                });
            }
        }

        internal T StoredProcReturnVal<T>(string proc, params DbParameter[] @params) {
            using (new CodeTimer(String.Format("StoredProcReaderFirst '{0}'", proc))) {
                
                Array.Resize(ref @params, @params.Length + 1);
                var ret = ReturnParam("Return Value", SqlDbType.Variant);
                @params[@params.Length - 1] = ret;

                Command((con, cmd) => {
                    cmd.CommandText = proc;
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (User.InTransaction && User.CurrentTransaction != null) {
                        cmd.Transaction = User.CurrentTransaction;
                    }
                    foreach (DbParameter param in @params) {
                        cmd.Parameters.Add(param);
                    }
                    cmd.ExecuteNonQuery();                    

                });

                return (T) ret.Value;
            }
        }

        internal List<T> StoredProcToList<T>(string storedproc, GenericMapper<T> mapper, params DbParameter[] @params) where T : new() {
            var list = new List<T>();

            StoredProcReaderForEach(storedproc, reader => list.Add(mapper.Map(reader)), @params);

            return list;
        }

        internal T StoredProcGetOne<T>(string storedproc, GenericMapper<T> mapper, params DbParameter[] @params) where T : new() {
            T ret = default(T);
            StoredProcReaderFirst(storedproc, reader => {
                ret = mapper.Map(reader);
            }, @params);
            return ret;
        }

        internal DataTable StoredProcDataTable(string proc, params DbParameter[] @params) {

            DataTable table = null;
            StoredProcReaderForEach(proc, reader => {

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

        public const string HIDDEN_COLUMN_PREFIX = "HIDDEN_";

        internal DataMatrix StoredProcDataMatrix(string proc, Dictionary<string, ColumnDataFormatter> formatterMap, params DbParameter[] @params) {
            return StoredProcDataMatrix(proc, formatterMap, null, @params);
        }

        public DataMatrix StoredProcDataMatrix(string proc, Dictionary<string, ColumnDataFormatter> formatterMap, List<MatrixColumn> additionalColumns, params DbParameter[] @params) {

            DataMatrix[] matrix = {null};
            ColumnDataFormatter[] formatters = null;

            var defaultFormatter = new ColumnDataFormatter((value, rdr) => value);

            StoredProcReaderForEach(proc, (reader) => {

                if (matrix[0] == null) {

                    // Set up formatter array...
                    formatters = new ColumnDataFormatter[reader.FieldCount];

                    matrix[0] = new DataMatrix();
                    for (int i = 0; i < reader.FieldCount; ++i) {
                        var columnName = reader.GetName(i);

                        bool hidden = false;
                        if (columnName.StartsWith(HIDDEN_COLUMN_PREFIX)) {
                            columnName = columnName.Substring(HIDDEN_COLUMN_PREFIX.Length);
                            hidden = true;
                        }

                        matrix[0].Columns.Add(new MatrixColumn { Name = columnName, IsHidden = hidden });
                        if (formatterMap != null && formatterMap.ContainsKey(columnName)) {
                            formatters[i] = formatterMap[columnName];
                        } else {
                            formatters[i] = defaultFormatter;
                        }
                    }
                }

                if (additionalColumns != null && additionalColumns.Count > 0) {
                    foreach (MatrixColumn col in additionalColumns) {
                        matrix[0].Columns.Add(col);
                    }
                }

                MatrixRow row = matrix[0].AddRow();
                for (int i = 0; i < reader.FieldCount; ++i) {
                    if (!reader.IsDBNull(i)) {                        
                        row[i] = formatters[i](reader[i], reader);
                    }
                }
                

            }, @params);

            if (matrix[0] == null) {
                matrix[0] = new DataMatrix();
            }

            return matrix[0];
        }

        private string GetParamsString(params DbParameter[] @params) {
            var plist = new List<string>();
            foreach (SqlParameter p in @params) {
                var pValue = p.Value == null ? "(null)" : p.Value.ToString().Truncate(50);
                plist.Add(p.ParameterName + "=" + pValue);
            }

            return plist.Join(",");

        }

        internal int StoredProcUpdate(string proc, params DbParameter[] @params) {
            int rowsAffected = -1;
            using (new CodeTimer(String.Format("StoredProcUpdate '{0}'", proc))) {

                Logger.Debug("Calling stored procedure (update): {0}({1})", proc, GetParamsString(@params));


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

        internal DbParameter _P(string name, object value, object defIfNull = null) {
            if (value == null) {
                if (defIfNull == null) {
                    value = DBNull.Value;
                } else {
                    value = defIfNull;
                }
            } 
            return new SqlParameter(name, value);
        }

        internal DbParameter ReturnParam(string name, SqlDbType type = SqlDbType.Int) {
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

        public string AsString(object obj, string @default = "") {
            if (obj != null && !DBNull.Value.Equals(obj)) {
                return obj.ToString();
            }
            return @default;
        }

        protected string EscapeSearchTerm(string searchTerm, bool appendWildcard = false) {
            if (string.IsNullOrWhiteSpace(searchTerm)) {
                if (appendWildcard) {
                    return "%";
                }
                return "";
            }

            var escaped = searchTerm.Replace('*', '%');
            escaped = escaped.Replace("[", "[[]");
            if (appendWildcard && !escaped.EndsWith("%")) {
                escaped += "%";
            }
            return escaped;
        }
     
        /// <summary>
        /// Holds user credentials, and is the conduit to gaining a Connection object
        /// </summary>
        public User User { get; private set; }

        public event ServiceMessageDelegate ServiceMessage;

    }

    public delegate void ServiceCommandAction(DbConnection connection, DbCommand command);

    public delegate void ServiceMessageDelegate(string message);

    public delegate void ServiceReaderAction(DbDataReader reader);

    public static class DataExtensions {
        /// <summary>
        /// Safe get return that attempts to coerce the value of the column specified by field into type T
        /// If the value is null or DBNull the default is returned.
        /// </summary>
        /// <typeparam name="T">The Type to coerce the value to</typeparam>
        /// <param name="reader">A sql reader</param>
        /// <param name="field">The name of the column to extract the value from</param>
        /// <param name="defvalue">A default value to use should the actual value be null or DBNull</param>
        /// <returns></returns>
        public static T Get<T>(this DbDataReader reader, string field, T defvalue = default(T)) {
            var value = reader[field];
            if (value == null || value == DBNull.Value) {
                return defvalue;
            }
            return (T)value;
        }

        public static int GetIdentityValue(this System.Data.Common.DbDataReader reader, int ordinal = 0, int @default = -1) {
            if (ordinal >= 0) {
                if (!reader.IsDBNull(ordinal)) {
                    var obj = reader[ordinal];
                    if (obj != null) {
                        if (typeof(Int32).IsAssignableFrom(obj.GetType())) {
                            return (Int32)reader[0];
                        } else if (typeof(decimal).IsAssignableFrom(obj.GetType())) {
                            return (int)(decimal)reader[0];
                        }
                    }
                }
            }
            return @default;
        }

    }

    public delegate object ColumnDataFormatter(object data, DbDataReader reader);

}
