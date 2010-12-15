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
    /// Interaction logic for ReferenceDetail.xaml
    /// </summary>
    public partial class ReferenceDetail : DatabaseActionControl {

        private ReferenceViewModel _viewModel;

        #region Designer Constructor

        public ReferenceDetail() {
            InitializeComponent();
        }

        #endregion

        private TabItem _notesTab;
        private TabItem _traitsTab;
        private TabItem _mmTab;

        public ReferenceDetail(User user, int referenceID)
            : base(user, "Reference:" + referenceID) {

            InitializeComponent();

            var refTypeList = new List<RefTypeMapping>();
            foreach (string reftypecode in SupportService.RefTypeMap.Keys) {
                refTypeList.Add(SupportService.RefTypeMap[reftypecode]);
            }

            Reference model = null;
            if (referenceID >= 0) {
                var service = new SupportService(user);
                model = service.GetReference(referenceID);
            } else {
                model = new Reference();
                model.RefID = -1;
                txtRefCode.IsReadOnly = false;
                Loaded += new RoutedEventHandler(ReferenceDetail_Loaded);
                model.RefType = refTypeList[0].RefTypeCode;
            }

            _viewModel = new ReferenceViewModel(model);

            _viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);

            this.DataContext = _viewModel;

            cmbRefType.ItemsSource = refTypeList;
            cmbRefType.DisplayMemberPath = "RefTypeName";

            txtPossess.BindUser(User, PickListType.Phrase, "Reference Possess", TraitCategoryType.Reference);
            txtSource.BindUser(User, PickListType.Phrase, "Reference Source", TraitCategoryType.Reference);

            _traitsTab = tabRef.AddTabItem("Traits", new TraitControl(User, TraitCategoryType.Reference, _viewModel.RefID));
            _notesTab = tabRef.AddTabItem("Notes", new NotesControl(User, TraitCategoryType.Reference, _viewModel.RefID));
            _mmTab = tabRef.AddTabItem("Multimedia", new MultimediaControl(User, TraitCategoryType.Reference, _viewModel.RefID));

            tabRef.AddTabItem("Ownership", new OwnershipDetails(_viewModel.Model));

            if (model.RefID < 0) {
                _traitsTab.IsEnabled = false;
                _notesTab.IsEnabled = false;
                _mmTab.IsEnabled = false;
            }

            cmbRefType.SelectionChanged += new SelectionChangedEventHandler(cmbRefType_SelectionChanged);

            this.ChangesCommitted += new PendingChangesCommittedHandler(ReferenceDetail_ChangesCommitted);
            
        }

        void cmbRefType_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var refTypeMapping = cmbRefType.SelectedItem as RefTypeMapping;
            gridSpecific.Children.Clear();
            if (refTypeMapping != null) {                
                FrameworkElement control = null;
                switch (refTypeMapping.RefTypeCode) {
                    case "J":
                        control = new JournalDetails(User);
                        break;
                    case "JS":
                        break;
                    case "B":
                        control = new BookDetails();
                        break;
                    case "BS":
                        break;
                    case "M":
                        control = new MiscDetails();
                        break;
                    case "U":
                        break;
                }

                if (control != null) {
                    control.DataContext = _viewModel;
                    gridSpecific.Children.Add(control);
                }
            }            
        }

        void ReferenceDetail_ChangesCommitted(object sender) {
            _traitsTab.IsEnabled = true;
            _notesTab.IsEnabled = true;
            _mmTab.IsEnabled = true;            
        }

        void ReferenceDetail_Loaded(object sender, RoutedEventArgs e) {
            if (_viewModel != null && _viewModel.RefID < 0) {
                RegisterUniquePendingChange(new InsertReferenceAction(_viewModel.Model));
                txtRefCode.Focus();
            }
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            var r = viewmodel as ReferenceViewModel;
            if (r != null) {
                RegisterUniquePendingChange(new UpdateReferenceAction(r.Model));
            }
        }

    }

    public class RefTypeConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string reftype = value as string;
            if (reftype != null) {
                if (SupportService.RefTypeMap.ContainsKey(reftype)) {
                    return SupportService.RefTypeMap[reftype];
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var pair = value as RefTypeMapping;
            if (pair != null) {
                return pair.RefTypeCode;
            }
            return null;
        }
    }
}
