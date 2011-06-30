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

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for TaxonTooltipContent.xaml
    /// </summary>
    public partial class TaxonTooltipContent : UserControl {

        public TaxonTooltipContent(User user, int taxonId) {
            InitializeComponent();
            TaxonID = taxonId;            
            User = user;
            Loaded += new RoutedEventHandler(TaxonTooltipContent_Loaded);
        }


        void TaxonTooltipContent_Loaded(object sender, RoutedEventArgs e) {
            var service = new TaxaService(PluginManager.Instance.User);
            var Model = service.GetTaxon(TaxonID);
            if (Model == null) {
                return;
            }

            var elementRank = service.GetTaxonRank(Model.ElemType);

            lblHeader.Content = Model.TaxaFullName;
            var rankName = (elementRank == null ? "Unranked" : elementRank.LongName);
            lblSystem.Content = string.Format("[{0}] {1} Last updated: {2:g} by {3}", Model.TaxaID.Value, rankName, Model.DateLastUpdated, Model.WhoLastUpdated);
            imgIcon.Source = TaxonViewModel.ConstructIcon(Model.AvailableName.ValueOrFalse() || Model.LiteratureName.ValueOrFalse(), Model.ElemType, false);            

            // Ancestry
            var bits = Model.Parentage.Split('\\');

            int i = bits.Length - 1;
            int j = 0;
            var parents = new Stack<Taxon>();
            while (--i >= 0 && j++ < 3) {
                if (!string.IsNullOrEmpty(bits[i])) {
                    var parentId = Int32.Parse(bits[i]);
                    var parent = service.GetTaxon(parentId);
                    parents.Push(parent);
                }
            }

            i = 0;
            grdAncestry.Children.Clear();
            
            foreach (Taxon t in parents) {
                var parentPanel = new StackPanel();
                parentPanel.Orientation = Orientation.Horizontal;
                parentPanel.Margin = new Thickness(i * 15, i * 25, 0, 0);

                var parentIcon = new Image();
                parentIcon.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                parentIcon.Source = TaxonViewModel.ConstructIcon(t.AvailableName.ValueOrFalse() || t.LiteratureName.ValueOrFalse(), t.ElemType, false);
                parentIcon.UseLayoutRounding = true;
                parentIcon.SnapsToDevicePixels = true;
                parentIcon.Stretch = Stretch.None;
                parentIcon.Margin = new Thickness(6, 0, 6, 0);
                parentPanel.Children.Add(parentIcon);

                var rank = service.GetTaxonRank(t.ElemType);
                rankName = (rank == null ? "Unranked" : rank.LongName);
                var txt = new TextBlock();
                txt.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                txt.Text = string.Format("{1}   ({0})", rankName, t.TaxaFullName);
                parentPanel.Children.Add(txt);
                grdAncestry.Children.Add(parentPanel);
                i++;
            }

        }

        protected int TaxonID { get; private set; }        

        protected User User { get; private set; }
    }
}
