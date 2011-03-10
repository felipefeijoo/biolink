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
using System.ComponentModel;
using BioLink.Data;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for CellEditorWindow.xaml
    /// </summary>
    public partial class CellEditorWindow : Window, IDataErrorInfo {

        public CellEditorWindow(ImportFieldMapping mapping, string currentValue) {
            InitializeComponent();
            this.Mapping = mapping;
            this.CurrentValue = currentValue;
            this.NewValue = currentValue;

            this.DataContext = this;

            Loaded += new RoutedEventHandler(CellEditorWindow_Loaded);
            
        }

        void CellEditorWindow_Loaded(object sender, RoutedEventArgs e) {
            txtNewValue.Focus();
        }

        public String CurrentValue { get; private set; }
        public String NewValue { get; set; }
        public ImportFieldMapping Mapping { get; private set; }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Close();
        }

        public string Error {
            get { return null; }
        }

        public string this[string columnName] {
            get {
                if (columnName == "NewValue") {
                    var service = new ImportService(PluginManager.Instance.User);
                    var result = service.ValidateImportValue(Mapping.TargetColumn, NewValue);
                    if (!result.IsValid) {
                        return result.Message;
                    }
                }

                return null;
            }
        }
    }
    
}
