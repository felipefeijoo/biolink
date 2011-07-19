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


namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for MultimediaDetails.xaml
    /// </summary>
    public partial class MultimediaDetails : DatabaseActionControl {

        #region Designer Constructor
        public MultimediaDetails() {
            InitializeComponent();
        }
        #endregion

        public MultimediaDetails(Multimedia multimedia, User user) : base(user, "Multimedia::" + multimedia.MultimediaID) {
            InitializeComponent();
            this.Multimedia = new MultimediaViewModel(multimedia);
            AddTabItem("Traits", new TraitControl(User, TraitCategoryType.Multimedia, Multimedia));
            AddTabItem("Ownership", new OwnershipDetails(multimedia));
            txtArtist.BindUser(user, "tblMultimedia", "vchrArtist");
            txtOwner.BindUser(user, "tblMultimedia", "vchrOwner");
            txtIDNumber.BindUser(user, "MultimediaID", "tblMultimedia", "vchrNumber");
            this.DataContext = this.Multimedia;

            Multimedia.DataChanged += new DataChangedHandler(Multimedia_DataChanged);
        }

        void Multimedia_DataChanged(ChangeableModelBase viewmodel) {
            RegisterUniquePendingChange(new UpdateMultimediaCommand(Multimedia.Model));
        }

        private TabItem AddTabItem(string title, UIElement content, Action bringIntoViewAction = null) {
            TabItem tabItem = new TabItem();
            tabItem.Header = title;
            tabItem.Content = content;
            tab.Items.Add(tabItem);
            if (bringIntoViewAction != null) {
                tabItem.RequestBringIntoView += new RequestBringIntoViewEventHandler((s, e) => {
                    bringIntoViewAction();
                });
            }

            return tabItem;
        }

        #region Properties

        public MultimediaViewModel Multimedia { get; private set; }

        #endregion

    }


}
