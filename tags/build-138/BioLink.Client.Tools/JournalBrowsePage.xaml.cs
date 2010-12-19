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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for JournalBrowsePage.xaml
    /// </summary>
    public partial class JournalBrowsePage : UserControl {

        private ObservableCollection<JournalViewModel> _model;

        public JournalBrowsePage() {
            InitializeComponent();
        }

        public JournalBrowsePage(User user, string range) {
            InitializeComponent();
            this.Range = range;
            this.User = user;

            lblPageHeader.Content = string.Format("Journals - {0}", range);

            lst.SelectionChanged += new SelectionChangedEventHandler(lst_SelectionChanged);
        }

        public void LoadPage() {
            string[] bits = Range.Split('-');
            _model = null;
            if (bits.Length == 2) {
                string from = bits[0];
                string to = bits[1];

                var service = new SupportService(User);
                string where = "((vchrAbbrevName is null or ltrim(rtrim(vchrAbbrevName)) = '') and Left(vchrFullName," + from.Length + ") between '" + from + "' and '" + to + "') or (Left(vchrAbbrevName," + from.Length + ") between '" + from + "' and '" + to + "')";
                var list = service.ListJournalRange(where);
                _model = new ObservableCollection<JournalViewModel>(list.ConvertAll((model) => {
                    return new JournalViewModel(model);
                }));
                
            }
            lst.ItemsSource = _model;

        }

        public void Clear() {
            _model.Clear();
        }

        protected String Range { get; private set; }

        public User User { get; private set; }

        void lst_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            
        }
    }
}
