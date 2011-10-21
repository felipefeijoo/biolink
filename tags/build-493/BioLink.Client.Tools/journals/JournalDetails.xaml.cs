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
using BioLink.Data.Model;
using BioLink.Data;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for JournalDetails.xaml
    /// </summary>
    public partial class JournalDetails : DatabaseCommandControl {

        private JournalViewModel _viewModel;

        private TabItem _traits;
        private TabItem _notes;
        

        #region Designer Ctor
        public JournalDetails() {
            InitializeComponent();
        }
        #endregion

        public JournalDetails(User user, int journalID) : base(user, "Journal:" + journalID) {
            InitializeComponent();
            Journal model = null;

            if (journalID >= 0) {
                var service = new SupportService(user);
                model = service.GetJournal(journalID);
            } else {
                model = new Journal();
                model.JournalID = -1;
                model.FullName = "<New Journal>";
            }

            _viewModel = new JournalViewModel(model);

            _traits = tabJournal.AddTabItem("Traits", new TraitControl(user, TraitCategoryType.Journal, _viewModel));
            _notes = tabJournal.AddTabItem("Notes", new NotesControl(user, TraitCategoryType.Journal, _viewModel));
            tabJournal.AddTabItem("Ownership", new OwnershipDetails(model));

            
            if (_viewModel.JournalID >= 0) {
                _viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
            } else {
                Loaded += new RoutedEventHandler(JournalDetails_Loaded);
                _traits.IsEnabled = false;
                _notes.IsEnabled = false;
            }

            ChangesCommitted += new PendingChangesCommittedHandler(JournalDetails_ChangesCommitted);

            this.DataContext = _viewModel;
        }

        void JournalDetails_ChangesCommitted(object sender) {
            _traits.IsEnabled = true;
            _notes.IsEnabled = true;
        }

        void JournalDetails_Loaded(object sender, RoutedEventArgs e) {
            RegisterPendingChange(new InsertJournalCommand(_viewModel.Model));
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            RegisterUniquePendingChange(new UpdateJournalCommand((viewmodel as JournalViewModel).Model));
        }

    }
}
