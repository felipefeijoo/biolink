using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using BioLink.Client.Utilities;
using System.IO;
using System.Data.Common;
using BioLink.Data.Model;
using System.Reflection;

namespace BioLink.Data {

    class SQLiteConnectionProvider : ConnectionProvider {

        public SQLiteConnectionProvider(Model.ConnectionProfile profile, String username, String password) : base(profile, username, password) { }

        public override System.Data.Common.DbConnection GetConnection(Model.ConnectionProfile profile, string username, string password) {

            bool isNew = false;
            if (!File.Exists(profile.Database)) {
                SQLiteConnection.CreateFile(profile.Database);                
                isNew = true;
            }
            var conn = new SQLiteConnection(String.Format("Data Source={0}", profile.Database));
            try {
                conn.Open();
            } catch (Exception ex) {
                conn.Dispose();
                conn = null;
                throw ex;
            }

            if (isNew) {
                CreateSchema(conn);
                conn.Close();
                return GetConnection(profile, username, password);
            }

            return conn;
        }

        private void CreateSchema(DbConnection conn) {
            CreateTable(conn, new TableOptions<Associate>());
            CreateTable(conn, new TableOptions<AutoNumber>());
            CreateTable(conn, new TableOptions<AvailableName>());
            CreateTable(conn, new TableOptions<CommonName>());
            CreateTable(conn, new TableOptions<Contact>());
            CreateTable(conn, new TableOptions<CurationEvent>());
            CreateTable(conn, new TableOptions<DistributionRegion>());
            CreateTable(conn, new TableOptions<GANIncludedSpecies>());
            CreateTable(conn, new TableOptions<GenusAvailableName>());
            CreateTable(conn, new TableOptions<Journal>());
            CreateTable(conn, new TableOptions<Kingdom>());
            CreateTable(conn, new TableOptions<LabelSet>());
            CreateTable(conn, new TableOptions<LabelSetItem>());
            CreateTable(conn, new TableOptions<Loan>());
            CreateTable(conn, new TableOptions<LoanCorrespondence>());
            CreateTable(conn, new TableOptions<LoanMaterial>());
            CreateTable(conn, new TableOptions<LoanReminder>());
            CreateTable(conn, new TableOptions<Material>());
            CreateTable(conn, new TableOptions<MaterialIdentification>());
            CreateTable(conn, new TableOptions<MaterialPart>());
            CreateTable(conn, new TableOptions<Multimedia> { });
            CreateTable(conn, new TableOptions<MultimediaLink>());
            CreateTable(conn, new TableOptions<Note> { });
            CreateTable(conn, new TableOptions<Phrase> { });
            CreateTable(conn, new TableOptions<PhraseCategory> { });
            CreateTable(conn, new TableOptions<Site>());
            CreateTable(conn, new TableOptions<SiteVisit>());
            CreateTable(conn, new TableOptions<Taxon> { TableName = "tblBiota", IDField="intTaxaID" });
            

        }

        private void CreateTable<T>(DbConnection conn, TableOptions<T> options = null) where T : BioLinkDataObject {
            Type t = typeof(T);
            if (options == null) {
                options = new TableOptions<T>();
            }

            var properties = t.GetProperties();
            var sb = new StringBuilder();
            sb.Append("CREATE TABLE ").Append(options.TableName).Append(" (\r\n");
            sb.Append("  ").Append(options.IDField).Append(" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,\r\n");            
            foreach (PropertyInfo prop in properties) {
                string type = "TEXT";
                string prefix = "";
                bool nullable = false;
                Type propertyType = prop.PropertyType;

                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    propertyType = propertyType.GetGenericArguments()[0];
                    nullable = true;
                }

                if (typeof(Boolean).IsAssignableFrom(propertyType)) {
                    type = "INTEGER";
                    prefix = "bit";
                } else if (typeof(int).IsAssignableFrom(propertyType)) {
                    type = "INTEGER";
                    prefix = "int";
                } else if (typeof(Int64).IsAssignableFrom(propertyType)) {
                    type = "INTEGER";
                    prefix = "int";
                } else if (typeof(Int32).IsAssignableFrom(propertyType)) {
                    type = "INTEGER";
                    prefix = "int";
                } else if (typeof(Double).IsAssignableFrom(propertyType)) {
                    type = "REAL";
                    prefix = "dbl";
                } else if (typeof(String).IsAssignableFrom(propertyType)) {
                    type = "TEXT";
                    prefix = "vchr";
                } else if (typeof(DateTime).IsAssignableFrom(propertyType)) {
                    type = "TEXT";
                    prefix = "dt";
                } else if (typeof(Guid).IsAssignableFrom(propertyType)) {
                    type = "GUID";
                    prefix = "";
                }

                string columnName = String.Format("{0}{1}", prefix, prop.Name);

                if (!columnName.Equals(options.IDField, StringComparison.CurrentCultureIgnoreCase)) {
                    sb.Append("  [").Append(columnName).Append("] ").Append(type);
                    if (!nullable) {
                        sb.Append(" NOT NULL ");  
                    }
                    sb.Append(",\r\n");
                }
            }

            sb.Remove(sb.Length - 3, 3);
            
            sb.Append(");");

            Console.WriteLine(sb.ToString());

            ExecuteNonQuery(conn, sb.ToString());
        }

        public override void StoredProcReaderForEach(User user, System.Data.Common.DbCommand command, string proc, ServiceReaderAction action, Action<string> message, params System.Data.Common.DbParameter[] @params) {
            
        }


        public override void StoredProcReaderFirst(User user, System.Data.Common.DbCommand command, string proc, ServiceReaderAction action, Action<string> message, params System.Data.Common.DbParameter[] @params) {
            
        }

        public override bool IsSysAdmin(User user) {
            return true;
        }


        public override int StoredProcUpdate(User user, System.Data.Common.DbCommand command, string proc, params System.Data.Common.DbParameter[] @params) {
            return 0;
        }

        public override DbParameter CreateParameter(string name, object value) {
            return new SQLiteParameter(name, value);
        }
    }

    public class TableOptions<T> where T : BioLinkDataObject {

        public TableOptions() {
            var t = typeof(T);
            TableName = "tbl" + t.Name;
            IDField = "int" + t.Name + "ID";
        }

        public String TableName { get; set; }
        public String IDField { get; set; }
    }

    [SQLiteFunction(Name = "BLTest", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class BLTest : SQLiteFunction {
        public override object Invoke(object[] args) {
            return Convert.ToString(args[0]) + "XXX";
        }
    }
}
