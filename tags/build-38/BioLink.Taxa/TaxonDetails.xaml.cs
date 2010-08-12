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
using BioLink.Data.Model;
using System.Reflection;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for TaxonDetails.xaml
    /// </summary>
    public partial class TaxonDetails : Window {

        public TaxonDetails() {
            InitializeComponent();
        }

        public TaxonDetails(Taxon taxon) {
            InitializeComponent();
            this.Taxon = taxon;
            // Build dynamic content...

            PropertyInfo[] props = Taxon.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in props) {
                Grid item = new Grid();
                item.ColumnDefinitions.Add(new ColumnDefinition());
                item.ColumnDefinitions.Add(new ColumnDefinition());
                TextBlock block = new TextBlock();

                block.Text = prop.Name + ":";                
                item.Children.Add(block);
                Grid.SetColumn(block, 0);

                TextBox txt = new TextBox();
                object value = prop.GetValue(Taxon, null);
                txt.Text = (value == null ? "" : value.ToString());

                item.Children.Add(txt);
                Grid.SetColumn(txt, 1);

                this.contentStack.Children.Add(item);

            }

        }

        #region properties

        public Taxon Taxon { get; private set; }

        #endregion

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

    }
}
