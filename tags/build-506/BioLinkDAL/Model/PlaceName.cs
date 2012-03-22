/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
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
