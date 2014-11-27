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
using System.Text.RegularExpressions;
using System.Windows;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Tools {

    public abstract class ImportProcessor {

        protected Regex _TraitRegex = new Regex("^(.*)[.]Other$");
        protected Regex _NoteRegex = new Regex("^(.*)[.]Notes$");
        protected Regex _MultimediaRegex = new Regex("^(.*)[.]Multimedia$");

        protected Dictionary<string, int> _fieldIndex = new Dictionary<string, int>();
        private int _errorCount;
        private int _successCount;

        protected ImportProcessor() {
            this.User = PluginManager.Instance.User;
            this.Service = new ImportService(User);
            this.MaterialService = new MaterialService(User);
            this.TaxaService = new TaxaService(User);
        }

        public void Import(Window parentWindow, ImportWizardContext context, IProgressObserver progress, Action<ImportStatusLevel, string> logFunc) {

            this.ParentWindow = parentWindow;
            this.Importer = context.Importer;
            this.Mappings = context.FieldMappings;
            this.Progress = progress;
            this.LogFunc = logFunc;


            if (Progress != null) {
                Progress.ProgressStart("Initialising...");
            }

            if (!InitImport()) {
                return;
            }

            ProgressMsg("Importing data - Stage 1", 0);
            if (!DoStage1()) {
                return;
            }

            CreateColumnIndexes();


            Cancelled = false;

            var connection = User.GetConnection();

            LogMsg("Importing data - Stage 2", 10);
            int rowCount = 0;
            int lastPercent = 0;

            DateTime timeStarted = DateTime.Now;
            DateTime lastCheck = timeStarted;

            RowSource.Reset();

            while (RowSource.MoveNext() && !Cancelled) {

                ImportCurrentRow(rowCount++, connection);
                
                var dblPercent = (double)((double)rowCount / (double)RowSource.RowCount) * 90;
                int percent = ((int)dblPercent) + 10 ;

                var timeSinceLastUpdate = DateTime.Now - lastCheck;
                
                if (percent != lastPercent || timeSinceLastUpdate.Seconds > 5) {
                    
                    var timeSinceStart = DateTime.Now - timeStarted;
                    
                    var avgMillisPerRow = (double)(timeSinceStart.TotalMilliseconds == 0 ? 1 : timeSinceStart.TotalMilliseconds) / (double) (rowCount == 0 ? 1 : rowCount);
                    var rowsLeft = RowSource.RowCount - rowCount;
                    var secondsLeft = (int) ((double) (rowsLeft * avgMillisPerRow) / 1000.0);

                    lastCheck = DateTime.Now;

                    TimeSpan ts = new TimeSpan(0, 0, secondsLeft);

                    var message = string.Format("Importing rows - Stage 2 ({0} of {1}). {2} remaining.", rowCount, RowSource.RowCount, ts.ToString());
                    ProgressMsg(message, percent);
                    lastPercent = percent;
                }
            }

            ProgressMsg("Importing rows - Stage 2 Complete", 100);

            LogMsg("{0} Rows successfully imported, {1} rows failed with errors", _successCount, _errorCount);

            LogMsg("Cleaning up staging database...");

            if (RowSource.Service.GetErrorCount() > 0) {

                if (ParentWindow.Question("Errors were encountered during the import. Rejected rows can be corrected and re-imported by re-running the import wizard and selecting the 'Import Error Database' option. Would you like to save the rejected rows so that they can be corrected and re-imported?", "Save rejected rows?", System.Windows.MessageBoxImage.Exclamation)) {

                    // Clean out just the imported records, leaving just the error rows...
                    LogMsg("Purging successfully imported rows from staging database...");
                    RowSource.Service.PurgeImportedRecords();
                    LogMsg("Saving mapping information to staging database...");
                    RowSource.Service.SaveMappingInfo(Importer, Mappings);
                    LogMsg("Disconnecting from staging database...");
                    RowSource.Service.Disconnect();

                    var dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.Filter = "SQLite database files|*.sqlite|All files (*.*)|*.*";
                    dlg.Title = "Save error database file";
                    if (dlg.ShowDialog().ValueOrFalse()) {
                        LogMsg("Copying staging database from {0} to {1}", RowSource.Service.FileName, dlg.FileName);
                        System.IO.File.Copy(RowSource.Service.FileName, dlg.FileName, true);
                    }
                }

            }

            connection.Close();

        }

        private bool DoStage1() {

            LogMsg("Stage 1 - Preparing staging database");
            LogMsg("Stage 1 - Populating staging database...");
            this.RowSource = Importer.CreateRowSource(Progress);
            LogMsg("Stage 1 Complete, {0} rows staged for import.", RowSource.RowCount);

            return true;
        }

        private bool InitImport() {
            LogMsg("Initializing...");

            InitImportImpl();

            _errorCount = 0;
            _successCount = 0;

            LogMsg("Initialisation complete");
            return true;
        }

        private void CreateColumnIndexes() {

            LogMsg("Caching column mappings...");

            _fieldIndex.Clear();

            foreach (ImportFieldMapping mapping in Mappings) {

                if (mapping.TargetColumn == null) {
                    continue;
                }

                string target = mapping.TargetColumn.ToLower();
                int index = -1;

                for (int i = 0; i <= RowSource.ColumnCount - 1; ++i) {
                    var sourceColumnName = RowSource.ColumnName(i);
                    if (sourceColumnName.Equals(mapping.SourceColumn, StringComparison.CurrentCultureIgnoreCase)) {
                        index = i;
                        break;
                    }
                }
                if (index >= 0) {
                    _fieldIndex[target] = index;
                    if (!string.IsNullOrWhiteSpace(mapping.TargetColumn)) {
                        LogMsg("{0} mapped to {1} (index {2})", mapping.TargetColumn, mapping.SourceColumn, index);
                    } else {
                        LogMsg("Source column {1} (index {2}) is not mapped to a target column.", mapping.TargetColumn, mapping.SourceColumn, index);
                    }
                } 
            }

        }


        protected void ImportCurrentRow(int rowId, System.Data.Common.DbConnection connection) {
            try {
                User.BeginTransaction(connection);

                ImportRowImpl(rowId, connection);

                // If we get here we can commit the transacton....
                User.CommitTransaction();

                _successCount++;

            } catch (Exception ex) {
                Error("Error on Row {0}: {1}", rowId, ex.Message);
                // Roll back the transaction....
                User.RollbackTransaction();
                //  Mark the import row as failed
                RowSource.CopyToErrorTable(ex.Message);
                _errorCount++;
            } 

        }


        protected abstract void ImportRowImpl(int rowId, System.Data.Common.DbConnection connection);

        protected abstract void InitImportImpl();

        protected abstract void RollbackRow(int rowId);

        protected void ProgressMsg(string message, double? percent = null) {
            if (Progress != null) {
                Progress.ProgressMessage(message, percent);
            }
        }

        protected void LogMsg(String format, params object[] args) {
            if (LogFunc != null) {
                if (args.Length > 0) {
                    LogFunc(ImportStatusLevel.Info, string.Format(format, args));
                } else {
                    LogFunc(ImportStatusLevel.Info, format);
                }
            }
        }

        protected string Get(string field, string def = "") {
            var key = field.ToLower();
            if (_fieldIndex.ContainsKey(key)) {
                var index = _fieldIndex[key];
                var mapping = Mappings.ElementAt(index);
                Object value = null;
                if (!mapping.IsFixed) {
                    value = RowSource[index];
                    if (value != null && value is String) {
                        value = ((String)value).Replace("\\n", "\n");
                    }
                } else {
                    value = mapping.DefaultValue;
                }

                var defaultValue = string.IsNullOrWhiteSpace(mapping.DefaultValue) ? def : mapping.DefaultValue;
                String finalValue = value == null ? defaultValue : value.ToString();

                if (mapping.Transformer != null) {
                    finalValue = mapping.Transformer.transform(finalValue, RowSource);
                }

                return finalValue;
            }

            return def;
        }

        protected string BuildDate(string date) {

            if (date.Length < 0) {
                date = date.PadRight(8, '0');
            }

            if (string.IsNullOrWhiteSpace(date)) {
                return "";
            }

            if (date.IsNumeric()) {
                return DateUtils.BLDateToStr(Int32.Parse(date));
            }

            return "";
        }

        protected void Error(string format, params object[] args) {
            if (LogFunc != null) {
                if (args.Length == 0) {
                    LogFunc(ImportStatusLevel.Error, format);
                } else {
                    LogFunc(ImportStatusLevel.Error, string.Format(format, args));
                }
            }
        }

        protected double? GetDouble(string field, double? @default = null) {
            var str = Get(field);
            if (!string.IsNullOrWhiteSpace(str)) {
                double val = 0;
                if (double.TryParse(str, out val)) {
                    return val;
                }
                throw new Exception(string.Format("Expected a double value for field {0}, got {1}", field, str));
            }
            return @default;
        }


        protected T GetConvert<T>(string field, T def = default(T), bool throwIfFail = true) {
            var strValue = Get(field);
            if (!string.IsNullOrWhiteSpace(strValue)) {
                var result = Service.ValidateImportValue(field, strValue, throwIfFail);
                if (result.IsValid) {
                    return result.ConvertedValue == null ? def : (T)result.ConvertedValue;
                } else {
                    return def;
                }
            }
            return def;
        }

        protected void InsertOneToManys(string category, int id) {
            InsertTraits(category, id);
            InsertNotes(category, id);
            InsertMultimedia(category, id);
        }

        private void InsertOneToMany(String category, int id, Regex regex, Action<string, int, string, string> importMethod) {
            foreach (ImportFieldMapping mapping in Mappings) {
                if (!string.IsNullOrWhiteSpace(mapping.TargetColumn)) {
                    var match = regex.Match(mapping.TargetColumn);
                    if (match.Success) {
                        var candiateCategory = match.Groups[1].Value;
                        if (!string.IsNullOrWhiteSpace(candiateCategory) && candiateCategory.Equals(category, StringComparison.CurrentCultureIgnoreCase)) {
                            var valueObj = RowSource[mapping.SourceColumn];
                            string strValue = null;
                            if (valueObj != null) {
                                strValue = valueObj.ToString();
                            }

                            if (string.IsNullOrWhiteSpace(strValue)) {
                                strValue = mapping.DefaultValue;
                            }

                            if (!string.IsNullOrWhiteSpace(strValue)) {
                                importMethod(category, id, mapping.SourceColumn, strValue);
                            }
                        }
                    }
                }
            }

        }

        protected void InsertMultimedia(string category, int id) {
            InsertOneToMany(category, id, _MultimediaRegex, (cat, intraCatId, columnName, value) => {
                Service.ImportMultimedia(cat, intraCatId, columnName, value);
            });
        }

        protected void InsertNotes(string category, int id) {
            InsertOneToMany(category, id, _NoteRegex, (cat, intraCatId, columnName, value) => {
                Service.ImportNote(cat, intraCatId, columnName, value);
            });
        }

        protected void InsertTraits(string category, int id) {
            InsertOneToMany(category, id, _TraitRegex, (cat, intraCatId, columnName, value) => {
                Service.ImportTrait(cat, intraCatId, columnName, value);
            });
        }

        protected Window ParentWindow { get; set; }

        protected MaterialService MaterialService { get; private set; }

        protected ImportService Service { get; private set; }

        protected TaxaService TaxaService { get; private set; }

        protected IProgressObserver Progress { get; private set; }

        protected Action<ImportStatusLevel, string> LogFunc { get; private set; }

        protected TabularDataImporter Importer { get; private set; }

        protected IEnumerable<ImportFieldMapping> Mappings { get; private set; }

        protected ImportRowSource RowSource { get; private set; }

        protected User User { get; private set; }

        public bool Cancelled { get; set; }

    }

}
