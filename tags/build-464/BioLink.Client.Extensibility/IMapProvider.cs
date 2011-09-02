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
using System.Windows.Media;
using BioLink.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public interface IMapProvider {

        void Show();

        void DropAnchor(double longitude, double latitude, string caption);
        void HideAnchor();

        void PlotPoints(MapPointSet points);
        void ClearPoints();
        void AddRasterLayer(string filename);
    }

    public abstract class MapPointSet : IEnumerable<MapPoint>, INotifyCollectionChanged {

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

        public abstract void RemoveAt(int index);
        public abstract void Append(MapPoint point);

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if (this.CollectionChanged != null) {
                CollectionChanged(this, e);
            }
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
                _latIndex = FindIndex(_set.LatitudeColumn);
                _lonIndex = FindIndex(_set.LongitudeColumn);
                _siteIndex = FindIndex(_set.SiteIDColumn);
                _siteVisitIndex = FindIndex(_set.SiteVisitIDColumn);
                _materialIndex = FindIndex(_set.MaterialIDColumn);
            }

            public MapPoint Current {
                get {
                    int realIndex = _currentRow;

                    if (_activeRows != null) {
                        realIndex = _activeRows[_currentRow];
                    }

                    MatrixRow row = _set._data.Rows[realIndex];
                    var mp = new MapPoint(GeoUtils.ParseCoordinate(row[_latIndex]).GetValueOrDefault(0), GeoUtils.ParseCoordinate(row[_lonIndex]).GetValueOrDefault(0));

                    if (_siteIndex >= 0 && row[_siteIndex] != null) {
                        mp.SiteID = (int)row[_siteIndex];
                    }
                    if (_siteVisitIndex >= 0 && row[_siteVisitIndex] != null) {
                        mp.SiteVisitID = (int)row[_siteVisitIndex];
                    }
                    if (_materialIndex >= 0 && row[_materialIndex] != null) {
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

            private int FindIndex(string colName) {
                for (int i = 0; i < _set._data.Columns.Count; ++i) {
                    var col = _set._data.Columns[i];
                    if (col.Name.Contains(colName)) {
                        return i;
                    }
                }
                return -1;
            }

        }

        public override void RemoveAt(int index) {
            _data.RemoveRow(index);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private int FindIndex(string colName) {
            for (int i = 0; i < _data.Columns.Count; ++i) {
                var col = _data.Columns[i];
                if (col.Name.Contains(colName)) {
                    return i;
                }
            }
            return -1;
        }

        public override void Append(MapPoint point) {

            int latIndex = FindIndex(LatitudeColumn);
            int lonIndex = FindIndex(LongitudeColumn);
            int siteIndex = FindIndex(SiteIDColumn);
            int siteVisitIndex = FindIndex(SiteVisitIDColumn);
            int materialIndex = FindIndex(MaterialIDColumn);

            var row = _data.AddRow();

            row[latIndex] = point.Latitude;
            row[lonIndex] = point.Longitude;
            row[siteIndex] = point.SiteID;
            row[siteVisitIndex] = point.SiteVisitID;
            row[materialIndex] = point.MaterialID;

            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public class ListMapPointSet : MapPointSet, IList<MapPoint> {

        private ObservableCollection<MapPoint> _impl = new ObservableCollection<MapPoint>();

        public ListMapPointSet(string name)
            : base(name) {
            _impl.CollectionChanged += new NotifyCollectionChangedEventHandler(_impl_CollectionChanged);
        }

        void _impl_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            RaiseCollectionChanged(e);
        }

        public int IndexOf(MapPoint item) {
            return _impl.IndexOf(item);
        }

        public void Insert(int index, MapPoint item) {
            _impl.Insert(index, item);
        }

        public override void RemoveAt(int index) {
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

        public override void Append(MapPoint point) {
            _impl.Add(point);
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
