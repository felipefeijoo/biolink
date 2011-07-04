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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for LinkedMultimediaItemsControl.xaml
    /// </summary>
    public partial class LinkedMultimediaItemsControl : UserControl, IIdentifiableContent {
        public LinkedMultimediaItemsControl(int multimediaId) {
            InitializeComponent();
            this.MultimediaID = multimediaId;            
            var service = new SupportService(User);
            var items = service.ListItemsLinkedToMultimedia(multimediaId);
            var model = new ObservableCollection<ViewModelBase>();

            foreach (MultimediaLinkedItem item in items) {
                if ( !string.IsNullOrWhiteSpace(item.CategoryName)) {
                    LookupType t;
                    if (Enum.TryParse<LookupType>(item.CategoryName, out t)) {
                        var viewModel = PluginManager.Instance.GetViewModel(t, item.IntraCatID);
                        if (viewModel != null) {
                            model.Add(viewModel);
                        }
                    }
                }
            }

            lvw.ItemsSource = model;
        }

        public User User { get { return PluginManager.Instance.User; } }

        public int MultimediaID { get; private set; }

        public string ContentIdentifier {
            get { return "ItemsForMultimedia:" + MultimediaID; }
        }

        public void RefreshContent() {
            throw new NotImplementedException();
        }
    }
}
