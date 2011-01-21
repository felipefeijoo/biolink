using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using BioLink.Data;

namespace BioLink.Client.Extensibility {

    public interface IMapProvider {

        void Show();

        void DropAnchor(double longitude, double latitude, string caption);
        void HideAnchor();

        void PlotPoints(MapPointSet points);
        void ClearPoints();
    }

    public abstract class MapPointSet : IEnumerable<MapPoint> {

        public MapPointSet(string name) {
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


        public abstract IEnumerator<MapPoint> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    public class MatrixMapPointSet : MapPointSet {

        private DataMatrix _data;
        private int[] _selectedRowIndexes;

        public MatrixMapPointSet(string name, DataMatrix data, int[] selectedRowIndexes = null)
            : base(name) {
            _data = data;
            _selectedRowIndexes = selectedRowIndexes;
            // Default column names...
            LatitudeColumn = "Lat";
            LongitudeColumn = "Long";
            SiteIDColumn = "SiteID";
            SiteVisitIDColumn = "SiteVisitID";
            MaterialIDColumn = "MaterialID";
        }

        public override IEnumerator<MapPoint> GetEnumerator() {
            return new MatrixMapPointSetEnumerator(this, _selectedRowIndexes);
        }

        public string LatitudeColumn { get; set; }
        public string LongitudeColumn { get; set; }
        public string SiteIDColumn { get; set; }
        public string SiteVisitIDColumn { get; set; }
        public string MaterialIDColumn { get; set; }

        class MatrixMapPointSetEnumerator : IEnumerator<MapPoint> {

            private int _currentRow;
            private MatrixMapPointSet _set;
            private int[] _activeRows;

            private int _latIndex;
            private int _lonIndex;
            private int _siteIndex;
            private int _siteVisitIndex; 
            private int _materialIndex;


            public MatrixMapPointSetEnumerator(MatrixMapPointSet set, int[] activeRows = null) {
                _currentRow = -1;
                _set = set;
                _activeRows = activeRows;
                _latIndex = _set._data.IndexOf(_set.LatitudeColumn);
                _lonIndex = _set._data.IndexOf(_set.LongitudeColumn);
                _siteIndex = _set._data.IndexOf(_set.SiteIDColumn);
                _siteVisitIndex = _set._data.IndexOf(_set.SiteVisitIDColumn);
                _materialIndex = _set._data.IndexOf(_set.MaterialIDColumn);

            }

            public MapPoint Current {
                get {
                    int realIndex = _currentRow;

                    if (_activeRows != null) {
                        realIndex = _activeRows[_currentRow];
                    }

                    MatrixRow row = _set._data.Rows[realIndex];
                    var latObj = row[_latIndex];
                    var lonObj = row[_lonIndex];                    
                    double lat = latObj == null ? 0 : (double) latObj;
                    double lon = lonObj == null ? 0 : (double) lonObj;
                    var mp = new MapPoint(lat, lon);

                    if (_siteIndex >= 0) {
                        mp.SiteID = (int)row[_siteIndex];
                    }
                    if (_siteVisitIndex >= 0) {
                        mp.SiteVisitID = (int)row[_siteVisitIndex];
                    }
                    if (_materialIndex >= 0) {
                        mp.MaterialID = (int)row[_materialIndex];
                    }

                    return mp;

                }
            }

            public void Dispose() {
                _set = null;
            }

            object System.Collections.IEnumerator.Current {
                get { return Current; }
            }

            public bool MoveNext() {
                _currentRow++;
                if (_activeRows != null) {
                    return _currentRow < _activeRows.Length;
                } else {
                    return _currentRow < _set._data.Rows.Count;
                }
            }

            public void Reset() {
                _currentRow = -1;
            }
        }
    }



    public class ListMapPointSet : MapPointSet, IList<MapPoint> {

        private List<MapPoint> _impl = new List<MapPoint>();

        public ListMapPointSet(string name)
            : base(name) {
        }

        public int IndexOf(MapPoint item) {
            return _impl.IndexOf(item);
        }

        public void Insert(int index, MapPoint item) {
            _impl.Insert(index, item);
        }

        public void RemoveAt(int index) {
            _impl.RemoveAt(index);
        }

        public MapPoint this[int index] {
            get {
                return _impl[index];
            }
            set {
                _impl[index] = value;
            }
        }

        public void Add(MapPoint item) {
            _impl.Add(item);
        }

        public void Clear() {
            _impl.Clear();
        }

        public bool Contains(MapPoint item) {
            return _impl.Contains(item);
        }

        public void CopyTo(MapPoint[] array, int arrayIndex) {
            _impl.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _impl.Count; }
        }

        public bool IsReadOnly {
            get { return true; }
        }

        public bool Remove(MapPoint item) {
            return _impl.Remove(item);
        }

        public override IEnumerator<MapPoint> GetEnumerator() {
            return _impl.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _impl.GetEnumerator();
        }

    }

    public enum MapPointShape {
        Circle,
        Square,
        Triangle
    }

    public class MapPoint {

        public MapPoint() {
        }

        public MapPoint(double lat, double lon) {
            this.Latitude = lat;
            this.Longitude = lon;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Label { get; set; }

        public int SiteID { get; set; }
        public string SiteName { get; set; }
        public int SiteVisitID { get; set; }
        public string SiteVisitName { get; set; }
        public int MaterialID { get; set; }
        public string MaterialName { get; set; }

    }
}
