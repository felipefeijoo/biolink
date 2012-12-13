using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Windows;

namespace BioLink.Client.BVPImport {

    public class BVPZipImportFilter : TabularDataImporter {

        private BVPImportOptions _options;

        public override bool GetOptions(System.Windows.Window parentWindow, ImportWizardContext context) {
            var frm = new BVPImportOptionsWindow(PluginManager.Instance.User, _options);
            frm.Owner = parentWindow;
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (frm.ShowDialog().GetValueOrDefault(false)) {
                _options = new BVPImportOptions { Filename = frm.Filename, RowSource = frm.RowSource };
                if (context.FieldMappings == null || context.FieldMappings.Count() == 0) {
                    PreloadMappings(context);
                }
                return true;
            }

            return false;
        }

        private void PreloadMappings(ImportWizardContext context) {
            //foreach (ImportFieldMappingViewModel mapping in _model) {
            //    if (string.IsNullOrEmpty(mapping.TargetColumn)) {
            //        var candidate = _fields.Find((field) => {
            //            if (!string.IsNullOrEmpty(mapping.SourceColumn)) {
            //                // First try a simple match of the name...
            //                if (field.DisplayName.Equals(mapping.SourceColumn, StringComparison.CurrentCultureIgnoreCase)) {
            //                    return true;
            //                };
            //                // Next convert all underscores to spaces and try that....

            //                var test = mapping.SourceColumn.Replace("_", " ");
            //                if (field.DisplayName.Equals(test, StringComparison.CurrentCultureIgnoreCase)) {
            //                    return true;
            //                }
            //            }
            //            return false;
            //        });
            //        if (candidate != null) {
            //            mapping.TargetColumn = string.Format("{0}.{1}", candidate.Category, candidate.DisplayName);
            //            mapping.DefaultValue = null;
            //        }
            //    }
            //}
        }

        public override ImportRowSource CreateRowSource() {
            if (_options != null) {
                if (_options.RowSource == null) {
                    var builder = new BVPImportSourceBuilder(_options.Filename);
                    _options.RowSource = builder.BuildRowSource();
                }
                return _options.RowSource;
            }
            return null;
        }

        public override string Name {
            get { return "ALA BVP Zip file"; }
        }

        public override string Description {
            get { return "Imports data exported from the Atlas of Living Australia's Biodiversity Volunteer Portal"; }
        }

        public override System.Windows.Media.Imaging.BitmapSource Icon {
            get {
                return ImageCache.GetPackedImage("images/ala_export.png", GetType().Assembly.GetName().Name);
            }
        }

        public override List<string> GetColumnNames() {
            var columns = new List<String>();
            if (_options != null) {

                if (_options.RowSource == null) {
                    var builder = new BVPImportSourceBuilder(_options.Filename);
                    _options.RowSource = builder.BuildRowSource();
                }

                for (int i = 0; i < _options.RowSource.ColumnCount; i++) {
                    columns.Add(_options.RowSource.ColumnName(i));
                }
            }

            return columns;
        }

        protected override void WriteEntryPoint(EntryPoint ep) {
            ep.AddParameter("Filename", _options.Filename);            

        }

        protected override void ReadEntryPoint(EntryPoint ep) {
            _options = new BVPImportOptions();
            _options.Filename = ep["Filename"];
        }
    }

    public class BVPImportOptions {

        public String Filename { get; set; }
        public ImportRowSource RowSource { get; set; }

    }
}
