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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for SpecializedMultimediaManager.xaml
    /// </summary>
    public partial class SpecializedMultimediaManager : DatabaseCommandControl {

        private MultimediaControl _content;

        #region Designer Ctor
        public SpecializedMultimediaManager() {
            InitializeComponent();
        }
        #endregion

        public SpecializedMultimediaManager(User user, ToolsPlugin plugin, TraitCategoryType category, int intraCategoryId) : base(user, string.Format("Multimedia:{0}:{1}", category, intraCategoryId )) {
            InitializeComponent();
            this.Plugin = plugin;
            this.TraitCategory = category;
            this.IntraCategoryID = intraCategoryId;
            var proxy = new BuiltInProxyViewModel(intraCategoryId);
            _content = new MultimediaControl(User, category, proxy) { Margin = new Thickness(6) };
            this.Content = _content;

            Loaded += new RoutedEventHandler(SpecializedMultimediaManager_Loaded);

        }

        void SpecializedMultimediaManager_Loaded(object sender, RoutedEventArgs e) {
            _content.Populate();
        }

        protected ToolsPlugin Plugin { get; private set; }
        protected TraitCategoryType TraitCategory { get; private set; }
        protected int IntraCategoryID { get; private set; }

    }

    public class BuiltInProxyViewModel : ViewModelBase {

        public BuiltInProxyViewModel(int intraCatId = SupportService.BIOLINK_INTRA_CAT_ID) {
            this.IntraCategoryID = intraCatId;
        }

        public override int? ObjectID {
            get { return IntraCategoryID; }
        }

        public int IntraCategoryID { get; private set; }
    }

}
