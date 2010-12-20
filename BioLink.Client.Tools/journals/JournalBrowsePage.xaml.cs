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
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;
using BioLink.Client.Utilities;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for JournalBrowsePage.xaml
    /// </summary>
    public partial class JournalBrowsePage : UserControl {

        private ObservableCollection<JournalViewModel> _model = new ObservableCollection<JournalViewModel>();

        public JournalBrowsePage() {
            InitializeComponent();
        }

        public JournalBrowsePage(User user) {
            InitializeComponent();
            this.User = user;
            lst.ItemsSource = _model;
        }

        public void LoadPage(string range) {

            lblPageHeader.Content = string.Format("Journals - {0}", range);

            string[] bits = range.Split('-');
            _model.Clear();
            if (bits.Length == 2) {
                string from = bits[0];
                string to = bits[1];

                var service = new SupportService(User);
                string where = "((vchrAbbrevName is null or ltrim(rtrim(vchrAbbrevName)) = '') and Left(vchrFullName," + from.Length + ") between '" + from + "' and '" + to + "') or (Left(vchrAbbrevName," + from.Length + ") between '" + from + "' and '" + to + "')";
                var list = service.ListJournalRange(where);
                foreach (Journal j in list) {
                    _model.Add(new JournalViewModel(j));
                }                
            }
            
        }

        public void Clear() {
            _model.Clear();
        }

        public User User { get; private set; }

        public JournalViewModel SelectedItem {
            get { return lst.SelectedItem as JournalViewModel; }
        }

        public ObservableCollection<JournalViewModel> Model { get { return _model; } }
    }
}
