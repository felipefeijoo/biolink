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
            PreviewDataSet();
        }

        private object BuildColumnHeader(String name) {
            var t = new TextBlock();
            t.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            t.TextAlignment = TextAlignment.Left;            
            t.Text = name;
            return t;
        }

    }
    
}
