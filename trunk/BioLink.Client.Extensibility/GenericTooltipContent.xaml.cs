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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for GenericTooltipContent.xaml
    /// </summary>
    public partial class GenericTooltipContent : UserControl {

        public GenericTooltipContent(User user, OwnedDataObject model, ViewModelBase viewModel) {
            InitializeComponent();
            User = user;
            Model = model;
            ViewModel = viewModel;
            Loaded += new RoutedEventHandler(GenericTooltipContent_Loaded);
        }

        void GenericTooltipContent_Loaded(object sender, RoutedEventArgs e) {

            imgIcon.Source = ViewModel.Icon;
            lblHeader.Content = ViewModel.DisplayLabel;

            if (Model != null) {
                var sb = new StringBuilder();
                sb.AppendFormat("Guid\t:", Model.GUID.ToString());
                txtDetails.Text = sb.ToString();
            }

            lblSystem.Content = String.Format("[{0}] {1}, modified by {2:g} on {3}.", Model.ObjectID, Model.GetType().Name, Model.DateLastUpdated, Model.WhoLastUpdated);
        }

        protected User User { get; private set; }

        protected OwnedDataObject Model { get; private set; }

        protected ViewModelBase ViewModel { get; private set; }
    }
}
