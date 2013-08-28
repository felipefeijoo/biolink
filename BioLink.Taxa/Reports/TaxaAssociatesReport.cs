using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class TaxaAssociatesReport : ReportBase {

        public TaxaAssociatesReport(User user, List<TaxonViewModel> taxa) : base(user) {
            this.Taxa = taxa;
            var tableViewer = new TabularDataViewerSource();

            tableViewer.ContextMenuHandler = (builder, row) => {
                var viewModels = row.Matrix.Tag as List<AssociateReportViewModel>;
                if (viewModels != null) {
                    // find the view model for this associate record...
                    var index = row.Matrix.IndexOf("AssociateID");
                    if (index > 0) {
                        var targetId = (int) row[index];
                        var selected = viewModels.Find((vm) => {
                            return vm.AssociateID == targetId;
                        });
                        if (selected != null) {

                            var fromDescription = string.Format(" {0} \"{1}\" ({2})", TraitCategoryTypeHelper.GetLookupTypeFromCategoryID(selected.Model.FromCatID), selected.FromViewModel.DisplayLabel, selected.RelationFromTo);
                            string toDescription = null;                            
                            builder.New("Edit" + fromDescription).Handler(() => {
                                EditAssociatedItem(selected.Model.FromCatID, selected.Model.FromIntraCatID);
                            }).End();

                            if (selected.Model.ToIntraCatID.HasValue) {

                                toDescription = string.Format(" {0} \"{1}\" ({2})", TraitCategoryTypeHelper.GetLookupTypeFromCategoryID(selected.Model.ToCatID.Value), selected.ToViewModel.DisplayLabel, selected.RelationToFrom);
                                builder.New("Edit" + toDescription).Handler(() => {
                                    EditAssociatedItem(selected.Model.ToCatID.Value, selected.Model.ToIntraCatID.Value);
                                }).End();
                            }

                            builder.Separator();

                            builder.New("Pin " + fromDescription + " to pin board").Handler(() => PinAssociatedItem(selected.Model.FromCatID, selected.Model.FromIntraCatID)).End();
                            if (selected.Model.ToIntraCatID.HasValue) {
                                builder.New("Pin " + toDescription + " to pin board").Handler(() => PinAssociatedItem(selected.Model.ToCatID.Value, selected.Model.ToIntraCatID.Value)).End();
                            }

                        }
                    }
                }
            };
            RegisterViewer(tableViewer);
            RegisterViewer(new AssociateReportsViewerSource());

            this.DisplayColumns.Add(new DisplayColumnDefinition { ColumnName = "BiotaFullName", DisplayName = "Taxa" });
            this.DisplayColumns.Add(new DisplayColumnDefinition { ColumnName = "AssociateName", DisplayName = "Associate" });
            this.DisplayColumns.Add(new DisplayColumnDefinition { ColumnName = "Relationship", DisplayName = "Relationship" });
            this.DisplayColumns.Add(new DisplayColumnDefinition { ColumnName = "FullRegion", DisplayName = "Region" });
            this.DisplayColumns.Add(new DisplayColumnDefinition { ColumnName = "Source", DisplayName = "Source" });
            this.DisplayColumns.Add(new DisplayColumnDefinition { ColumnName = "RefCode", DisplayName = "Reference" });
            this.DisplayColumns.Add(new DisplayColumnDefinition { ColumnName = "RefPage", DisplayName = "Ref. Page" });
            this.DisplayColumns.Add(new DisplayColumnDefinition { ColumnName = "Notes", DisplayName = "Notes" });
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {

            if (progress != null) {
                progress.ProgressMessage("Phase 1: Finding associate relationships...");
            }

            int[] ids = new int[Taxa.Count];
            int i = 0;
            foreach (TaxonViewModel vm in Taxa) {
                ids[i++] = vm.TaxaID.Value;
            }

            var matrix = Service.GetAssociatesForTaxa(RegionID, this.StripRTF, ids);

            var index = matrix.IndexOf("AssociateID");
            if (index >= 0) {

                if (progress != null) {
                    progress.ProgressMessage("Phase 2: Retrieving associates...");
                }

                var idList = new List<int>();
                foreach (MatrixRow row in matrix) {
                    var associateId = (int)row[index];
                    if (!idList.Contains(associateId)) {
                        idList.Add(associateId);
                    }
                }

                if (progress != null) {                    
                    progress.ProgressStart("Phase 3: Building view models...");
                }

                var viewModels = new List<AssociateReportViewModel>();

                if (idList.Count > 0) {
                    var service = new SupportService(PluginManager.Instance.User);
                    var associates = service.GetAssociatesById(idList);
                    int count = 0;
                    foreach (Associate m in associates) {

                        count++;

                        if (progress != null) {
                            var percentComplete = ((double) count / (double) associates.Count) * 100.0;
                            progress.ProgressMessage("Phase 3: Building view models...", percentComplete);
                        }

                        var vm = new AssociateReportViewModel(m);

                        if (m.FromCatID == TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Material)) {
                            vm.FromViewModel = GetViewModel(LookupType.Material, m.FromIntraCatID);
                        } else if (m.FromCatID == TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Taxon)) {
                            vm.FromViewModel = GetViewModel(LookupType.Taxon, m.FromIntraCatID);
                        } else {
                            vm.FromViewModel = new ViewModelPlaceholder(m.AssocDescription, "images/Description.png");
                        }

                        if (m.ToCatID == TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Material)) {
                            vm.ToViewModel = GetViewModel(LookupType.Material, m.ToIntraCatID);
                        } else if (m.ToCatID == TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Taxon)) {
                            vm.ToViewModel = GetViewModel(LookupType.Taxon, m.ToIntraCatID);
                        } else {
                            vm.ToViewModel = new ViewModelPlaceholder(m.AssocDescription, "images/Description.png");
                        }

                        viewModels.Add(vm);
                    }

                    progress.ProgressEnd("Complete.");                    
                }

                matrix.Tag = viewModels;
            }

            return matrix;
        }

        internal void EditAssociatedItem(int catId, int intraCatId) {
            var lookupType = TraitCategoryTypeHelper.GetLookupTypeFromCategoryID(catId);
            switch (lookupType) {
                case LookupType.Material:
                case LookupType.Taxon:
                    PluginManager.Instance.EditLookupObject(lookupType, intraCatId);
                    break;
                default:
                    // Don't think this should ever happen!
                    ErrorMessage.Show("Error!");
                    break;
            }

        }

        internal void PinAssociatedItem(int catId, int intraCatId) {
            LookupType type = TraitCategoryTypeHelper.GetLookupTypeFromCategoryID(catId);
            IBioLinkPlugin plugin = PluginManager.Instance.GetLookupTypeOwner(type);
            if (plugin != null) {
                PluginManager.Instance.PinObject(new PinnableObject(plugin.Name, type, intraCatId));
            }

        }


        private ViewModelBase GetViewModel(LookupType lookupType, int? objectId) {
            if (objectId.HasValue) {
                var plugin = PluginManager.Instance.GetLookupTypeOwner(lookupType);
                if (plugin != null) {
                    var pin = new PinnableObject(plugin.Name, lookupType, objectId.Value);
                    return PluginManager.Instance.GetViewModel(pin);
                }
            }
            return null;
        }

        public override string Name {
            get { return string.Format("Associates for Taxa: {0}", Taxa.Count == 1 ? Taxa[0].DisplayLabel : "Multiple taxa"); }
        }

        public override bool DisplayOptions(User user, System.Windows.Window parentWindow) {
            var frm = new RegionBasedReportOptions(user, Taxa, "Associates for Taxa options");
            frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            frm.Owner = parentWindow;
            if (frm.ShowDialog().ValueOrFalse()) {
                this.RegionID = frm.txtRegion.ObjectID.GetValueOrDefault(-1);
                this.StripRTF = frm.chkStripRTF.IsChecked.GetValueOrDefault(true);
                return true;
            }

            return false;            
        }

        protected int RegionID { get; private set; }

        protected bool StripRTF { get; private set; }

        protected List<TaxonViewModel> Taxa { get; private set; }

        protected TaxaService Service { get { return new TaxaService(User); } }
    }
}
