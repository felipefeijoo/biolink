using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace BioLink.Client.Extensibility {

    public interface IMapProvider {

        void Show();

        void DropAnchor(double longitude, double latitude, string caption);
        void HideAnchor();

        void PlotPoints(MapPointSet points);
        void ClearPoints();
    }

    public class MapPointSet : List<MapPoint> {

        public MapPointSet(string name) : base() {
            this.Name = name;
            this.PointColor = Colors.Red;
            this.DrawOutline = true;
            this.OutlineColor = Colors.Black;
            this.PointShape = MapPointShape.Circle;
            this.Size = 7;
        }

        public String Name { get; set; }
        public Color PointColor { get; set; }
        public MapPointShape PointShape { get; set; }
        public bool DrawOutline { get; set; }
        public Color OutlineColor { get; set; }
        public int Size { get; set; }
        public bool DrawLabels { get; set; } 

    }

    public enum MapPointShape {
        Circle,
        Square,
        Triangle
    }

    public class MapPoint {

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Label { get; set; }

        // TODO - extra stuff in here for click events etc...

    }
}
