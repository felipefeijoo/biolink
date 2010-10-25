using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpMap.Converters.Geometries;
using GeoAPI.Geometries;

namespace BioLink.Client.Maps {

    public class SerializedEnvelope {

        public SerializedEnvelope() {
        }

        public SerializedEnvelope(IEnvelope envelope) {
            MinX = envelope.MinX;
            MinY = envelope.MinY;
            MaxX = envelope.MaxX;
            MaxY = envelope.MaxY;
        }

        public IEnvelope CreateEnvelope() {
            return GeometryFactory.CreateEnvelope(MinX, MaxX, MinY, MaxY);
        }

        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public double MinX { get; set; }
        public double MinY { get; set; }

    }

}
