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
            if (Progress != null) {
                Progress.ProgressMessage("Extracting associate details...");
            }

            var index = DataMatrix.IndexOf("AssociateID");
            var idList = new List<int>();
            foreach (MatrixRow row in DataMatrix) {
                var associateId = (int) row[index];
                if (!idList.Contains(associateId)) {
                    idList.Add(associateId);
                }
            }

            if (idList.Count > 0) {
                var service = new SupportService(PluginManager.Instance.User);
                var associates = service.GetAssociatesById(idList);

                var model = new List<AssociateReportViewModel>(associates.Select((m) => {
                    return new AssociateReportViewModel(m);
                }));

                lst.ItemsSource = model;
            }
        }

        protected IBioLinkReport Report { get; set; }

        protected DataMatrix DataMatrix { get; set; }

        protected IProgressObserver Progress { get; set; }
        
    }

    public class AssociateReportViewModel : GenericViewModelBase<Associate>{

        public AssociateReportViewModel(Associate model) : base(model, ()=>model.AssociateID) {

            switch (Model.FromCatID) {
                case 1: // Material
                    FromViewModel = GetViewModel(LookupType.Material, Model.FromIntraCatID);
                    break;
                case 2:
                    FromViewModel = GetViewModel(LookupType.Taxon, Model.FromIntraCatID);
                    break;
                default:
                    FromViewModel = new ViewModelPlaceholder(Model.AssocDescription, "images/Description.png");
                    break;
            }

            switch (Model.ToCatID) {
                case 1: // Material
                    ToViewModel = GetViewModel(LookupType.Material, Model.ToIntraCatID);
                    break;
                case 2:
                    ToViewModel = GetViewModel(LookupType.Taxon, Model.ToIntraCatID);
                    break;
                default:
                    ToViewModel = new ViewModelPlaceholder(Model.AssocDescription, "images/Description.png");
                    break;
            }

        }

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

        private ViewModelBase GetViewModel(LookupType lookupType, int? objectId) {
            if (objectId.HasValue) {
                var plugin = PluginManager.Instance.GetLookupTypeOwner(lookupType);
                if (plugin != null) {
                    var pin = new PinnableObject(plugin.Name, lookupType, objectId.Value);
                    return PluginManager.Instance.GetViewModel(pin);
                }
            }
            return null;
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
