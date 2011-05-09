using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    [Serializable()]
    public abstract class RDEObject : BioLinkDataObject {

        public bool Locked { get; set; }
        public int? Changes { get; set; }
        public int? TemplateID { get; set; }

    }

    [Serializable()]
    public class RDESite : RDEObject {

        public RDESite() {
            SiteID = -1;
        }

        public int SiteID { get;set; }
        public int ParentID	{get;set;}
        public string SiteName { get;set;}	
        public byte? LocalType {get; set; }
        public string Locality{get;set;}
        public int? PoliticalRegionID { get;set;}
        public string PoliticalRegion { get; set; }
        public byte? PosAreaType { get; set; }
        public byte? PosCoordinates { get; set; }
        public double? Longitude{ get; set; }
        public double? Latitude { get; set; }
        public double? Longitude2 { get; set; }
        public double? Latitude2 { get; set; }
        public byte? ElevType { get; set; }
        public double? ElevUpper { get; set; }
        public double? ElevLower	{ get; set; }
        public string ElevUnits	{ get; set; }
        public string ElevSource { get; set; }
        public string ElevError	{ get; set; }
        public string LLSource	{ get; set; }
        public string LLError	{ get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.SiteID; }
        }
        
    }
}
