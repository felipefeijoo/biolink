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

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for SiteTooltipContent.xaml
    /// </summary>
    public partial class SiteTooltipContent : UserControl {

        public SiteTooltipContent(User user, int objectId, string elemType, string name) {
            InitializeComponent();
            User = user;
            ObjectID = objectId;
            ElemType = elemType;
            Name = name;
            Loaded += new RoutedEventHandler(SiteTooltipContent_Loaded);
        }

        void SiteTooltipContent_Loaded(object sender, RoutedEventArgs e) {
            var model = GetModel();
            imgIcon.Source = GetIcon();
            lblHeader.Content = Name;

            if (model != null) {

                var sb = new StringBuilder();

                switch (ElemType) {
                    case "Site":
                        var site = model as Site;
                        sb.AppendFormat("Political Region\t: {0}\n", site.PoliticalRegion);
                        sb.AppendFormat("Position        \t: {0}  {1}\n", GeoUtils.DecDegToDMS(site.PosX1.Value, CoordinateType.Longitude), GeoUtils.DecDegToDMS(site.PosY1.Value, CoordinateType.Latitude));
                        break;
                    case "SiteVisit":
                        var visit = model as SiteVisit;
                        sb.AppendFormat("Collector(s)\t: {0}\n", visit.Collector);
                        sb.AppendFormat("Start Date\t: {0:g}\n", visit.DateStart);
                        sb.AppendFormat("End Date\t: {0:g}\n", visit.DateEnd);
                        sb.AppendFormat("Site\t\t: {0}\n", visit.SiteName);
                        break;
                    case "Material":
                        var mat = model as Data.Model.Material;
                        sb.AppendFormat("Site\t\t: {0}\n", mat.SiteName);
                        sb.AppendFormat("Site Visit\t\t: {0}\n", mat.SiteVisitName);
                        sb.AppendFormat("Accession No.\t: {0}\n", mat.AccessionNumber);
                        sb.AppendFormat("Registration No.\t: {0}\n", mat.RegistrationNumber);
                        sb.AppendFormat("Identification\t: {0}\n", mat.TaxaDesc);
                        break;
                    case "Region":
                        var reg = model as Region;
                        sb.AppendFormat("Region type\t: {0}", reg.Rank);
                        break;                        
                }


                if (sb.Length > 0) {
                    txtDetails.Text = sb.ToString();
                }

                lblSystem.Content = string.Format("[{0}] {1}, Last Modified: {2:g} by {3}.", ObjectID, ElemType, model.DateLastUpdated, model.WhoLastUpdated);
            } else {
                lblSystem.Content = string.Format("[{0}] {1}.", ObjectID, ElemType);
            }
        }

        private OwnedDataObject GetModel() {
            var service = new MaterialService(User);
            OwnedDataObject obj = null;
            switch (ElemType) {
                case "Region":
                    obj = service.GetRegion(ObjectID);
                    break;
                case "Site":
                    obj = service.GetSite(ObjectID);
                    break;
                case "SiteVisit":
                    obj = service.GetSiteVisit(ObjectID);
                    break;
                case "Material":
                    obj = service.GetMaterial(ObjectID);
                    break;
                case "SiteGroup":                    
                    break;
                case "Trap":
                    obj = service.GetTrap(ObjectID);
                    break;
            }
            return obj;
        }

        protected string GetRelativeImagePath(string elemType) {
            return String.Format(@"images\{0}.png", elemType);
        }

        protected ImageSource GetIcon() {
            string assemblyName = this.GetType().Assembly.GetName().Name;
            return ImageCache.GetImage(String.Format("pack://application:,,,/{0};component/{1}", assemblyName, GetRelativeImagePath(ElemType)));
        }


        protected User User { get; private set; }

        protected int ObjectID { get; private set; }

        protected string ElemType { get; set; }

        protected string Name { get; set; }

    }
}
