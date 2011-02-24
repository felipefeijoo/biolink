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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for ImportPage.xaml
    /// </summary>
    public partial class ImportPage : WizardPage {

        public ImportPage() {
            InitializeComponent();
        }

        public override string PageTitle {
            get {
                return "Import data";
            }
        }

        private void btnSaveTemplate_Click(object sender, RoutedEventArgs e) {
            var inifile = new IniFile();
            inifile.SetValue("Import", "ProfileStr", ImportContext.Importer.CreateProfileString());
            inifile.SetValue("Import", "ImportFilter", ImportContext.Importer.Name);
            inifile.SetValue("Import", "FieldCount", ImportContext.FieldMappings.Count());

            int i = 0;
            foreach (ImportFieldMapping mapping in ImportContext.FieldMappings) {
                var ep = new EntryPoint(mapping.SourceColumn);
                ep.AddParameter("Mapping", mapping.TargetColumn);
                ep.AddParameter("Default", mapping.DefaultValue);
                ep.AddParameter("IsFixed", mapping.IsFixed.ToString());

                inifile.SetValue("Mappings", string.Format("Field{0}", i++), ep.ToString());
            }

            inifile.Save("c:\\zz\\test.bip");
        }

        protected ImportWizardContext ImportContext { get { return WizardContext as ImportWizardContext; } }
    }
}
