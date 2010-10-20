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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ReferencesControl.xaml
    /// </summary>
    public partial class ReferencesControl : DatabaseActionControl {

        private ObservableCollection<RefLinkViewModel> _model;

        #region Designer Constructor
        public ReferencesControl() {
            InitializeComponent();
        }
        #endregion

        public ReferencesControl(User user, TraitCategoryType category, int? intraCategoryID) : base(user, String.Format("References::{0}::{1}", category, intraCategoryID)) {
            InitializeComponent();

            this.Category = category;
            this.IntraCategoryID = intraCategoryID.Value;

            txtRefType.Click += new RoutedEventHandler((source, e) => {
                var reftype = SelectRefType();
                if (!string.IsNullOrEmpty(reftype)) {
                    txtRefType.Text = reftype;
                }

            });

            txtReference.BindUser(user, LookupType.Reference);

            var list = Service.GetReferenceLinks(category.ToString(), IntraCategoryID);
            _model = new ObservableCollection<RefLinkViewModel>(list.ConvertAll((rl) => {
                var vm = new RefLinkViewModel(rl);
                vm.DataChanged += new DataChangedHandler((changed) => {
                    RegisterUniquePendingChange(new UpdateRefLinkAction(vm.Model, Category.ToString()));
                });
                return vm;
            }));

            gridRefLink.IsEnabled = false;
            
            lstReferences.ItemsSource = _model;
            lstReferences.SelectionChanged += new SelectionChangedEventHandler((source, e) => {
                if (!gridRefLink.IsEnabled) {
                    gridRefLink.IsEnabled = true;
                }
                gridRefLink.DataContext = lstReferences.SelectedItem;
            });
        }

        private string SelectRefType() {
            var frm = new PickListWindow(User, "Ref Link types", () => { return Service.GetRefLinkTypes(); }, (newval) => {
                Service.InsertRefLinkType(newval, Category.ToString());
                return true;
            });

            if (frm.ShowDialog().ValueOrFalse()) {
                return frm.SelectedValue;
            }

            return null;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelectedRefLink();
        }

        private void DeleteSelectedRefLink() {
            var item = lstReferences.SelectedItem as RefLinkViewModel;
            if (item != null) {
                RegisterPendingChange(new DeleteRefLinkAction(item.Model));
                _model.Remove(item);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            AddNewRefLink();
        }

        private void AddNewRefLink() {

            var refLinkType = SelectRefType();
            if (!string.IsNullOrEmpty(refLinkType)) {
                var data = new RefLink();
                data.RefLinkID = -1;
                data.RefLinkType = refLinkType;                
                RegisterPendingChange(new InsertRefLinkAction(data, Category.ToString(), IntraCategoryID));

                var viewModel = new RefLinkViewModel(data);
                viewModel.RefCode = NextNewName("<New {0}>", _model, () => viewModel.RefCode);
                _model.Add(viewModel);

                lstReferences.SelectedItem = viewModel;
                lstReferences.ScrollIntoView(viewModel);
            }
        }

        #region Properties

        public TraitCategoryType Category { get; private set; }

        public int IntraCategoryID { get; private set; }

        public SupportService Service { get { return new SupportService(User); } }

        #endregion

    }
}
