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
using BioLink.Data;
using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for PinBoard.xaml
    /// </summary>
    public partial class PinBoard : UserControl {

        private ObservableCollection<ViewModelBase> _model = new ObservableCollection<ViewModelBase>();
        private List<IPinnable> _items = new List<IPinnable>();

        public PinBoard() {
            InitializeComponent();
        }

        public PinBoard(IBioLinkPlugin owner) {
            this.Owner = owner;
            InitializeComponent();
            this.DataContext = _model;
            lvw.ItemsSource = _model;
        }

        public void PersistPinnedItems() {
            Config.SetProfile(Owner.User, "Pinboard.PinnedItems", _items);
        }

        public void InitializePinBoard() {
            _items = Config.GetProfile(Owner.User, "Pinboard.PinnedItems", new List<IPinnable>());
        }

        public void Pin(IPinnable pinnable) {
            _items.Add(pinnable);
            _model.Add(pinnable.CreateViewModel());
        }

        public IEnumerable<IPinnable> PinnableItems { get { return _items; } }

        public IBioLinkPlugin Owner { get; private set; }

    }

    public interface IPinnable {

        ViewModelBase CreateViewModel();

        string PluginID { get; }

    }

    public abstract class GenericPinnable<T> : IPinnable where T : BiolinkDataObject {

        public abstract ViewModelBase CreateViewModel();

        public abstract string PluginID { get; }
        
    }

}
