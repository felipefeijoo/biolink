using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class SiteNodeTooltipContent : TooltipContentBase {

        public SiteNodeTooltipContent(int objectId, ViewModelBase viewModel, string elemType, string name) : base(objectId, viewModel) {
            ElemType = elemType;
            ElemName = name;
        }

        protected override string Title {
            get { return ElemName; }
        }

        protected override Data.Model.OwnedDataObject GetModel() {
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

        private string ElemType { get; set; }

        public string ElemName { get; set; }

        protected override void GetDetailText(OwnedDataObject model, TextTableBuilder builder) {
            
            if (model != null) {

                switch (ElemType) {
                    case "Site":
                        var site = model as Site;
                        builder.Add("Political Region", site.PoliticalRegion);
                        if (site.PosY1.HasValue && site.PosX1.HasValue) {
                            builder.Add("Position", string.Format("{0}  {1}", GeoUtils.DecDegToDMS(site.PosX1.Value, CoordinateType.Longitude), GeoUtils.DecDegToDMS(site.PosY1.Value, CoordinateType.Latitude)));
                        } else {
                            builder.Add("Position", "No position recorded.");
                        }
                        break;
                    case "SiteVisit":
                        var visit = model as SiteVisit;
                        builder.Add("Collector(s)", visit.Collector);
                        builder.AddFormat("Start Date", "{0}", DateUtils.BLDateToStr(visit.DateStart, "Not specified"));
                        builder.AddFormat("End Date", "{0}", DateUtils.BLDateToStr(visit.DateEnd, "Not specified"));
                        builder.Add("Site", visit.SiteName);
                        break;
                    case "Material":
                        var mat = model as Data.Model.Material;
                        builder.Add("Site", mat.SiteName);
                        builder.Add("Site Visit", mat.SiteVisitName);
                        builder.Add("Accession No.", mat.AccessionNumber);
                        builder.Add("Registration No.", mat.RegistrationNumber);
                        builder.Add("Identification", mat.TaxaDesc);
                        break;
                    case "Region":
                        var reg = model as Region;
                        if (string.IsNullOrWhiteSpace(reg.Rank)) {
                            builder.Add("Region type", "Not specified");
                        } else {
                            builder.Add("Region type", reg.Rank);
                        }
                        break;
                    case "Trap":
                        var trap = model as Trap;
                        builder.Add("Site", trap.SiteName);
                        builder.Add("Trap type", trap.TrapType);
                        builder.Add("Description", trap.Description);
                        break;
                }
            }            
        }
    }
}
