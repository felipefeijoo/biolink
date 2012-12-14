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

        public void Reset() {
            if (Reader != null && !Reader.IsClosed) {
                Reader.Close();
            }
            Reader = Service.GetImportReader();
        }

        public List<String> ColumnNames {
            get {
                var names = new List<String>();
                for (int i = 0; i < ColumnCount; ++i) {
                    names.Add(ColumnName(i));
                }
                return names;
            }
        }

        protected SQLiteDataReader Reader { get; private set; }

        public ImportStagingService Service { get; private set; }
    
    }
}
