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
using System.Collections.ObjectModel;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;


namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for TraitTypesControl.xaml
    /// </summary>
    public partial class TraitTypesControl : AdministrationComponent {

        private List<OneToManyTypeInfo> _typeData;
        private List<string> _categories;

        private KeyValuePair<string, LookupType>[] _typeMappings = new KeyValuePair<string, LookupType>[] {
            new KeyValuePair<string, LookupType>("MaterialID", LookupType.Material),
            new KeyValuePair<string, LookupType>("BiotaID", LookupType.Taxon),
            new KeyValuePair<string, LookupType>("SiteID", LookupType.Site),
            new KeyValuePair<string, LookupType>("SiteVisitID", LookupType.SiteVisit),
            new KeyValuePair<string, LookupType>("JournalID", LookupType.Journal),
            new KeyValuePair<string, LookupType>("ReferenceID", LookupType.Reference),
            new KeyValuePair<string, LookupType>("TrapID", LookupType.Trap)
        };

        public TraitTypesControl(User user)  : base(user) {
            InitializeComponent();
            cmbCategory.SelectionChanged += new SelectionChangedEventHandler(cmbCategory_SelectionChanged);
            lstTraitTypes.SelectionChanged += new SelectionChangedEventHandler(lstTraitTypes_SelectionChanged);
            lvwValues.SelectionChanged += new SelectionChangedEventHandler(lvwValues_SelectionChanged);
            lvwValues.MouseRightButtonUp += new MouseButtonEventHandler(lvwValues_MouseRightButtonUp);
        }

        void lvwValues_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {

            var selected = lvwValues.SelectedItem as TraitOwnerInfo;
            if (selected != null) {
                ContextMenuBuilder builder = new ContextMenuBuilder(null);
                builder.New("Edit owner").Handler(() => { EditOwner(selected); }).End();
                builder.New("Delete trait from owner").Handler(() => { DeleteTraitFromOwner(selected); }).End();

                lvwValues.ContextMenu = builder.ContextMenu;
            }
        }

        private void EditOwner(TraitOwnerInfo ownerInfo) {
            var ep = EntryPoint.Parse(ownerInfo.EntryPoint);

            LookupType? type = null;
            int objectId = -1; 

            foreach (KeyValuePair<string, LookupType> mapping in _typeMappings) {
                if (ep.HasParameter(mapping.Key)) {
                    type = mapping.Value;                    
                    objectId = Int32.Parse(ep[mapping.Key]);
                }
            }

            if (type.HasValue && objectId >= 0) {
                PluginManager.Instance.EditLookupObject(type.Value, objectId);
            } else {
                MessageBox.Show(String.Format("Can't edit object! Owner call: {0} Owner Name: {1}, OwnerID={2}, EntryPoint={3}", ownerInfo.OwnerCall, ownerInfo.OwnerName, ownerInfo.OwnerID, ownerInfo.EntryPoint));
            }
        }

        private void DeleteTraitFromOwner(TraitOwnerInfo ownerInfo) {
            throw new NotImplementedException();
        }

        void lvwValues_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            txtComment.DataContext = lvwValues.SelectedItem;
        }

        void lstTraitTypes_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (IsPopulated) {
                var info = lstTraitTypes.SelectedItem as OneToManyTypeInfo;
                if (info != null) {
                    JobExecutor.QueueJob(() => {
                        lock (lvwValues) {
                            try {
                                this.InvokeIfRequired(() => {
                                    lvwValues.Cursor = Cursors.Wait;
                                });
                                var list = Service.GetTraitOwnerInfo(info.ID);
                                this.InvokeIfRequired(() => {
                                    lvwValues.ItemsSource = new ObservableCollection<TraitOwnerInfo>(list);
                                });
                            } finally {
                                this.InvokeIfRequired(() => {
                                    lvwValues.Cursor = Cursors.Arrow;
                                });
                            }
                        }
                    });
                }
            }
        }

        void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (IsPopulated) {
                string category = cmbCategory.SelectedItem as string;
                if (category != null) {
                    var types = new ObservableCollection<OneToManyTypeInfo>(_typeData.FindAll((info) => info.Category.Equals(category)));
                    lstTraitTypes.ItemsSource = types;
                }
            }
        }

        public override void Populate() {
            _typeData = Service.GetTypeInfo("trait");
            var map = new Dictionary<string, string>();
            foreach (OneToManyTypeInfo info in _typeData) {
                if (!map.ContainsKey(info.Category)) {
                    map[info.Category] = info.Category;
                }
            }
            _categories = new List<string>(map.Values);
            _categories.Sort();

            cmbCategory.ItemsSource = _categories;
            cmbCategory.SelectedItem = _categories[0];

            IsPopulated = true;
        }
    }
}
