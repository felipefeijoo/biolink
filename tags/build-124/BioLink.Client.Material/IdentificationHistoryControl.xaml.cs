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
using System.Collections.ObjectModel;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for IdentificationHistoryControl.xaml
    /// </summary>
    public partial class IdentificationHistoryControl : DatabaseActionControl {

        #region Designer Constructor
        public IdentificationHistoryControl() {
            InitializeComponent();
        }
        #endregion

        public IdentificationHistoryControl(User user, int materialID) : base(user, "MaterialIDHistory:" + materialID) {
            InitializeComponent();
            this.MaterialID = materialID;
            LoadIDHistory();
        }

        public void LoadIDHistory() {
            var service = new MaterialService(User);
            var list = service.GetMaterialIdentification(MaterialID);
            var model = new ObservableCollection<MaterialIdentificationViewModel>(list.ConvertAll((m) => {
                var viewModel = new MaterialIdentificationViewModel(m);
                viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
                return viewModel;
            }));

            lst.SelectionChanged += new SelectionChangedEventHandler(lst_SelectionChanged);

            detailGrid.IsEnabled = false;

            lst.ItemsSource = model;
            if (model.Count > 0) {
                lst.SelectedItem = model[0];
            }
        }

        void lst_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            detailGrid.IsEnabled = lst.SelectedItem != null;
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            
        }

        public int MaterialID { get; private set; }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {

        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {

        }


    }

    public class MaterialIdentificationViewModel : GenericViewModelBase<MaterialIdentification> {

        public MaterialIdentificationViewModel(MaterialIdentification model)
            : base(model) {
                DisplayLabel = string.Format("{0} by {1} on {2:d MMM, yyyy}", Taxa, IDBy, IDDate);
        }

        public int MaterialIdentID {
            get { return Model.MaterialIdentID; }
            set { SetProperty(() => Model.MaterialIdentID, value); }
        }

        public int MaterialID {
            get { return Model.MaterialID; }
            set { SetProperty(() => Model.MaterialID, value); }
        }

        public string Taxa {
            get { return Model.Taxa; }
            set { SetProperty(() => Model.Taxa, value); }
        }

        public string IDBy {
            get { return Model.IDBy; }
            set { SetProperty(() => Model.IDBy, value); }
        }

        public DateTime IDDate {
            get { return Model.IDDate; }
            set { SetProperty(() => Model.IDDate, value); }
        }

        public int IDRefID {
            get { return Model.IDRefID; }
            set { SetProperty(() => Model.IDRefID, value); }
        }

        public string IDMethod {
            get { return Model.IDMethod; }
            set { SetProperty(() => Model.IDMethod, value); }
        }

        public string IDAccuracy {
            get { return Model.IDAccuracy; }
            set { SetProperty(() => Model.IDAccuracy, value); }
        }

        public string NameQual {
            get { return Model.NameQual; }
            set { SetProperty(() => Model.NameQual, value); }
        }

        public string IDNotes {
            get { return Model.IDNotes; }
            set { SetProperty(() => Model.IDNotes, value); }
        }

        public string IDRefPage {
            get { return Model.IDRefPage; }
            set { SetProperty(() => Model.IDRefPage, value); }
        }

        public int BasedOnID {
            get { return Model.BasedOnID; }
            set { SetProperty(() => Model.BasedOnID, value); }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); }
        }

    }
}
