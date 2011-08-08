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


namespace BioLink.Client.Extensibility {

    /// <summary>
    /// Interaction logic for PestHostAssociateControl.xaml
    /// </summary>
    public partial class PestHostAssociateControl : UserControl, IOneToManyDetailEditor {

        private bool _manualSet;

        public PestHostAssociateControl(User user, TraitCategoryType category, ViewModelBase owner) {
            InitializeComponent();
            this.Category = category;

            txtRegion.BindUser(user, LookupType.Region);
            txtSource.BindUser(user, "tblAssociate", "vchrSource");
            txtReference.BindUser(user, LookupType.Reference);

            txtAssociate.PreviewDragOver += new DragEventHandler(txtAssociate_PreviewDragOver);
            txtAssociate.PreviewDragEnter += new DragEventHandler(txtAssociate_PreviewDragOver);

            txtAssociate.PreviewDrop += new DragEventHandler(txtAssociate_PreviewDrop);

            txtAssociate.KeyUp += new KeyEventHandler(txtAssociate_KeyUp);

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(PestHostAssociateControl_DataContextChanged);

        }

        void txtAssociate_KeyUp(object sender, KeyEventArgs e) {
            var selected = DataContext as AssociateViewModel;
            if (selected != null) {
                selected.RelativeCatID = null;
                selected.RelativeIntraCatID = null;
                lblAssociateType.Content = "Description";
                selected.AssocDescription = txtAssociate.Text;
            }
        }

        void txtAssociate_PreviewDrop(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            if (pinnable != null) {
                SetFromPinnable(pinnable);
                e.Handled = true;
            }
        }

        void PestHostAssociateControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var selected = this.DataContext as AssociateViewModel;
            if (selected != null) {

                if (selected.RelativeCatID == 1 || selected.RelativeCatID == 2) {
                    txtAssociate.Text = selected.AssocName;
                    var lookupType = GetLookupTypeFromCategoryID(selected.RelativeCatID.Value);
                    lblAssociateType.Content = lookupType.ToString();
                } else {
                    txtAssociate.Text = selected.AssocDescription;
                    lblAssociateType.Content = "Description";
                }

                _manualSet = true;
                if (selected.RelativeRelationFromTo.Equals("host", StringComparison.CurrentCultureIgnoreCase)) {
                    optPest.IsChecked = true;
                } else {
                    optHost.IsChecked = true;
                }
                _manualSet = false;
            }
        }

        private void GenericLookup<T>() {
            PluginManager.Instance.StartSelect<T>((result) => {
                var associate = this.DataContext as AssociateViewModel;
                _manualSet = true;                
                txtAssociate.Text = result.Description;
                associate.AssocName = result.Description;
                lblAssociateType.Content = result.LookupType.ToString();
                associate.RelativeIntraCatID = result.ObjectID;
                associate.RelativeCatID = GetCategoryIDFromLookupType(result.LookupType);
                associate.RelativeRelationFromTo = optPest.IsChecked.ValueOrFalse() ? "Pest" : "Host";
                associate.RelativeRelationToFrom = optPest.IsChecked.ValueOrFalse() ? "Host" : "Pest";

                _manualSet = false;
            }, LookupOptions.None);
        }

        void txtAssociate_PreviewDragOver(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            e.Effects = DragDropEffects.None;
            if (pinnable != null) {
                if (pinnable.LookupType == LookupType.Material || pinnable.LookupType == LookupType.Taxon) {
                    e.Effects = DragDropEffects.Link;                    
                }
                e.Handled = true;
            }                                        
        }

        private void SetFromPinnable(PinnableObject pinnable) {
            var selected = this.DataContext as AssociateViewModel;
            if (pinnable != null && selected != null) {
                var viewModel = PluginManager.Instance.GetViewModel(pinnable);
                txtAssociate.Text = viewModel.DisplayLabel;
                selected.AssocName = viewModel.DisplayLabel;
                selected.RelativeCatID = GetCategoryIDFromLookupType(pinnable.LookupType);
                selected.RelativeIntraCatID = pinnable.ObjectID;
                SetRelationships();
                lblAssociateType.Content = pinnable.LookupType.ToString();
            }
        }

        private int GetCategoryIDFromLookupType(LookupType l) {
            switch (l) {
                case LookupType.Material:
                    return 1;
                case LookupType.Taxon:
                    return 2;
                default:
                    return -1;
            }
        }

        public LookupType GetLookupTypeFromCategoryID(int catId) {
            switch (catId) {                
                case 1: // Material
                    return LookupType.Material;
                case 2: // Taxon
                    return LookupType.Taxon;
                default:
                    return LookupType.Unknown;
            }
        }

        #region Properties

        public TraitCategoryType Category { get; private set; }

        #endregion

        private void btnSelectTaxon_Click(object sender, RoutedEventArgs e) {
            GenericLookup<Taxon>();
        }

        private void btnSelectMaterial_Click(object sender, RoutedEventArgs e) {
            GenericLookup<Material>();
        }

        private void btnEditSelected_Click(object sender, RoutedEventArgs e) {
            var selected = DataContext as AssociateViewModel;
            if (selected != null) {

                if (selected.RelativeCatID.HasValue) {
                    var lookupType = GetLookupTypeFromCategoryID(selected.RelativeCatID.Value);
                    if (lookupType != LookupType.Unknown && selected.RelativeIntraCatID.HasValue) {
                        PluginManager.Instance.EditLookupObject(lookupType, selected.RelativeIntraCatID.Value);
                    }
                }

            }
        }

        private void optPest_Checked(object sender, RoutedEventArgs e) {
            if (!_manualSet) {
                SetRelationships();
            }
        }

        private void optHost_Checked(object sender, RoutedEventArgs e) {
            if (!_manualSet) {
                SetRelationships();
            }
        }

        private void SetRelationships() {
            var associate = DataContext as AssociateViewModel;
            if (associate != null) {
                associate.RelativeRelationFromTo = optPest.IsChecked.ValueOrFalse() ? "Host" : "Pest";
                associate.RelativeRelationToFrom = optPest.IsChecked.ValueOrFalse() ? "Pest" : "Host";
            }
        }

        protected User User { get; private set; }

        UIElement IOneToManyDetailEditor.FirstControl {
            get { throw new NotImplementedException(); }
        }
    }
}
