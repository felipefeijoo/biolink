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
using System.Collections.ObjectModel;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for IdentificationHistoryControl.xaml
    /// </summary>
    public partial class IdentificationHistoryControl : DatabaseCommandControl {

        private ObservableCollection<MaterialIdentificationViewModel> _model;

        #region Designer Constructor
        public IdentificationHistoryControl() {
            InitializeComponent();
        }
        #endregion

        public IdentificationHistoryControl(User user, int materialID) : base(user, "MaterialIDHistory:" + materialID) {
            InitializeComponent();
            this.MaterialID = materialID;

            txtIdentifiedBy.BindUser(User, "tblMaterial", "vchrIDBy");
            txtReference.BindUser(User, LookupType.Reference);
            txtAccuracy.BindUser(User, PickListType.Phrase, "Identification Accuracy", TraitCategoryType.Material);
            txtMethod.BindUser(User, PickListType.Phrase, "Identification Method", TraitCategoryType.Material);
            txtNameQual.BindUser(User, PickListType.Phrase, "Identification Qualifier", TraitCategoryType.Material);

            this.ChangesCommitted += new PendingChangesCommittedHandler(IdentificationHistoryControl_ChangesCommitted);

            LoadIDHistory();
        }

        void IdentificationHistoryControl_ChangesCommitted(object sender) {
            LoadIDHistory();
        }

        public void LoadIDHistory() {
            var service = new MaterialService(User);
            var list = service.GetMaterialIdentification(MaterialID);
            _model = new ObservableCollection<MaterialIdentificationViewModel>(list.ConvertAll((m) => {
                var viewModel = new MaterialIdentificationViewModel(m);
                viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
                return viewModel;
            }));

            lst.SelectionChanged += new SelectionChangedEventHandler(lst_SelectionChanged);

            detailGrid.IsEnabled = false;

            lst.ItemsSource = _model;
            if (_model.Count > 0) {
                lst.SelectedItem = _model[0];
            }
        }

        void lst_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            detailGrid.IsEnabled = lst.SelectedItem != null;
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            RegisterUniquePendingChange(new UpdateMaterialIdentificationCommand((viewmodel as MaterialIdentificationViewModel).Model));
        }

        public int MaterialID { get; private set; }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelected();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            AddNew();
        }

        private void AddNew() {
            var model = new MaterialIdentification();
            model.MaterialID = MaterialID;
            model.MaterialIdentID = -1;
            model.Taxa = "<New identification>";
            var viewmodel = new MaterialIdentificationViewModel(model);
            _model.Add(viewmodel);

            lst.SelectedItem = viewmodel;

            RegisterUniquePendingChange(new InsertMaterialIdentificationCommand(viewmodel.Model));
        }

        private void DeleteSelected() {
            var selected = lst.SelectedItem as MaterialIdentificationViewModel;
            if (selected != null) {
                _model.Remove(selected);
                RegisterPendingChange(new DeleteMaterialIdentificationCommand(selected.Model));
            }
        }

        public void AddHistoryFromMaterial(MaterialViewModel m) {
            var model = new MaterialIdentification();

            model.MaterialID = m.MaterialID;
            model.MaterialIdentID = -1;
            model.IDAccuracy = m.IdentificationAccuracy;
            model.IDBy = m.IdentifiedBy;
            model.IDDate = m.IdentificationDate;
            model.IDMethod = m.IdentificationMethod;
            model.IDNotes = m.IdentificationNotes;
            model.IDRefID = m.IdentificationReferenceID;
            model.IDRefPage = m.IdentificationRefPage;
            model.Taxa = m.TaxaDesc;

            var viewmodel = new MaterialIdentificationViewModel(model);
            _model.Add(viewmodel);
            RegisterUniquePendingChange(new InsertMaterialIdentificationCommand(viewmodel.Model));            
        }
    }

    public class MaterialIdentificationViewModel : GenericViewModelBase<MaterialIdentification> {

        public MaterialIdentificationViewModel(MaterialIdentification model) : base(model, ()=>model.MaterialIdentID) { }

        public override string DisplayLabel {
            get {
                return string.Format("{0} by {1} on {2:d MMM, yyyy}", Taxa, IDBy, IDDate);
            }
        }

        public int MaterialIdentID {
            get { return Model.MaterialIdentID; }
            set { SetProperty(() => Model.MaterialIdentID, value); }
        }

        public int MaterialID {
            get { return Model.MaterialID; }
            set { SetProperty(() => Model.MaterialID, value); }
        }

        public string Taxa {
            get { return Model.Taxa; }
            set { SetProperty(() => Model.Taxa, value); }
        }

        public string IDBy {
            get { return Model.IDBy; }
            set { SetProperty(() => Model.IDBy, value); }
        }

        public DateTime? IDDate {
            get { return Model.IDDate; }
            set { SetProperty(() => Model.IDDate, value); }
        }

        public int IDRefID {
            get { return Model.IDRefID; }
            set { SetProperty(() => Model.IDRefID, value); }
        }

        public string IDMethod {
            get { return Model.IDMethod; }
            set { SetProperty(() => Model.IDMethod, value); }
        }

        public string IDAccuracy {
            get { return Model.IDAccuracy; }
            set { SetProperty(() => Model.IDAccuracy, value); }
        }

        public string NameQual {
            get { return Model.NameQual; }
            set { SetProperty(() => Model.NameQual, value); }
        }

        public string IDNotes {
            get { return Model.IDNotes; }
            set { SetProperty(() => Model.IDNotes, value); }
        }

        public string IDRefPage {
            get { return Model.IDRefPage; }
            set { SetProperty(() => Model.IDRefPage, value); }
        }

        public int? BasedOnID {
            get { return Model.BasedOnID; }
            set { SetProperty(() => Model.BasedOnID, value); }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); }
        }

    }

    public class UpdateMaterialIdentificationCommand : GenericDatabaseCommand<MaterialIdentification> {

        public UpdateMaterialIdentificationCommand(MaterialIdentification model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateMaterialIdentification(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }

    public class InsertMaterialIdentificationCommand : GenericDatabaseCommand<MaterialIdentification> {
        public InsertMaterialIdentificationCommand(MaterialIdentification model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.MaterialIdentID = service.InsertMaterialIdentification(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }

    public class DeleteMaterialIdentificationCommand : GenericDatabaseCommand<MaterialIdentification> {
        public DeleteMaterialIdentificationCommand(MaterialIdentification model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteMaterialIdentification(Model.MaterialIdentID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }

}
