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

        public ReferenceDetail(User user, int referenceID)
            : base(user, "Reference:" + referenceID) {

            InitializeComponent();

            Reference model = null;
            if (referenceID >= 0) {
                var service = new SupportService(user);
                model = service.GetReference(referenceID);
            } else {
                model = new Reference();
                model.RefID = -1;
                txtRefCode.IsReadOnly = false;
                Loaded += new RoutedEventHandler(ReferenceDetail_Loaded);                
            }

            _viewModel = new ReferenceViewModel(model);

            _viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);

            this.DataContext = _viewModel;

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
}
