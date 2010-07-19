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
using System.Collections.ObjectModel;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using System.Threading;

namespace BioLink.Client.Taxa {

    
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TaxonExplorer : UserControl {

        private ListView _searchResults;
        private Timer _timer;

        public TaxonExplorer() {
            InitializeComponent();

            _searchResults = new ListView();
            _searchResults.Margin = treeView.Margin;
            _searchResults.Visibility = Visibility.Hidden;
            allTaxaGrid.Children.Add(_searchResults);            

        }

        internal void SetModel(ObservableCollection<TaxonViewModel> model) {
            treeView.Items.Clear();            
            this.treeView.ItemsSource = model;
        }

        private void txtFind_TextChanged(object sender, TextChangedEventArgs e) {

            if (String.IsNullOrEmpty(txtFind.Text)) {                                
                treeView.Visibility = System.Windows.Visibility.Visible;                
                _searchResults.Visibility = Visibility.Hidden;
                _searchResults.Items.Clear();

            } else {

                _searchResults.Items.Clear();

                FindAsync();
                
                _searchResults.Items.Add("Searching...");

                treeView.Visibility = Visibility.Hidden;
                _searchResults.Visibility = Visibility.Visible;
                
            }
        }

        private void DoFind(object t) {
        }

        private void FindAsync() {
            string oldTerm = txtFind.Text;
            
            
            JobExecutor.QueueJob(() => {
                Thread.Sleep(1000);
                string currentTerm = null;
                this.InvokeIfRequired(() => {
                    currentTerm = txtFind.Text;
                });

                if (currentTerm != null && currentTerm.Equals(oldTerm)) {
                    MessageBox.Show("Search!" + currentTerm);
                }
            });
        }
    }
}
