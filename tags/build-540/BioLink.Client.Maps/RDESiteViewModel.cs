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
using BioLink.Data.Model;

namespace BioLink.Client.Maps {

    public class RDESiteViewModel : GenericViewModelBase<RDESite> {

        public RDESiteViewModel(RDESite model) : base(model, ()=>model.SiteID) { }

        public int SiteID {
            get { return Model.SiteID; }
            set { SetProperty(() => Model.SiteID, value); }
        }

        public int ParentID {
            get { return Model.ParentID; }
            set { SetProperty(() => Model.ParentID, value); }
        }

        public string SiteName {
            get { return Model.SiteName; }
            set { SetProperty(() => Model.SiteName, value); }
        }

        public byte? LocalType {
            get { return Model.LocalType; }
            set { SetProperty(() => Model.LocalType, value); }
        }

        public string Locality {
            get { return Model.Locality; }
            set { SetProperty(() => Model.Locality, value); }
        }

        public int? PoliticalRegionID {
            get { return Model.PoliticalRegionID; }
            set { SetProperty(() => Model.PoliticalRegionID, value); }
        }

        public string PoliticalRegion {
            get { return Model.PoliticalRegion; }
            set { SetProperty(() => Model.PoliticalRegion, value); }
        }

        public byte? PosAreaType {
            get { return Model.PosAreaType; }
            set { SetProperty(() => Model.PosAreaType, value); }
        }

        public byte? PosCoordinates {
            get { return Model.PosCoordinates; }
            set { SetProperty(() => Model.PosCoordinates, value); }
        }

        public double? Longitude {
            get { return Model.Longitude; }
            set { SetProperty(()=>Model.Longitude, value); }
        }

        public double? Latitude { 
            get { return Model.Latitude; }
            set { SetProperty(()=>Model.Latitude, value); }
        }

        public double? Longitude2 { 
            get { return Model.Longitude2; }
            set { SetProperty(()=>Model.Longitude2, value); }
        }

        public double? Latitude2 { 
            get { return Model.Latitude2; }
            set { SetProperty(() => Model.Latitude2, value); }
        }

        public byte? ElevType {
            get { return Model.ElevType; }
            set { SetProperty(() => Model.ElevType, value); }
        }

        public double? ElevUpper {
            get { return Model.ElevUpper; }
            set { SetProperty(() => Model.ElevUpper, value); }
        }

        public double? ElevLower {
            get { return Model.ElevLower; }
            set { SetProperty(() => Model.ElevLower, value); }
        }

        public string ElevUnits {
            get { return Model.ElevUnits; }
            set { SetProperty(() => Model.ElevUnits, value); }
        }

        public string ElevSource {
            get { return Model.ElevSource; }
            set { SetProperty(() => Model.ElevSource, value); }
        }

        public string ElevError {
            get { return Model.ElevError; }
            set { SetProperty(() => Model.ElevError, value); }
        }

        public string LLSource {
            get { return Model.LLSource; }
            set { SetProperty(() => Model.LLSource, value); }
        }

        public string LLError {
            get { return Model.LLError; }
            set { SetProperty(() => Model.LLError, value); }
        }

        public int? Changes {
            get { return Model.Changes; }
            set { SetProperty(() => Model.Changes, value); }
        }

        public int? TemplateID {
            get { return Model.TemplateID; }
            set { SetProperty(() => Model.TemplateID, value); }
        }

        public bool Locked {
            get { return Model.Locked; }
            set { SetProperty(() => Model.Locked, value); }
        }

    }
}
