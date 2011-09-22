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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for RegionDetails.xaml
    /// </summary>
    public partial class RegionDetails : DatabaseCommandControl {

        private RegionViewModel _viewModel;

        #region Designer Constructor
        public RegionDetails() {
            InitializeComponent();
        }
        #endregion

        public RegionDetails(User user, int regionId, bool readOnly) : base(user, "Region:" + regionId) {
            InitializeComponent();
            this.RegionID = RegionID;

            var service = new MaterialService(user);
            _viewModel = new RegionViewModel(service.GetRegion(regionId));
            this.DataContext = _viewModel;

            _viewModel.DataChanged += new DataChangedHandler(model_DataChanged);

            txtRegionType.BindUser(User, PickListType.Phrase, "Region Rank", TraitCategoryType.Region);
            txtRegion.IsReadOnly = readOnly;

            tabRegion.AddTabItem("Traits", new TraitControl(User, TraitCategoryType.Region, ViewModel) { IsReadOnly = readOnly});
            tabRegion.AddTabItem("Ownership", new OwnershipDetails(_viewModel.Model));

        }

        void model_DataChanged(ChangeableModelBase viewmodel) {
            RegisterUniquePendingChange(new UpdateRegionCommand(_viewModel.Model));
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(RegionDetails), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (RegionDetails)obj;
            if (control != null) {
                var readOnly = (bool)args.NewValue;
                control.txtRegion.IsReadOnly = readOnly;
                control.txtRegionType.IsReadOnly = readOnly;
            }
        }


        #region Properties

        public RegionViewModel ViewModel {
            get { return _viewModel; }
        }

        public int RegionID { get; set; }

        #endregion
    }

    public class UpdateRegionCommand : GenericDatabaseCommand<Region> {

        public UpdateRegionCommand(Region model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateRegion(Model.PoliticalRegionID, Model.Name, Model.Rank);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_REGION, PERMISSION_MASK.UPDATE);
        }

    }

    public class RegionViewModel : GenericViewModelBase<Region> {

        public RegionViewModel(Region model) : base(model, ()=>model.PoliticalRegionID) { }

        public int PoliticalRegionID { 
            get { return Model.PoliticalRegionID; }
            set { SetProperty(() => Model.PoliticalRegionID, value); }
        }
        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string Rank {
            get { return Model.Rank; }
            set { SetProperty(() => Model.Rank, value); }
        }

        public string Parentage {
            get { return Model.Parentage; }
            set { SetProperty(() => Model.Parentage, value); }
        }

    }

}
