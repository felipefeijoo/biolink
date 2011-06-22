using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class PlaceName {        
        public string Name { get; set; }
        public string PlaceType { get; set; }
        public string Division { get; set; }
        public string LatitudeString { get; set; }
        public string LongitudeString { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }        
        public PlaceNameType PlaceNameType { get; set; }
        public string Offset { get; set; }
        public string Units { get; set; }
        public string Direction { get; set; }
    }

    public enum PlaceNameType {
        Location,
        OffsetAndDirection
    }

}
