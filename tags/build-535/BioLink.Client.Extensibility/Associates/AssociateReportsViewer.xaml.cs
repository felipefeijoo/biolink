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
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections;


namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for AssociateReportsViewer.xaml
    /// </summary>
    public partial class AssociateReportsViewer : UserControl {

        public AssociateReportsViewer() {
            InitializeComponent();
        }

        public AssociateReportsViewer(IBioLinkReport report, DataMatrix data, IProgressObserver progress) {
            InitializeComponent();
            this.Report = report;
            this.DataMatrix = data;
            this.Progress = progress;

            Loaded += new RoutedEventHandler(AssociateReportsViewer_Loaded);
        }

        void AssociateReportsViewer_Loaded(object sender, RoutedEventArgs e) {

            var viewModels = DataMatrix.Tag as List<AssociateReportViewModel>;
            if (viewModels != null) {
                lst.ItemsSource = viewModels;
            }

        }

        protected IBioLinkReport Report { get; set; }

        protected DataMatrix DataMatrix { get; set; }

        protected IProgressObserver Progress { get; set; }

        private void Border_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {

            lst.ContextMenu = null;
            var selected = lst.SelectedItem as AssociateReportViewModel;
            if (selected != null) {
                var builder = new ContextMenuBuilder(null);
                builder.New("Edit " + selected.FromViewModel.DisplayLabel + " (" + selected.RelationFromTo + ")").Handler(() => {
                    EditAssociatedItem(selected.Model.FromCatID, selected.Model.FromIntraCatID);
                }).End();

                builder.Separator();

                builder.New("Pin " + selected.FromViewModel.DisplayLabel + " (" + selected.RelationFromTo + ") to pin board").Handler(() => PinAssociatedItem(selected.Model.FromCatID, selected.Model.FromIntraCatID)).End();

                lst.ContextMenu = builder.ContextMenu;
            }
        }

        private void EditAssociatedItem(int catId, int intraCatId) {

            if (catId == TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Material)) {
                PluginManager.Instance.EditLookupObject(LookupType.Material, intraCatId);
            } else if (catId == TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Taxon)) {
                PluginManager.Instance.EditLookupObject(LookupType.Taxon, intraCatId);                    
            } else {
                // Don't think this should ever happen!
                ErrorMessage.Show("Error!");
            }

        }

        private void PinAssociatedItem(int catId, int intraCatId) {
            LookupType type = LookupType.Unknown;
            IBioLinkPlugin plugin = null;

            if (catId == TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Material)) {
                type = LookupType.Material;
                plugin = PluginManager.Instance.GetLookupTypeOwner(LookupType.Material);
            } else if (catId == TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Taxon)) {
                type = LookupType.Taxon;
                plugin = PluginManager.Instance.GetLookupTypeOwner(LookupType.Taxon);
            } 

            if (plugin != null) {
                PluginManager.Instance.PinObject(new PinnableObject(plugin.Name, type, intraCatId));
            }
        }

        private void Border_MouseRightButtonUp_1(object sender, MouseButtonEventArgs e) {
            lst.ContextMenu = null;
            var selected = lst.SelectedItem as AssociateReportViewModel;
            if (selected != null && selected.Model.ToIntraCatID.HasValue) {
                var builder = new ContextMenuBuilder(null);
                builder.New("Edit " + selected.ToViewModel.DisplayLabel + " (" + selected.RelationToFrom + ")").Handler(() => {
                    EditAssociatedItem(selected.Model.ToCatID.Value, selected.Model.ToIntraCatID.Value);
                }).End();

                lst.ContextMenu = builder.ContextMenu;

                builder.Separator();

                builder.New("Pin " + selected.ToViewModel.DisplayLabel + " (" + selected.RelationToFrom + ") to pin board").Handler(() => PinAssociatedItem(selected.Model.ToCatID.Value, selected.Model.ToIntraCatID.Value)).End();
            }

        }
        
    }

    public class AssociateReportTooltip : TooltipContentBase {

        public AssociateReportTooltip(AssociateReportViewModel viewModel) : base(viewModel.ObjectID.Value, viewModel) { }

        protected override void GetDetailText(BioLinkDataObject model, TextTableBuilder builder) {
            var m = model as Associate;
            if (m != null) {

                builder.Add("Name", m.AssocName);
                builder.Add("Description", m.AssocDescription);

                builder.Add("Direction", m.Direction);
                builder.Add("From category", m.FromCategory);
                builder.Add("To category", m.ToCategory);

                builder.Add("Relation From->To", m.RelationFromTo);
                builder.Add("Relation To->From", m.RelationToFrom);

                builder.Add("Region", m.PoliticalRegion);
                builder.Add("Region ID", m.PoliticalRegionID);

                builder.Add("Ref Code", m.RefCode);
                builder.Add("Ref Page", m.RefPage);

                builder.Add("Source", m.Source);                
                builder.Add("Is Uncertain", m.Uncertain.ToString());

                var vm = ViewModel as AssociateReportViewModel;
                if (vm != null) {

                    if (vm.FromViewModel != null) {
                        builder.Add("\"From\" type", GetLookupTypeFromAssociateCategoryId(m.FromCatID).ToString());
                        builder.Add("\"From\" name", vm.FromViewModel.DisplayLabel);
                        builder.Add("\"From\" object id", vm.FromViewModel.ObjectID);
                    }

                    if (vm.ToViewModel != null) {
                        if (m.ToCatID.HasValue) {
                            builder.Add("\"To\" type", GetLookupTypeFromAssociateCategoryId(m.ToCatID.Value).ToString());
                        } else {
                            builder.Add("\"To\" type", "Description");
                        }
                        builder.Add("\"To\" name", vm.ToViewModel.DisplayLabel);
                        builder.Add("\"To\" object id", vm.ToViewModel.ObjectID);
                    }
                    
                }
                
            }
        }

        internal LookupType GetLookupTypeFromAssociateCategoryId(int catId) {

            if (catId == TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Material)) {
                return LookupType.Material;
            } else if (catId == TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Taxon)) {
                return LookupType.Taxon;
            } else {
                return LookupType.Unknown;
            }

        }

        protected override BioLinkDataObject GetModel() {
            var vm = ViewModel as AssociateReportViewModel;
            if (vm != null) {
                return vm.Model;
            }
            return null;
        }
    }

    public class AssociateReportViewModel : GenericViewModelBase<Associate>{

        public AssociateReportViewModel(Associate model) : base(model, ()=>model.AssociateID) { }

        public ViewModelBase FromViewModel { get; set; }

        public ViewModelBase ToViewModel { get; set; }

        public override string DisplayLabel {
            get { return "Association " + Model.AssocName; }
        }

        public override FrameworkElement TooltipContent {
            get {
                return new AssociateReportTooltip(this);
            }
        }

        public String RelationFromTo {
            get { return Model.RelationFromTo; }
        }

        public String RelationToFrom {
            get { return Model.RelationToFrom; }
        }

        public int AssociateID {
            get { return Model.AssociateID; }
        }

    }    

    public class AssociateReportsViewerSource : IReportViewerSource {

        public string Name {
            get { return "Associates"; }
        }

        public FrameworkElement ConstructView(IBioLinkReport report, DataMatrix reportData, IProgressObserver progress) {
            AssociateReportsViewer viewer = new AssociateReportsViewer(report, reportData, progress);
            return viewer;
        }

    }

}
