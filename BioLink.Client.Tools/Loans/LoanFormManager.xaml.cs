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
    /// Interaction logic for LoanFormManager.xaml
    /// </summary>
    public partial class LoanFormManager : DatabaseActionControl {

        private MultimediaControl _forms;

        public LoanFormManager(User user, ToolsPlugin plugin) : base(user, "LoanFormManager") {
            InitializeComponent();
            this.Plugin = plugin;
            var proxy = new BuiltInProxyViewModel();
           _forms = new MultimediaControl(User, TraitCategoryType.Biolink, proxy);
           this.Content = _forms;

           Loaded += new RoutedEventHandler(LoanFormManager_Loaded);
        }

        void LoanFormManager_Loaded(object sender, RoutedEventArgs e) {
            _forms.Populate();
        }

        protected ToolsPlugin Plugin { get; private set; }
    }

    public class BuiltInProxyViewModel : ViewModelBase {

        public override int? ObjectID {
            get { return SupportService.BIOLINK_INTRA_CAT_ID; }
        }
    }
}
