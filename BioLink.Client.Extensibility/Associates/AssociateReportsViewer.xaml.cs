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
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections;


namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for AssociateReportsViewer.xaml
    /// </summary>
    public partial class AssociateReportsViewer : UserControl {

        public AssociateReportsViewer() {
            InitializeComponent();
        }

        public AssociateReportsViewer(IBioLinkReport report, DataMatrix data, IProgressObserver progress) {
            InitializeComponent();
            this.Report = report;
            this.DataMatrix = data;
            this.Progress = progress;

            Loaded += new RoutedEventHandler(AssociateReportsViewer_Loaded);
        }

        void AssociateReportsViewer_Loaded(object sender, RoutedEventArgs e) {

            var viewModels = DataMatrix.Tag as List<AssociateReportViewModel>;
            if (viewModels != null) {
                lst.ItemsSource = viewModels;
            }

            //if (Progress != null) {
            //    Progress.ProgressMessage("Extracting associate details...");
            //}

            //var index = DataMatrix.IndexOf("AssociateID");
            //var idList = new List<int>();
            //foreach (MatrixRow row in DataMatrix) {
            //    var associateId = (int) row[index];
            //    if (!idList.Contains(associateId)) {
            //        idList.Add(associateId);
            //    }
            //}

            //if (idList.Count > 0) {
            //    var service = new SupportService(PluginManager.Instance.User);
            //    var associates = service.GetAssociatesById(idList);

            //    var model = new List<AssociateReportViewModel>(associates.Select((m) => {
            //        return new AssociateReportViewModel(m);
            //    }));

            //    lst.ItemsSource = model;
            //}
        }

        protected IBioLinkReport Report { get; set; }

        protected DataMatrix DataMatrix { get; set; }

        protected IProgressObserver Progress { get; set; }

        private void Border_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {

            lst.ContextMenu = null;
            var selected = lst.SelectedItem as AssociateReportViewModel;
            if (selected != null) {
                var builder = new ContextMenuBuilder(null);
                builder.New("Edit " + selected.FromViewModel.DisplayLabel + " (" + selected.RelationFromTo + ")").Handler(() => {
                    EditAssociatedItem(selected.Model.FromCatID, selected.Model.FromIntraCatID);
                }).End();

                builder.Separator();

                builder.New("Pin " + selected.FromViewModel.DisplayLabel + " (" + selected.RelationFromTo + ") to pin board").Handler(() => PinAssociatedItem(selected.Model.FromCatID, selected.Model.FromIntraCatID)).End();

                lst.ContextMenu = builder.ContextMenu;
            }
        }

        private void EditAssociatedItem(int catId, int intraCatId) {
            switch (catId) {
                case 1: // Material
                    PluginManager.Instance.EditLookupObject(LookupType.Material, intraCatId);                    
                    break;
                case 2: // Taxon
                    PluginManager.Instance.EditLookupObject(LookupType.Taxon, intraCatId);                    
                    break;
                default:
                    // Don't think this should ever happen!
                    ErrorMessage.Show("Error!");
                    break;
            }

        }

        private void PinAssociatedItem(int catId, int intraCatId) {
            LookupType type = LookupType.Unknown;
            IBioLinkPlugin plugin = null;
            switch (catId) {
                case 1: // Material
                    type = LookupType.Material;
                    plugin = PluginManager.Instance.GetLookupTypeOwner(LookupType.Material);
                    break;
                case 2: // Taxon
                    type = LookupType.Taxon;
                    plugin = PluginManager.Instance.GetLookupTypeOwner(LookupType.Taxon);
                    break;
            }

            if (plugin != null) {
                PluginManager.Instance.PinObject(new PinnableObject(plugin.Name, type, intraCatId));
            }

        }


        private void Border_MouseRightButtonUp_1(object sender, MouseButtonEventArgs e) {
            lst.ContextMenu = null;
            var selected = lst.SelectedItem as AssociateReportViewModel;
            if (selected != null && selected.Model.ToIntraCatID.HasValue) {
                var builder = new ContextMenuBuilder(null);
                builder.New("Edit " + selected.ToViewModel.DisplayLabel + " (" + selected.RelationToFrom + ")").Handler(() => {
                    EditAssociatedItem(selected.Model.ToCatID.Value, selected.Model.ToIntraCatID.Value);
                }).End();

                lst.ContextMenu = builder.ContextMenu;

                builder.Separator();

                builder.New("Pin " + selected.ToViewModel.DisplayLabel + " (" + selected.RelationToFrom + ") to pin board").Handler(() => PinAssociatedItem(selected.Model.ToCatID.Value, selected.Model.ToIntraCatID.Value)).End();
            }

        }
        
    }

    public class AssociateReportViewModel : GenericViewModelBase<Associate>{

        public AssociateReportViewModel(Associate model) : base(model, ()=>model.AssociateID) { }

        public ViewModelBase FromViewModel { get; set; }

        public ViewModelBase ToViewModel { get; set; }

        public String RelationFromTo {
            get { return Model.RelationFromTo; }
        }

        public String RelationToFrom {
            get { return Model.RelationToFrom; }
        }

        public int AssociateID {
            get { return Model.AssociateID; }
        }

    }    

    public class AssociateReportsViewerSource : IReportViewerSource {

        public string Name {
            get { return "Associates"; }
        }

        public FrameworkElement ConstructView(IBioLinkReport report, DataMatrix reportData, IProgressObserver progress) {
            AssociateReportsViewer viewer = new AssociateReportsViewer(report, reportData, progress);
            return viewer;
        }

    }

}
