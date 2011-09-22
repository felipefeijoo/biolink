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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data.Model;

namespace BioLink.Client.Gazetteer {

    public class PlaceNameViewModel : GenericViewModelBase<PlaceName> {

        public PlaceNameViewModel(PlaceName model) : base(model, null) { }

        public override string DisplayLabel {
            get {
                if (PlaceNameType == Data.Model.PlaceNameType.Location) {
                    return String.Format("[{0}] {1} - {2}  ({3},{4})", Division, Name, PlaceType, LatitudeString, LongitudeString);
                } else {
                    string place = string.Format("{0} {1} {2} of {3}", Offset, Units, Direction, Name);
                    return String.Format("[{0}] {1} - {2}  ({3},{4})", Division, place, PlaceType, LatitudeString, LongitudeString);
                }
            }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string PlaceType {
            get { return Model.PlaceType; }
            set { SetProperty(() => Model.PlaceType, value); }

        }

        public string Division {
            get { return Model.Division; }
            set { SetProperty(() => Model.Division, value); }
        }

        public string LatitudeString {
            get { return Model.LatitudeString; }
            set { SetProperty(() => Model.LatitudeString, value); }
        }

        public string LongitudeString {
            get { return Model.LongitudeString; }
            set { SetProperty(() => Model.LongitudeString, value); }
        }

        public double Latitude {
            get { return Model.Latitude; }
            set { SetProperty(() => Model.Latitude, value); }
        }

        public double Longitude {
            get { return Model.Longitude; }
            set { SetProperty(() => Model.Longitude, value); }
        }

        public PlaceNameType PlaceNameType {
            get { return Model.PlaceNameType; }
            set { SetProperty(() => Model.PlaceNameType, value); }
        }

        public string Offset {
            get { return Model.Offset; }
            set { SetProperty(() => Model.Offset, value); }
        }

        public string Units {
            get { return Model.Units; }
            set { SetProperty(() => Model.Units, value); }
        }

        public string Direction {
            get { return Model.Direction; }
            set { SetProperty(() => Model.Direction, value); }
        }

        public string Position {
            get { return GeoUtils.FormatCoordinates(Latitude, Longitude); }
        }

        public double Distance { get; set; }

    }
}
