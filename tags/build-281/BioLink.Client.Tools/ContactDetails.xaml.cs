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


namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for ContactDetails.xaml
    /// </summary>
    public partial class ContactDetails : DatabaseActionControl {

        private ContactViewModel _viewModel;
        private int _contactId;

        public ContactDetails(User user, int contactId) : base(user, "LoanContact:" + contactId) {
            InitializeComponent();
            _contactId = contactId;
            this.ChangeContainerSet += new Action(ContactDetails_ChangeContainerSet);

        }

        void ContactDetails_ChangeContainerSet() {
            Contact model = null;
            if (_contactId < 0) {
                model = new Contact();
                model.ContactID = _contactId;
                RegisterUniquePendingChange(new InsertContactAction(model));
            } else {
                var service = new LoanService(User);
                model = service.GetContact(_contactId);
            }

            _viewModel = new ContactViewModel(model);

            if (model != null && !string.IsNullOrEmpty(model.StreetAddress)) {
                if (model.StreetAddress.Equals(model.PostalAddress, StringComparison.CurrentCultureIgnoreCase)) {
                    _viewModel.PostalSameAsStreet = true;
                }
            }

            tabContact.AddTabItem("Traits", new TraitControl(User, TraitCategoryType.Contact, _viewModel));
            tabContact.AddTabItem("Notes", new NotesControl(User, TraitCategoryType.Contact, _viewModel));

            txtInstituion.BindUser(User, "tblContact", "vchrInstitution");

            _viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);

            this.DataContext = _viewModel;
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            if (_viewModel.ContactID >= 0) {
                RegisterUniquePendingChange(new UpdateContactAction(_viewModel.Model));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var address = txtEmail.Text;
            if (string.IsNullOrWhiteSpace(address)) {
                ErrorMessage.Show("You must supply an email address for this contact before you can send them a message.");
                return;
            }

            SystemUtils.ShellExecute(string.Format("mailto:{0}", address));
        }
    }

}
