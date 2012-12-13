using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using System.IO;
using Ionic.Zip;
using GenericParsing;

namespace BioLink.Client.BVPImport {
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class BVPImportOptionsWindow : Window {

        public BVPImportOptionsWindow() {
            InitializeComponent();
        }

        public BVPImportOptionsWindow(User user, BVPImportOptions options) {
            InitializeComponent();
            this.DataContext = this;
            this.User = user;
            this.Options = options;
            if (options != null && !String.IsNullOrEmpty(options.Filename)) {
                Filename = options.Filename;
            }
        }

        public User User { get; private set; }

        public BVPImportOptions Options { get; private set; }

        public String Filename { get; set; }
        public ImportRowSource RowSource { get; set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            Hide();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            Hide();
        }

        private void PreviewDataSet() {
            if (String.IsNullOrEmpty(Filename)) {
                return;
            }

            var builder = new BVPImportSourceBuilder(Filename);
            this.RowSource = builder.BuildRowSource();
            // make a matrix of the data - for now all of it, but if it becomes too much, we can limit it to top 100 or so...
            var matrix = new DataMatrix();
            var view = new GridView();
            for (int i = 0; i < RowSource.ColumnCount; ++i) {
                String name = RowSource.ColumnName(i);
                matrix.Columns.Add(new MatrixColumn { Name = name });
                var column = new GridViewColumn { Header = BuildColumnHeader(name), DisplayMemberBinding = new Binding(String.Format("[{0}]", i)) };
                view.Columns.Add(column);
            }

            while (RowSource.MoveNext()) {
                var row = matrix.AddRow();
                for (int i = 0; i < RowSource.ColumnCount; ++i) {
                    row[RowSource.ColumnName(i)] = RowSource[i];
                }
            }

            lvwPreview.ItemsSource = matrix.Rows;
            lvwPreview.View = view;
        }

        private void txtFilename_FileSelected(string filename) {
            this.Filename = filename;
        }

        private object BuildColumnHeader(String name) {
            var t = new TextBlock();
            t.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            t.TextAlignment = TextAlignment.Left;            
            t.Text = name;
            return t;
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e) {
            PreviewDataSet();
        }

    }

    public class BVPImportSourceBuilder {

        public String Filename { get; private set; }
        protected ImportStagingService Service { get; private set; }

        public BVPImportSourceBuilder(String filename) {
            this.Filename = filename;
            this.Service = new ImportStagingService();            
        }

        public ImportRowSource BuildRowSource() {            
            var rowCount = 0;

            var extra = new Dictionary<int,Dictionary<String,List<String>>>();

            // first define the columns...
            var columnBuilder = new ColumnDefinitionBuilder();            
            using (ZipFile zipfile = ZipFile.Read(Filename)) {
                foreach (ZipEntry entry in zipfile) {
                    if (entry.FileName.Equals("tasks.csv", StringComparison.OrdinalIgnoreCase)) {
                        GetTaskColumns(entry, columnBuilder);
                    } else if (entry.FileName.Equals("recordedBy.csv", StringComparison.OrdinalIgnoreCase)) {
                        AddToExtraData(entry, "recordedBy", extra);
                        var valueExtractor = new ANICCollectorNameFormattingValueExtractor();
                        columnBuilder.ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = "Collector(s)", SourceColumnName = "recordedBy", SourceFilename = entry.FileName, ValueExtractor = valueExtractor });
                    }
                }
            }
            // Now we have all the columns, we can create the staging table
            Service.BeginTransaction();
            Service.CreateImportTable(columnBuilder.ColumnNames);

            // And get the values...            
            using (ZipFile zipfile = ZipFile.Read(Filename)) {
                foreach (ZipEntry entry in zipfile) {
                    if (entry.FileName.Equals("tasks.csv")) {
                        IterateOverCSVZipEntry(entry, (parser) => {
                            var taskId = Int32.Parse(parser["taskID"]);
                            Dictionary<String, List<String>> fieldMap = null;
                            if (extra.ContainsKey(taskId)) {
                                fieldMap = extra[taskId];
                            }
                            var values = new List<String>();
                            columnBuilder.ColumnDefinitions.ForEach((coldef) => {
                                var value = coldef.ValueExtractor.ExtractValue(coldef, parser, fieldMap);
                                values.Add(value);                                
                            });                            
                            Service.InsertImportRow(values);
                            rowCount++;
                            return true;
                        });
                    }
                }
            }
            Service.CommitTransaction();
            return new ImportRowSource(Service, rowCount);
        }

        private void AddToExtraData(ZipEntry entry, String columnName, Dictionary<int, Dictionary<String, List<String>>> data) {
            var rowCount = 0;            
            IterateOverCSVZipEntry(entry, (parser) => {
                if (rowCount > 0) {                    
                    var taskId = Int32.Parse(parser["taskID"]);
                    if (!data.ContainsKey(taskId)) {
                        data[taskId] = new Dictionary<string, List<string>>();
                    }
                    var fieldMap = data[taskId];
                    if (!fieldMap.ContainsKey(columnName)) {
                        fieldMap[columnName] = new List<string>();
                    }
                    var fieldValues = fieldMap[columnName];
                    var fieldValue = parser[columnName];
                    fieldValues.Add(fieldValue);
                } 
                rowCount++;
                return true;
            });
        }

        private void GetTaskColumns(ZipEntry entry, ColumnDefinitionBuilder colummBuilder) {            
            IterateOverCSVZipEntry(entry, (parser) => {
                for (int i = 0; i < parser.ColumnCount; ++i) {                    
                    var dwcName = parser.GetColumnName(i);
                    colummBuilder.ProcessDwCField(dwcName, entry.FileName);
                }
                return false;
            });
        }

        protected String RegexCapture(BVPImportColumnDefinition colDef, GenericParserAdapter row, Dictionary<String, List<String>> extra) {
            return row[colDef.SourceColumnName];
        }

        protected void IterateOverCSVZipEntry(ZipEntry entry, Func<GenericParserAdapter, bool> func) {
            using (var reader = new StreamReader(entry.OpenReader())) {
                using (var parser = new GenericParserAdapter(reader)) {
                    parser.ColumnDelimiter = ',';
                    parser.FirstRowHasHeader = true;
                    parser.TextQualifier = '\"';
                    parser.FirstRowSetsExpectedColumnCount = true;
                    while (parser.Read()) {
                        if (func != null) {
                            if (!func(parser)) {
                                return;
                            }
                        }
                    }
                }
            }
        }

    }

    
}
