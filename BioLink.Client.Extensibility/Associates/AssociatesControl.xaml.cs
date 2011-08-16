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
    /// Interaction logic for AssociatesControl.xaml
    /// </summary>
    public partial class AssociatesControl : UserControl, IOneToManyDetailEditor {

        #region Designer CTOR
        public AssociatesControl() {
            InitializeComponent();
        }
        #endregion

        public AssociatesControl(TraitCategoryType category, ViewModelBase owner) {

            InitializeComponent();
            this.Category = category;

            var itemsList =new List<String>(new String[] {"Description", "Taxon", "Material" });
            cmbType.ItemsSource = itemsList;

            if (category != TraitCategoryType.Taxon) {
                contentGrid.RowDefinitions[2].Height = new GridLength(0);
                lblRelAToB.Content = "Specimen is a:";
            } else {
                txtPoliticalRegion.BindUser(User, LookupType.Region);
                lblRelAToB.Content = "Taxon is a:";
            }

            cmbType.SelectionChanged += new SelectionChangedEventHandler(cmbType_SelectionChanged);            
            DataContextChanged += new DependencyPropertyChangedEventHandler(AssociatesControl_DataContextChanged);

            txtSource.BindUser(User, "tblAssociate", "vchrSource");
            txtReference.BindUser(User, LookupType.Reference);

        }

        void AssociatesControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var viewModel = e.NewValue as AssociateViewModel;
            if (viewModel != null) {
                // Political region is only available when this control is attached to a taxon...

                if (viewModel.Direction == "ToFrom") {
                    txtRelBtoA.BindUser(User, "tblAssociate", "vchrRelationFromTo");
                    txtRelAToB.BindUser(User, "tblAssociate", "vchrRelationToFrom");
                } else {
                    txtRelBtoA.BindUser(User, "tblAssociate", "vchrRelationToFrom");
                    txtRelAToB.BindUser(User, "tblAssociate", "vchrRelationFromTo");
                }
            }
        }

        void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            contentGrid.RowDefinitions[2].Height = new GridLength(0); // Political region
            switch (cmbType.SelectedItem as string) {
                case "Taxon":
                    txtAssociate.Visibility = System.Windows.Visibility.Visible;
                    txtDescription.Visibility = System.Windows.Visibility.Hidden;
                    txtAssociate.BindUser(User, LookupType.Taxon);
                    contentGrid.RowDefinitions[1].Height = new GridLength(32);
                    if (Category == TraitCategoryType.Taxon) {
                        contentGrid.RowDefinitions[2].Height = contentGrid.RowDefinitions[0].Height;
                    }
                    break;
                case "Material":
                    contentGrid.RowDefinitions[1].Height = new GridLength(32);
                    txtAssociate.Visibility = System.Windows.Visibility.Visible;
                    txtDescription.Visibility = System.Windows.Visibility.Hidden;
                    txtAssociate.BindUser(User, LookupType.Material);
                    break;
                case "Description":
                    txtDescription.Visibility = System.Windows.Visibility.Visible;
                    txtAssociate.Visibility = System.Windows.Visibility.Hidden;
                    contentGrid.RowDefinitions[1].Height = new GridLength(50);
                    if (Category == TraitCategoryType.Taxon) {
                        contentGrid.RowDefinitions[2].Height = contentGrid.RowDefinitions[0].Height;
                    }
                    break;
            }
        }


        protected User User { get { return PluginManager.Instance.User; } }


        public TraitCategoryType Category { get; private set; }


        public UIElement FirstControl {
            get { return cmbType; }
        }
    }

    public class AssociateTypeConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var toCatID = value as int?;            
            if (toCatID != null && toCatID.HasValue && toCatID.Value > 0) {
                if (toCatID == 1) {
                    return TraitCategoryType.Material.ToString();
                } if (toCatID == 2) {
                    return TraitCategoryType.Taxon.ToString();
                } else {
                    throw new Exception("Unhandled Associate Type: " + toCatID);
                }
            } else {
                return "Description";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var str = value as string;
            if (str != null) {
                switch (str) {
                    case "Description":
                        return null;
                    case "Taxon":
                        return 2;
                    case "Material":
                        return 1;
                    default:
                        throw new Exception("Unhandled Associate Type: " + str);
                }
            }

            return null;
        }
    }
}
