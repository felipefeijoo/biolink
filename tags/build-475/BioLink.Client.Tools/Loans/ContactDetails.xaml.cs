/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
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
    public partial class ContactDetails : DatabaseCommandControl {

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
                RegisterUniquePendingChange(new InsertContactCommand(model));
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
            this.WindowTitle = string.Format("Contact detail: {0} [{1}]", _viewModel.FullName, _viewModel.ContactID);

        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            if (_viewModel.ContactID >= 0) {
                RegisterUniquePendingChange(new UpdateContactCommand(_viewModel.Model));
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
