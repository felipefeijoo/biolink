using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public interface IMapProvider {

        void Show();

        void DropAnchor(double longitude, double latitude, string caption);
        void HideAnchor();

        void PlotPoints(List<MapPoint> points);
        void ClearPoints();
    }

    public class MapPoint {

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // TODO - extra stuff in here for click events etc...

    }
}
