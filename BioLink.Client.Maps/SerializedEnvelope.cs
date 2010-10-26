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
