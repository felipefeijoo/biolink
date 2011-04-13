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
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using BioLink.Data;
using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility {

    public partial class AutoNumberCategoryManager : DatabaseActionControl {

        private ObservableCollection<AutoNumberViewModel> _model;

        #region Designer Constructor
        public AutoNumberCategoryManager() {
            InitializeComponent();
        }
        #endregion

        public AutoNumberCategoryManager(User user, string autoNumberCategory) : base(user, "AutomaticNumberCategoryEditor") {
            InitializeComponent();
            this.AutoNumberCategory = autoNumberCategory;
            LoadModel();

        }

        private void LoadModel() {
            var service = new SupportService(User);
            var list = service.GetAutoNumbersForCategory(AutoNumberCategory);
            _model = new ObservableCollection<AutoNumberViewModel>(list.ConvertAll((model) => {
                var viewmodel = new AutoNumberViewModel(model);
                viewmodel.DataChanged += new DataChangedHandler((m) => {
                    RegisterUniquePendingChange(new UpdateAutoNumberAction(model));
                });
                return viewmodel;
            }));
            lst.ItemsSource = _model;

            gridAutonumber.IsEnabled = false;

            if (_model.Count > 0) {
                lst.SelectedItem = _model[0];
            }
        }

        private void lst_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            gridAutonumber.DataContext = lst.SelectedItem;
            gridAutonumber.IsEnabled = lst.SelectedItem != null;
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {
            AddNew();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteCurrent();
        }

        private void AddNew() {
            var model = new AutoNumber();
            model.AutoNumberCatID = -1;
            model.Category = AutoNumberCategory;
            model.Name = "<New Autonumber>";

            RegisterPendingChange(new InsertAutoNumberAction(model));

            var viewmodel = new AutoNumberViewModel(model);
            _model.Add(viewmodel);
            viewmodel.IsSelected = true;
            lst.SelectedItem = viewmodel;

            
        }

        private void DeleteCurrent() {
            var selected = lst.SelectedItem as AutoNumberViewModel;
            if (selected != null) {
                RegisterUniquePendingChange(new DeleteAutoNumberAction(selected.Model));
                _model.Remove(selected);
            }
        }

        #region Properties

        internal SupportService Service { get { return new SupportService(User); } }

        public string AutoNumberCategory { get; private set; }

        #endregion


    }

    public class DeleteAutoNumberAction : GenericDatabaseAction<AutoNumber> {

        public DeleteAutoNumberAction(AutoNumber model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteAutoNumberCategory(Model.AutoNumberCatID);            
        }
    }

    public class InsertAutoNumberAction : GenericDatabaseAction<AutoNumber> {

        public InsertAutoNumberAction(AutoNumber model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.InsertAutoNumber(Model.Category, Model.Name, Model.Prefix, Model.Postfix, Model.NumLeadingZeros, Model.EnsureUnique);
        }

    }

    public class UpdateAutoNumberAction : GenericDatabaseAction<AutoNumber> {
        public UpdateAutoNumberAction(AutoNumber model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateAutoNumber(Model.AutoNumberCatID, Model.Name, Model.Prefix, Model.Postfix, Model.NumLeadingZeros, Model.EnsureUnique);
        }
    }
}
