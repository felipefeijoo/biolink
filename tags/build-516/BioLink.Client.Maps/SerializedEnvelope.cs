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
using SharpMap.Geometries;
using GeoAPI.Geometries;

namespace BioLink.Client.Maps {

    public class SerializedEnvelope {

        public SerializedEnvelope() {
        }

        public SerializedEnvelope(BoundingBox envelope) {
            MinX = envelope.Left;
            MinY = envelope.Bottom;
            MaxX = envelope.Right;
            MaxY = envelope.Top;
        }

        public BoundingBox CreateBoundingBox() {
            return new BoundingBox(MinX, MinY, MaxX, MaxY);            
        }

        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public double MinX { get; set; }
        public double MinY { get; set; }

    }

}
