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

        public ContactDetails(User user, int contactId) : base(user, "LoanContact:" + contactId) {
            InitializeComponent();

            var service = new LoanService(user);
            var model = service.GetContact(contactId);

            _viewModel = new ContactViewModel(model);

            if (model != null) {
                if (model.StreetAddress.Equals(model.PostalAddress, StringComparison.CurrentCultureIgnoreCase)) {
                    _viewModel.PostalSameAsStreet = true;
                }
            }

            tabContact.AddTabItem("Traits", new TraitControl(user, TraitCategoryType.Contact, _viewModel));
            tabContact.AddTabItem("Notes", new NotesControl(user, TraitCategoryType.Contact, _viewModel));

            txtInstituion.BindUser(user, "tblContact", "vchrInstitution");

            _viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);

            this.DataContext = _viewModel;
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            RegisterUniquePendingChange(new UpdateContactAction(_viewModel.Model));                
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

    public class UpdateContactAction : GenericDatabaseAction<Contact> {
        public UpdateContactAction(Contact model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.UpdateContact(Model);
        }
    }
}
