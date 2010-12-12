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
using System.Collections.ObjectModel;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for FindReferencesControl.xaml
    /// </summary>
    public partial class ReferenceManager : DatabaseActionControl, ISelectionHostControl {

        #region Designer Constructor
        public ReferenceManager() {
            InitializeComponent();
        }
        #endregion

        public ReferenceManager(User user, ToolsPlugin owner) 
            : base(user, "ReferenceManager") {
            InitializeComponent();
            this.Owner = owner;
            lvwResults.SelectionChanged += new SelectionChangedEventHandler((sender, e) => {
                var item = lvwResults.SelectedItem as ReferenceSearchResultViewModel;
                if (item != null) {
                    txtRTF.DataContext = item;
                } else {
                    txtRTF.Document.Blocks.Clear();
                }
            });

            ChangesCommitted += new PendingChangesCommittedHandler(ReferenceManager_ChangesCommitted);

            lvwResults.PreviewMouseRightButtonUp += new MouseButtonEventHandler(lvwResults_PreviewMouseRightButtonUp);
        }

        void ReferenceManager_ChangesCommitted(object sender) {
            // Redo the search...
            DoSearch();
        }

        void lvwResults_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            ContextMenuBuilder builder = new ContextMenuBuilder(null);

            builder.New("Add New").Handler(() => AddNew()).End();
            builder.Separator();
            builder.New("Delete").Handler(() => DeleteSelected()).End();
            builder.Separator();
            builder.New("Pin to pinboard").Handler(() => { PinSelected(); }).End();
            builder.New("Edit").Handler(() => EditSelected()).End();

            lvwResults.ContextMenu = builder.ContextMenu;
        }

        private void EditSelected() {
            var selected = lvwResults.SelectedItem as ReferenceSearchResultViewModel;
            if (selected != null) {
                Owner.EditReference(selected.RefID);
            }
        }

        private void AddNew() {
            Owner.AddNewReference();            
        }

        private void DeleteSelected() {
            var selected = lvwResults.SelectedItem as ReferenceSearchResultViewModel;
            if (selected != null) {
                selected.IsDeleted = true;
                RegisterUniquePendingChange(new DeleteReferenceAction(selected.RefID));                     
            }
        }

        private void PinSelected() {
            var selected = lvwResults.SelectedItem as ReferenceSearchResultViewModel;
            if (selected != null) {
                var pinnable = new PinnableObject(ToolsPlugin.TOOLS_PLUGIN_NAME, LookupType.Reference, selected.RefID, selected.DisplayLabel); // TODO need to do this properly!
                PluginManager.Instance.PinObject(pinnable);
            }
        }

        public SelectionResult Select() {
            var item = lvwResults.SelectedItem as ReferenceSearchResultViewModel;

            if (item != null) {
                var res = new ReferenceSelectionResult(item.Model);
                return res;
            }
            return null;

        }

        private void DoSearch() {

            lvwResults.ItemsSource = null;

            if (string.IsNullOrEmpty(txtAuthor.Text) && string.IsNullOrEmpty(txtCode.Text) && string.IsNullOrEmpty(txtOther.Text) && string.IsNullOrEmpty(txtYear.Text)) {
                ErrorMessage.Show("Please enter some search criteria");
                return;
            }

            List<ReferenceSearchResult> data = Service.FindReferences(Wildcard(txtCode.Text), Wildcard(txtAuthor.Text), Wildcard(txtYear.Text), Wildcard(txtOther.Text));

            lblStatus.Content = string.Format("{0} matching references found.", data.Count);

            var model = new ObservableCollection<ReferenceSearchResultViewModel>(data.ConvertAll(item => new ReferenceSearchResultViewModel(item)));
            lvwResults.ItemsSource = model;
        }

        private string Wildcard(string str) {
            if (String.IsNullOrEmpty(str)) {
                return null;
            }

            return str + "%";
        }

        protected SupportService Service { get { return new SupportService(User); } }

        protected ToolsPlugin Owner { get; private set; }

        private void btnFind_Click(object sender, RoutedEventArgs e) {
            DoSearch();
        }
    }

    public class ReferenceSelectionResult : SelectionResult {

        public ReferenceSelectionResult(ReferenceSearchResult data) {
            this.DataObject = data;
            this.Description = data.RefCode;
            this.ObjectID = data.RefID;
        }
        
    }

    public class ReferenceSearchResultViewModel : GenericViewModelBase<ReferenceSearchResult> {

        public ReferenceSearchResultViewModel(ReferenceSearchResult model)
            : base(model) {
        }

        public override string DisplayLabel {
            get {
                return String.Format("{0}, {1} [{2}] ({3})", Title, Author, YearOfPub, RefCode);
            }
            set {
                base.DisplayLabel = value;
            }
        }

        public int RefID {
            get { return Model.RefID; }
            set { SetProperty(() => Model.RefID, value); }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); }
        }

        public string Author {
            get { return Model.Author; }
            set { SetProperty(() => Model.Author, value); }
        }

        public string YearOfPub {
            get { return Model.YearOfPub; }
            set { SetProperty(() => Model.YearOfPub, value); }
        }

        public string Title {
            get { return Model.Title; }
            set { SetProperty(() => Model.Title, value); }
        }

        public string RefType {
            get { return Model.RefType; }
            set { SetProperty(() => Model.RefType, value); } 
        }

        public string RefRTF {
            get { return Model.RefRTF; }
            set { SetProperty(() => Model.RefRTF, value); }
        }

    }


}
