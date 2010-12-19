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
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for JournalManager.xaml
    /// </summary>
    public partial class JournalManager : DatabaseActionControl {

        private ObservableCollection<JournalViewModel> _findModel;

        private TabItem _previousPage;

        #region designer ctor
        public JournalManager() {
            InitializeComponent();
        }
        #endregion

        public JournalManager(User user, ToolsPlugin owner) : base(user, "JournalManager") {
            InitializeComponent();
            this.Owner = owner;

            string[] ranges = new string[] { "A-C", "D-F", "G-I", "J-L", "M-O", "P-R", "S-U", "V-X", "Y-Z" };

            foreach (string range in ranges) {
                AddTabPage(range);
            }

        }

        private void AddTabPage(string range) {

            string[] bits = range.Split('-');
            if (bits.Length == 2) {
                char from = bits[0][0];
                char to = bits[1][0];
                string caption = "";
                for (char ch = from; ch <= to; ch++) {
                    caption += ch;
                }

                TabItem item = new TabItem();
                item.Header = caption.ToUpper();
                item.Tag = range;
                item.Content = new JournalBrowsePage(User, range);

                item.RequestBringIntoView += new RequestBringIntoViewEventHandler(item_RequestBringIntoView);
                item.LayoutTransform = new RotateTransform(90);
                tabPages.Items.Add(item);

            } else {
                throw new Exception("Invalid page range!: " + range);
            }

        }

        void item_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {

            if (_previousPage != null && _previousPage != sender) {
                var page = _previousPage.Content as JournalBrowsePage;
                if (page != null) {
                    page.Clear();
                }
            }

            var selected = sender as TabItem;
            if (selected != null) {
                var page = selected.Content as JournalBrowsePage;
                if (page != null) {
                    selected.InvokeIfRequired(() => {
                        page.LoadPage();
                    });
                }
            }
            _previousPage = selected;
            
        }

        public ToolsPlugin Owner { get; private set; }

        private void txtFind_TypingPaused(string text) {
            if (string.IsNullOrEmpty(text)) {
                _findModel = null;
            } else {
                var service = new SupportService(User);
                var list = service.FindJournals(text);
                _findModel = new ObservableCollection<JournalViewModel>(list.ConvertAll((model) => {
                    return new JournalViewModel(model);
                }));
            }

            lstResults.ItemsSource = _findModel;
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {

        }

        private void btnProperties_Click(object sender, RoutedEventArgs e) {

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {

        }

    }
}
