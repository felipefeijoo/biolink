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
    public partial class AssociatesControl : OneToManyDetailControl {

        public AssociatesControl() {
            InitializeComponent();
        }

        public AssociatesControl(User user, TraitCategoryType category, ViewModelBase owner) : base(user, "Associates:" + category.ToString() + ":" + (owner == null ? -1 : owner.ObjectID.Value)) {

            InitializeComponent();
            this.Category = category;
            this.Owner = owner;

            var itemsList =new List<String>(new String[] {"Description", "Taxon", "Material" });
            cmbType.ItemsSource = itemsList;

            if (category != TraitCategoryType.Taxon) {
                contentGrid.RowDefinitions[2].Height = new GridLength(0);
                lblRelAToB.Content = "Specimen is a:";
            } else {
                txtPoliticalRegion.BindUser(user, LookupType.Region);
                lblRelAToB.Content = "Taxon is a:";
            }

            cmbType.SelectionChanged += new SelectionChangedEventHandler(cmbType_SelectionChanged);            
            DataContextChanged += new DependencyPropertyChangedEventHandler(AssociatesControl_DataContextChanged);

            txtSource.BindUser(user, "tblAssociate", "vchrSource");
            txtReference.BindUser(user, LookupType.Reference);

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

        public override List<ViewModelBase> LoadModel() {
            var service = new SupportService(User);
            var list = service.GetAssociates(Category.ToString(), Owner.ObjectID.Value);
            return list.ConvertAll((model) => {
                return (ViewModelBase) new AssociateViewModel(model);
            });

        }

        public override ViewModelBase AddNewItem(out DatabaseCommand addAction) {
            var model = new Associate();
            model.AssociateID = -1;
            model.FromIntraCatID = Owner.ObjectID.Value;
            model.FromCategory = Category.ToString();
            model.Direction = "FromTo";

            var viewModel = new AssociateViewModel(model);
            addAction = new InsertAssociateAction(model, Owner);
            return viewModel;
        }

        public override DatabaseCommand PrepareDeleteAction(ViewModelBase viewModel) {
            var a = viewModel as AssociateViewModel;
            if (a != null) {
                return new DeleteAssociateAction(a.Model);
            }
            return null;
        }

        public override DatabaseCommand PrepareUpdateAction(ViewModelBase viewModel) {
            var a = viewModel as AssociateViewModel;
            if (a != null) {
                return new UpdateAssociateAction(a.Model);
            }
            return null;
        }

        public override FrameworkElement FirstControl {
            get {
                return cmbType;
            }
        }

        public TraitCategoryType Category { get; private set; }

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
