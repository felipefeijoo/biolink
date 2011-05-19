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
using System.Collections.ObjectModel;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for LoanContactsControl.xaml
    /// </summary>
    public partial class LoanContactsControl : DatabaseActionControl {

        public LoanContactsControl(User user, ToolsPlugin plugin) : base(user, "LoanContactsManager") {
            InitializeComponent();
            this.Plugin = plugin;
            var list = Enum.GetValues(typeof(ContactSearchType));
            cmbFindWhat.ItemsSource = list;
            cmbFindWhat.SelectedIndex = 0;
            lvwFind.KeyUp += new KeyEventHandler(lvwFind_KeyUp);
            lvwFind.MouseDoubleClick += new MouseButtonEventHandler(lvwFind_MouseDoubleClick);

            txtFilter.PreviewKeyDown += new KeyEventHandler(txtFilter_PreviewKeyDown);
        }

        void lvwFind_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            EditSelectedContact();
        }

        void txtFilter_PreviewKeyDown(object sender, KeyEventArgs e) {             
            if (e.Key == Key.Down) {
                lvwFind.SelectedIndex = 0;
                lvwFind.Focus();
                e.Handled = true;
            }            
        }

        void lvwFind_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                EditSelectedContact();
            }
        }

        private void EditSelectedContact() {
            var selected = lvwFind.SelectedItem as ContactViewModel;
            if (selected != null) {
                var ctl = new ContactDetails(User, selected.ContactID);
                PluginManager.Instance.AddNonDockableContent(Plugin, ctl, "Contact details: " + selected.FullName, SizeToContent.Manual);
            }
        }

        protected ToolsPlugin Plugin { get; private set; }

        private void textBox1_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                DoFind();
            }
        }

        private void DoFind() {            
            var searchType = (ContactSearchType)cmbFindWhat.SelectedItem;
            var filter = txtFilter.Text;
            if (!string.IsNullOrWhiteSpace(filter)) {
                var service = new LoanService(User);
                var list = service.FindContacts(filter, searchType);
                lvwFind.ItemsSource = new ObservableCollection<ContactViewModel>(list.Select((m) => {
                    return new ContactViewModel(m);
                }));
            }
        }

        private void btnFind_Click(object sender, RoutedEventArgs e) {
            DoFind();
        }

    }

    public class ContactViewModel : GenericViewModelBase<Contact> {
        public ContactViewModel(Contact model) : base(model, () => model.ContactID) { }

        public override ImageSource Icon {
            get {
                if (base.Icon == null) {
                    return ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/Contact.png");
                }
                return base.Icon;
            }
            set {
                base.Icon = value;
            }
        }

        public int ContactID {
            get { return Model.ContactID; }
            set { SetProperty(() => Model.ContactID, value); }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string Title {
            get { return Model.Title; }
            set { SetProperty(() => Model.Title, value); }
        }

        public string GivenName {
            get { return Model.GivenName; }
            set { SetProperty(() => Model.GivenName, value); }
        }

        public string PostalAddress {
            get { return Model.PostalAddress; }
            set { SetProperty(()=>Model.PostalAddress, value); } 
        }

        public string StreetAddress {
            get { return Model.StreetAddress; }
            set { 
                SetProperty(() => Model.StreetAddress, value);
                if (_postalAsStreet) {
                    SetProperty(() => Model.PostalAddress, value);
                }
            }
        }

        public string Institution {
            get { return Model.Institution; }
            set { SetProperty(()=>Model.Institution, value); }
        }

        public string JobTitle {
            get { return Model.JobTitle; }
            set { SetProperty(() => Model.JobTitle, value); }
        }

        public string WorkPh {
            get { return Model.WorkPh; }
            set { SetProperty(() => Model.WorkPh, value); }
        }

        public string WorkFax {
            get { return Model.WorkFax; }
            set { SetProperty(() => Model.WorkFax, value); }
        }

        public string HomePh {
            get { return Model.HomePh; }
            set { SetProperty(() => Model.HomePh, value); }
        }

        public string EMail {
            get { return Model.EMail; }
            set { SetProperty(() => Model.EMail, value); }
        }

        public string FullName {
            get {
                var sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Title)) {
                    sb.Append(Title).Append(" ");
                }

                if (!string.IsNullOrWhiteSpace(GivenName)) {
                    sb.Append(GivenName).Append(" ");
                }

                sb.Append(Name);

                return sb.ToString();
            }
        }

        private bool _postalAsStreet;

        public bool PostalSameAsStreet {
            get { return _postalAsStreet; }
            set { 
                SetProperty("PostalSameAsStreet", ref _postalAsStreet, value);
                if (value) {
                    PostalAddress = StreetAddress;
                }
            }
        }

    }
}
