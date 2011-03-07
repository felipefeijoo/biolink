using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace BioLink.Client.Extensibility {

    public class ImportRowSource {

        public ImportRowSource(ImportStagingService service, int? rowcount) {
            this.Reader = service.GetImportReader();
            this.RowCount = rowcount;
            this.Service = service;
        }

        public bool MoveNext() {
            return Reader.Read();
        }

        public object this[int index] {
            get { return Reader[index]; }
        }

        public object this[string columnname] {
            get { return Reader[columnname]; }
        }

        public int? RowCount { get; private set; }

        public int? ColumnCount {
            get { return Reader.FieldCount; }
        }

        public string ColumnName(int index) {
            return Reader.GetName(index);
        }

        public void CopyToErrorTable(string message) {
            Service.CopyToErrorTable(this, message);
        }

        protected SQLiteDataReader Reader { get; private set; }

        public ImportStagingService Service { get; private set; }
    
    }
}
