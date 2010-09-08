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
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for TraitControl.xaml
    /// </summary>
    public partial class TraitControl : UserControl {

        private ObservableCollection<TraitViewModel> _model;

        #region Designer Constructor
        public TraitControl() {
            InitializeComponent();
        }
        #endregion

        public TraitControl(User user, TraitCategoryType category, int? nounId) {            
            InitializeComponent();

            if (nounId.HasValue) {
                SupportService s = new SupportService(user);
                var list = s.GetTraits(category.ToString(), nounId.Value);
                var modellist = list.Select((t) => {
                    return new TraitViewModel(t);
                });
                _model = new ObservableCollection<TraitViewModel>(modellist);
            }

            foreach (TraitViewModel m in _model) {
                traitsPanel.Children.Add(new TraitElementControl(m));
            }
            
        }
    }

    public class TraitViewModel : GenericViewModelBase<Trait> {

        public TraitViewModel(Trait t)
            : base(t) {            
        }

        public int TraitID {
            get { return Model.TraitID; }
            set { SetProperty(() => Model.TraitID, value); }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string DataType {
            get { return Model.DataType; }
            set { SetProperty(() => Model.DataType, value); }
        }

        public string Validation {
            get { return Model.Validation; }
            set { SetProperty(() => Model.Validation, value); }
        }

        public int IntraCatID {
            get { return Model.IntraCatID; }
            set { SetProperty(() => Model.IntraCatID, value); }
        }

        public string Value {
            get { return Model.Value; }
            set { SetProperty(() => Model.Value, value); }
        }

        public string Comment {
            get { return Model.Comment; }
            set { SetProperty(() => Model.Value, value); }
        }

    }

    public enum TraitCategoryType {
        Material,
        Taxon,
        Site,
        Trap,
        SiteVisit,
    }
}
