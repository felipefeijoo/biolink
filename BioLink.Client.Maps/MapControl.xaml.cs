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
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SharpMap.Layers;
using SharpMap.Data.Providers;
using System.IO;
using System.Threading;
using BioLink.Client.Utilities;
using SharpMap.Data;
using SharpMap.Geometries;
using BioLink.Client.Extensibility;
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Win32;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using BioLink.Data;
using System.Reflection;

namespace BioLink.Client.Maps {

    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl, IDisposable {

        private const int RESIZE_TIMEOUT = 0;

        private readonly Timer _resizeTimer;
        private Point _distanceAnchor;
        private string _anchorCaption;
        private Point _lastMousePos;
        private readonly Action<List<RegionDescriptor>> _callback;
        private RegionTreeNode _regionModel;
        private VectorLayer _regionLayer;
        private List<RegionDescriptor> _unmatchedRegions;

        #region Designer constructor
        public MapControl() {
            InitializeComponent();
        }
        #endregion

        public MapControl(MapMode mode, Action<List<RegionDescriptor>> callback = null) {
            InitializeComponent();
            Mode = mode;
            _callback = callback;
            _resizeTimer = new Timer(o => this.InvokeIfRequired(() => {
                                                                      mapBox.Refresh();
                                                                      _resizeTimer.Change(Timeout.Infinite, Timeout.Infinite);
                                                                  }));

            if (mode == MapMode.Normal) {
                buttonRow.Height = new System.Windows.GridLength(0);
            }

            mapBox.PreviewMode = MapBox.PreviewModes.Fast;
            mapBox.MouseMove += (p, e) => {
                                    _lastMousePos = p;
                                    string lat = GeoUtils.DecDegToDMS(p.X, CoordinateType.Longitude);
                                    string lng = GeoUtils.DecDegToDMS(p.Y, CoordinateType.Latitude);
                                    txtPosition.Text = String.Format("{0}, {1}", lat, lng);

                                    if (_distanceAnchor != null) {
                                        string from = "drop anchor";
                                        const DistanceUnits units = DistanceUnits.Kilometers;
                                        if (!string.IsNullOrEmpty(_anchorCaption)) {
                                            from = _anchorCaption;
                                        }
                                        var distance = GeoUtils.GreatCircleArcLength(_distanceAnchor.Y, _distanceAnchor.X, _lastMousePos.Y, _lastMousePos.X, units);
                                        string direction = GeoUtils.GreatCircleArcDirection(_distanceAnchor.Y, _distanceAnchor.X, _lastMousePos.Y, _lastMousePos.X, 32);

                                        txtDistance.Text = String.Format("Distance from {0}: {1:0.00} {2} {3}", from, distance, units, direction);
                                    }

                                };

            btnFindRegion.Visibility = (mode == MapMode.RegionSelect ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);

            InfoGrid.SizeChanged += InfoGrid_SizeChanged;

            mapBox.MouseUp += map_MouseUp;

            var user = PluginManager.Instance.User;

            List<LayerDescriptor> filenames = Config.GetUser(user, "MapTool." + mode.ToString() + ".Layers", new List<LayerDescriptor>());
            foreach (LayerDescriptor desc in filenames) {
                try {
                    AddLayer(desc, false, true);
                } catch (Exception ex) {
                    ErrorMessage.Show("Failed to load layer: " + desc.Filename + " (" + ex.Message + ")");
                }
            }

            System.Drawing.Color backcolor = Config.GetUser(user, "MapTool." + mode.ToString() + ".MapBackColor", System.Drawing.Color.White);
            mapBox.BackColor = backcolor;

            mapBox.Legend.IsVisible = Config.GetUser(user, "MapTool.LegendVisible", false);
            System.Drawing.Rectangle? legendPos = Config.GetUser<System.Drawing.Rectangle?>(user, "MapTool.LegendPosition", null);
            if (legendPos.HasValue) {
                mapBox.Legend.Position = legendPos.Value;
            }

            btnLegend.IsChecked = mapBox.Legend.IsVisible;

            var env = Config.GetUser<SerializedEnvelope>(user, "MapTool." + mode.ToString() + ".LastExtent", null);

            HideInfoPanel();

            Loaded += (source, e) => {
                          if (env != null) {
                              mapBox.Map.ZoomToBox(env.CreateBoundingBox());
                          } else {
                              if (mapBox.Map.Layers.Count > 0) {
                                  mapBox.Map.ZoomToExtents();
                              }
                          }
                      };

            mapBox.AllowDrop = true;

            mapBox.DragOver += mapBox_DragOver;
            mapBox.DragDrop += mapBox_DragDrop;

            btnInfo.Checked += btnInfo_Checked;
            btnInfo.Unchecked += btnInfo_Unchecked;

            Unloaded += MapControl_Unloaded;

        }

        void btnInfo_Unchecked(object sender, System.Windows.RoutedEventArgs e) {
            HideInfoPanel();
        }

        void btnInfo_Checked(object sender, System.Windows.RoutedEventArgs e) {
            ShowInfoPanel();
        }

        private void HideInfoPanel() {
            infoGridSplitter.Visibility = System.Windows.Visibility.Collapsed;
            InfoGrid.Visibility = System.Windows.Visibility.Collapsed;
            mapGrid.ColumnDefinitions[1].Width = new System.Windows.GridLength(0);
            mapGrid.ColumnDefinitions[2].Width = new System.Windows.GridLength(0);
            _resizeTimer.Change(RESIZE_TIMEOUT, Timeout.Infinite);
        }

        private void ShowInfoPanel() {
            infoGridSplitter.Visibility = System.Windows.Visibility.Visible;
            InfoGrid.Visibility = System.Windows.Visibility.Visible;
            mapGrid.ColumnDefinitions[1].Width = new System.Windows.GridLength(6);
            mapGrid.ColumnDefinitions[2].Width = new System.Windows.GridLength(250);
            btnArrow.IsChecked = true;
            _resizeTimer.Change(RESIZE_TIMEOUT, Timeout.Infinite);
        }

        void InfoGrid_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e) {
            _resizeTimer.Change(RESIZE_TIMEOUT, Timeout.Infinite);
        }

        private IMapPointSetGenerator GetPointGenerator(System.Windows.Forms.DragEventArgs e) {
            var pinnable = GetDragData<PinnableObject>(e, PinnableObject.DRAG_FORMAT_NAME);

            if (pinnable != null) {
                return PluginManager.Instance.FindAdaptorForPinnable<IMapPointSetGenerator>(pinnable);
            }
            return null;
        }


        void mapBox_DragDrop(object sender, System.Windows.Forms.DragEventArgs e) {
            var pointGenerator = GetPointGenerator(e);
            if (pointGenerator != null) {
                MapPointSet points = pointGenerator.GeneratePoints(true);
                if (points != null) {
                    PlotPoints(points);
                }
            }
        }

        /// <summary>
        /// This abomination is required because dragging from a WPF control to a WinForms hosted control doesn't
        /// work properly. This method uses reflection to grub around inside the inner data of the IDataObject and
        /// pull out the requested data format.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private T GetDragData<T>(System.Windows.Forms.DragEventArgs e, string format) where T : class {
            FieldInfo info = e.Data.GetType().GetField("innerData", BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null) {
                object obj = info.GetValue(e.Data);
                info = obj.GetType().GetField("innerData", BindingFlags.NonPublic | BindingFlags.Instance);
                if (info != null) {
                    var dataObj = info.GetValue(obj) as System.Windows.DataObject;
                    if (dataObj != null) {
                        return dataObj.GetData(format) as T;
                    }
                }
            }
            return null;
        }

        void mapBox_DragOver(object sender, System.Windows.Forms.DragEventArgs e) {
            var pointGenerator = GetPointGenerator(e);
            e.Effect = System.Windows.Forms.DragDropEffects.None;
            if (pointGenerator != null) {
                e.Effect = System.Windows.Forms.DragDropEffects.Link;
            }
        }

        void MapControl_Unloaded(object sender, System.Windows.RoutedEventArgs e) {
            var layers = new List<LayerDescriptor>();

            foreach (ILayer layer in mapBox.Map.Layers) {
                if (layer is VectorLayer) {
                    var vl = layer as VectorLayer;
                    if (vl.DataSource is ShapeFile) {
                        var filename = (vl.DataSource as ShapeFile).Filename;
                        var desc = new LayerDescriptor {Filename = filename, HatchStyle = GraphicsUtils.GetHatchStyleFromBrush(vl.Style.Fill), FillColor = GraphicsUtils.GetColorFromBrush(vl.Style.Fill), DrawOutline = vl.Style.EnableOutline};
                        layers.Add(desc);
                    }
                }
            }

            var user = PluginManager.Instance.User;

            Config.SetUser(user, "MapTool." + Mode.ToString() + ".Layers", layers);            


            var env = new SerializedEnvelope(mapBox.Map.Envelope);
            Config.SetUser(user, "MapTool." + Mode.ToString() + ".LastExtent", env);
            Config.SetUser(user, "MapTool." + Mode.ToString() + ".MapBackColor", mapBox.BackColor);

            Config.SetUser(user, "MapTool.LegendVisible", mapBox.Legend.IsVisible);
            Config.SetUser(user, "MapTool.LegendPosition", mapBox.Legend.Position);
        }

        public MapMode Mode { get; private set; }

        private void BuildMenuItem(System.Windows.Forms.ContextMenu menu, string caption, Action action) {
            var menuItem = menu.MenuItems.Add(caption);
            menuItem.Click += (source, e) => action();
        }

        private Dictionary<Int32, FeatureDataRow> FindIDs(IEnumerable<FeatureDataRowLayerPair> rows, string columnName) {
            var results = new Dictionary<Int32, FeatureDataRow>();

            foreach (FeatureDataRowLayerPair rowPair in rows) {
                var row = rowPair.FeatureDataRow;
                foreach (DataColumn col in row.Table.Columns) {
                    if (col.ColumnName.Contains(columnName)) {
                        var id = (int)row[col.ColumnName];
                        if (id > 0) {
                            results[id] = row;
                        }
                    }
                }
            }

            return results;
        }

        private void EditObject(Dictionary<Int32, FeatureDataRow> idMap, LookupType objectType) {
            if (idMap.Count == 1) {
                PluginManager.Instance.EditLookupObject(objectType, idMap.First().Key);
            } else {
                var service = new MaterialService(PluginManager.Instance.User);
                List<ViewModelBase> model = null;
                if (objectType == LookupType.Site) {
                    var sites = service.GetRDESites(idMap.Keys.ToArray());
                    model = sites.ConvertAll(s => (ViewModelBase)new RDESiteViewModel(s));

                } else if (objectType == LookupType.SiteVisit) {
                    var siteVisits = service.GetRDESiteVisits(idMap.Keys.ToArray());
                    var siteIds = siteVisits.Aggregate(new List<int>(), (l, v) => {
                        if (!l.Contains(v.SiteID)) {
                            l.Add(v.SiteID);
                        }
                        return l;
                    });
                    var sites = service.GetRDESites(siteIds.ToArray<int>()).ConvertAll(site => new RDESiteViewModel(site));
                    var siteMap = sites.ToDictionary(s => s.SiteID);

                    model = siteVisits.ConvertAll(sv => {
                        var vm = new RDESiteVisitViewModel(sv);
                        if (siteMap.ContainsKey(sv.SiteID)) {
                            vm.Site = siteMap[sv.SiteID];
                        }
                        return (ViewModelBase)vm;
                    });

                } else if (objectType == LookupType.Material) {
                    // First get the material records....
                    var material = service.GetRDEMaterial(idMap.Keys.ToArray());
                    // Get the distinct list of site visit ids...
                    var visitIds = material.Aggregate(new List<int>(), (l, v) => {
                        if (!l.Contains(v.SiteVisitID)) {
                            l.Add(v.SiteVisitID);
                        }
                        return l;
                    });

                    // Get the visit records, and convert into a dictionary of SiteVisitID => SiteVisit for easy lookup
                    var visits = service.GetRDESiteVisits(visitIds.ToArray()).ConvertAll(sv => new RDESiteVisitViewModel(sv));
                    var visitMap = visits.ToDictionary(sv => sv.SiteVisitID);

                    // Then get the unique (distinct) list of site ids...
                    var siteIds = visits.Aggregate(new List<int>(), (l, v) => {
                        if (!l.Contains(v.SiteID)) {
                            l.Add(v.SiteID);
                        }
                        return l;
                    });

                    // and create a map of SiteID => Site record for easy look up...
                    var sites = service.GetRDESites(siteIds.ToArray<int>()).ConvertAll(site => new RDESiteViewModel(site));
                    var siteMap = sites.ToDictionary(s => s.SiteID);

                    // Now we hook it all up - for every material record we attach its site visit record, and its site record
                    model = material.ConvertAll(m => {
                        var vm = new RDEMaterialViewModel(m);
                        vm.SiteVisit = visitMap[vm.SiteVisitID];
                        vm.SiteVisit.Site = siteMap[vm.SiteVisit.SiteID];
                        return (ViewModelBase)vm;
                    });
                }

                if (model != null) {
                    var frm = new SelectedObjectChooser(PluginManager.Instance.User, model, objectType) {Owner = this.FindParentWindow()};
                    frm.Show();
                }
            }
        }

        void map_MouseUp(Point WorldPos, System.Windows.Forms.MouseEventArgs evt) {

            var pointClick = new Point(WorldPos.X, WorldPos.Y);
            if (evt.Button == System.Windows.Forms.MouseButtons.Right) {
                mapBox.Focus();
                var menu = new System.Windows.Forms.ContextMenu();

                if (mapBox.Legend.Position.Contains(evt.Location)) {
                    BuildMenuItem(menu, "Legend options...", () => mapBox.ShowLegendOptions());
                } else {

                    var rows = Drill(pointClick);

                    var siteIDs = FindIDs(rows, "SiteID");
                    if (siteIDs.Count > 0) {

                    }

                    var siteVisitIDs = FindIDs(rows, "SiteVisitID");
                    if (siteVisitIDs.Count > 0) {
                        BuildMenuItem(menu, "Edit Site Visit...", () => EditObject(siteVisitIDs, LookupType.SiteVisit));
                    }

                    var materialIDs = FindIDs(rows, "MaterialID");
                    if (materialIDs.Count > 0) {
                        BuildMenuItem(menu, "Edit Material...", () => EditObject(materialIDs, LookupType.Material));
                    }

                    if (Mode == MapMode.Normal) {
                        if (_distanceAnchor == null) {
                            BuildMenuItem(menu, "Drop distance anchor", DropDistanceAnchor);

                        } else {
                            BuildMenuItem(menu, "Hide distance anchor", () => {
                                HideDistanceAnchor();
                                mapBox.Refresh();
                            });
                        }
                    } else {
                        var feature = FindRegionFeature(pointClick);
                        if (feature != null) {
                            var regionPath = feature["BLREGHIER"] as string;
                            if (regionPath != null) {
                                var bits = regionPath.Split('\\');
                                var sb = new StringBuilder();
                                foreach (string bit in bits) {
                                    sb.Append("\\").Append(bit);
                                    string path = sb.ToString().Substring(1); // trim off leading backslash
                                    BuildMenuItem(menu, bit, () => SelectRegionByPath(path));
                                }
                                menu.MenuItems.Add("-");
                            }
                        }
                        BuildMenuItem(menu, "Deselect all", DeselectAllRegions);

                    }
                }

                menu.Show(mapBox, evt.Location);
            } else {

                if (mapBox.ActiveTool == MapBox.Tools.None || mapBox.ActiveTool == MapBox.Tools.Pan) {
                    if (Mode == MapMode.RegionSelect && mapBox.ActiveTool == MapBox.Tools.None) {
                        SelectRegion(pointClick);
                    } else {
                        if (btnInfo.IsChecked.ValueOrFalse()) {
                            var rows = Drill(pointClick);
                            featureInfo.DisplayFeatures(pointClick, rows);
                        }
                    }
                }

            }
        }

        private void DeselectAllRegions() {
            // Deselect every node in the tree...Simply walk the tree...
            _regionModel.Traverse(node => {
                node.IsSelected = false;
            });

            DrawSelectionLayer();
        }

        public void SelectRegionByPath(string regionPath) {
            var node = _regionModel.FindByPath(regionPath);

            if (node != null) {
                node.IsSelected = true;
                DrawSelectionLayer();
            }
        }

        public void SelectRegions(List<RegionDescriptor> selectedRegions) {
            var layer = FindFirstRegionLayer();

            if (layer != null) {
                if (_regionModel != null) {
                    _regionModel.DeselectAll();
                } else {
                    _regionModel = BuildRegionModel(layer);
                }

                _unmatchedRegions = new List<RegionDescriptor>();


                foreach (RegionDescriptor selectedRegion in selectedRegions) {
                    var node = _regionModel.FindByPath(selectedRegion.Path);
                    if (node == null) {
                        // The selected region cannot be displayed/selected by the selected layer.
                        // we need to remember that we were handed this, however, so that we can pass it
                        // back during the update...
                        _unmatchedRegions.Add(selectedRegion);
                    }
                }

                using (var ds = layer.DataSource) {
                    ds.Open();
                    for (uint i = 0; i < ds.GetFeatureCount(); ++i) {
                        var row = ds.GetFeature(i);
                        if (row.Table.Columns.Contains("BLREGHIER")) {
                            var regionPath = row["BLREGHIER"] as string;
                            var node = _regionModel.FindByPath(regionPath);
                            foreach (RegionDescriptor selectedRegion in selectedRegions) {
                                if (regionPath != null && regionPath.StartsWith(selectedRegion.Path)) {
                                    node.IsSelected = true;
                                }
                            }
                        }
                    }
                }

            } else {
                _unmatchedRegions = new List<RegionDescriptor>(selectedRegions);
            }

            DrawSelectionLayer();
        }

        private void SelectRegion(Point pointClick) {

            if (_regionModel == null) {
                return;
            }

            var selectedFeature = FindRegionFeature(pointClick);
            if (selectedFeature != null) {
                var regionPath = selectedFeature["BLREGHIER"] as string;
                var node = _regionModel.FindByPath(regionPath);
                Debug.Assert(node != null, "could not find region node for path: " + regionPath);
                if (node != null) {
                    node.IsSelected = !node.IsSelected;
                }

                DrawSelectionLayer();
            }
        }

        private void DrawSelectionLayer() {
            // Now remove any exsiting overlay layer...
            RemoveLayerByName("_regionSelectLayer");

            if (_regionModel == null) {
                return;
            }

            var geometries = new Collection<Geometry>();
            var selectedFeatures = _regionModel.FindSelectedRegions();
            foreach (RegionTreeNode node in selectedFeatures) {
                if (node.FeatureRow != null) {
                    geometries.Add(node.FeatureRow.Geometry);
                }
            }

            var selectLayer = new VectorLayer("_regionSelectLayer");
            var provider = new GeometryProvider(geometries);
            selectLayer.DataSource = provider;
            selectLayer.Style.Fill = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(100, 0, 0, 0));

            selectLayer.Style.Outline = new System.Drawing.Pen(System.Drawing.SystemColors.Highlight, 1);
            selectLayer.Style.EnableOutline = true;

            addLayer(selectLayer, false, true);

            mapBox.Refresh();
        }

        private VectorLayer FindFirstRegionLayer() {
            gridRegionWarning.Visibility = System.Windows.Visibility.Collapsed;
            foreach (ILayer layer in mapBox.Map.Layers) {
                if (layer is VectorLayer) {
                    var vl = layer as VectorLayer;
                    var shapefile = vl.DataSource as ShapeFile;
                    if (shapefile != null) {
                        using (shapefile) {
                            shapefile.Open();
                            var row = shapefile.GetFeature(0);
                            if (row.Table.Columns.Contains("BLREGHIER")) {
                                return vl;
                            }
                        }
                    }
                }
            }

            // No region layers...Show a warning...
            gridRegionWarning.Visibility = System.Windows.Visibility.Visible;

            return null;
        }

        public FeatureDataRow FindRegionFeature(Point point) {

            var layer = FindFirstRegionLayer();
            if (layer != null) {
                var shapeFile = layer.DataSource as ShapeFile;
                if (shapeFile != null) {
                    var candidate = FindGeoNearPoint(point, layer);
                    if (candidate != null) {
                        return candidate;
                    }
                }
            }

            return null;
        }

        public List<FeatureDataRowLayerPair> Drill(Point pos) {

            var list = new List<FeatureDataRowLayerPair>();

            // BoundingBox bbox = pos.GetBoundingBox();            
            double delta = mapBox.Map.MapHeight * 0.01;
            var bbox = new BoundingBox(pos.X - delta, pos.Y - delta, pos.X + delta, pos.Y + delta);

            foreach (ILayer l in mapBox.Map.Layers) {
                if (l is VectorLayer && !l.LayerName.StartsWith("_")) {
                    var layer = l as VectorLayer;                    
                    var ds = new FeatureDataSet();
                    layer.DataSource.Open();
                    layer.DataSource.ExecuteIntersectionQuery(bbox, ds);
                    DataTable tbl = ds.Tables[0];
                    //var reader = new GisSharpBlog.NetTopologySuite.IO.WKTReader();
                    //IGeometry geometry = reader.Read(pos.ToString());
                    if (tbl.Rows.Count == 0) {
                        layer.DataSource.Close();
                    } else {
                        for (int i = 0; i < tbl.Rows.Count; ++i) {
                            var data = new FeatureDataRowLayerPair { FeatureDataRow = tbl.Rows[i] as FeatureDataRow, Layer = l };
                            if (data.FeatureDataRow.Geometry is MultiPolygon) {
                                var mp = data.FeatureDataRow.Geometry as MultiPolygon;
                                foreach (Polygon p in mp) {
                                    if (PointInsidePolygon(pos, p)) {
                                        list.Add(data);
                                    }
                                }
                            } else {
                                list.Add(data);
                            }

                        }
                    }
                }                
            }

            return list;
        }

        public static bool PointInsidePolygon(Point p, Polygon polygon) {

            var points = polygon.ExteriorRing.Vertices;
            int counter = 0;
            // double xinters;  

            var p1 = points[0];
            // for each line segment in the polygon (p[0]-p[1], p[1]-p[2], etc)...
            for (int i = 1; i <= points.Count; i++) {
                var p2 = points[i % points.Count];
                if (p.Y > Math.Min(p1.Y, p2.Y)) {
                    if (p.Y <= Math.Max(p1.Y, p2.Y)) {
                        if (p.X <= Math.Max(p1.X, p2.X)) {
                            if (p1.Y != p2.Y) {
                                // the x coord when the ray cast from the point intersects with the current line segment
                                double xinters = (p.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
                                if (p1.X == p2.X || p.X <= xinters)
                                    counter++;
                            }
                        }
                    }
                }
                p1 = p2;
            }

            return counter % 2 != 0;
        }

        public FeatureDataRow FindGeoNearPoint(Point pos, VectorLayer layer) {
            BoundingBox bbox = pos.GetBoundingBox();
            if (bbox != null) {
                var ds = new FeatureDataSet();
                layer.DataSource.Open();
                layer.DataSource.ExecuteIntersectionQuery(bbox, ds);
                DataTable tbl = ds.Tables[0];
                var reader = new GisSharpBlog.NetTopologySuite.IO.WKTReader();
                var point = reader.Read(pos.ToString());
                if (tbl.Rows.Count == 0) {
                    layer.DataSource.Close();
                    return null;
                }

                var featureDataRow = tbl.Rows[0] as FeatureDataRow;
                if (featureDataRow != null) {
                    double distance = point.Distance(reader.Read(featureDataRow.Geometry.ToString()));
                    var selectedFeature = featureDataRow;

                    if (tbl.Rows.Count > 1) {
                        for (int i = 1; i < tbl.Rows.Count; i++) {
                            var dataRow = tbl.Rows[i] as FeatureDataRow;
                            if (dataRow != null) {
                                var line = reader.Read(dataRow.Geometry.ToString());
                                if (point.Distance(line) < distance) {
                                    distance = point.Distance(line);
                                    selectedFeature = dataRow;
                                }
                            }
                        }
                    }
                    layer.DataSource.Close();
                    return selectedFeature;
                }
            }
            return null;
        }

        private void RemoveLayerByName(string name) {
            ILayer layer = mapBox.Map.GetLayerByName(name);
            if (layer != null) {
                mapBox.Map.Layers.Remove(layer);
                if (layer is IDisposable) {
                    (layer as IDisposable).Dispose();
                }
            }
            // Now check if there is a layer label to remove...
            if (!name.EndsWith(" Labels")) {
                RemoveLayerByName(string.Format("{0} Labels", name));
            }
        }

        public void HideDistanceAnchor(bool refresh = false) {
            RemoveLayerByName("_distanceAnchor");
            _distanceAnchor = null;
            txtDistance.Text = "";
            if (refresh) {
                mapBox.Refresh();
            }
        }

        public void DropDistanceAnchor(double longitude, double latitude, string caption) {
            var p = new Point(longitude, latitude);
            DropDistanceAnchor(p, caption);
        }

        private void DropDistanceAnchor() {
            if (_lastMousePos != null) {
                var p = new Point(_lastMousePos.X, _lastMousePos.Y);
                DropDistanceAnchor(p);
            }
        }

        private void DropDistanceAnchor(Point p, string caption = null) {

            HideDistanceAnchor();
            _anchorCaption = caption;
            var shapeFileLayer = new VectorLayer("_distanceAnchor", new GeometryProvider(p)) {Style = {Symbol = MapSymbolGenerator.Triangle(10, System.Drawing.Color.Blue, true, System.Drawing.Color.Black)}};
            _distanceAnchor = p;
            addLayer(shapeFileLayer, false, true);
        }

        public void PlotPoints(MapPointSet points) {

            var table = new FeatureDataTable();
            table.Columns.Add(new DataColumn("Label", typeof(string)));
            table.Columns.Add(new DataColumn("SiteID", typeof(int)));
            table.Columns.Add(new DataColumn("SiteVisitID", typeof(int)));
            table.Columns.Add(new DataColumn("MaterialID", typeof(int)));

            foreach (MapPoint mp in points) {
                var row = table.NewRow();
                row.Geometry = new Point(mp.Longitude, mp.Latitude);
                row["Label"] = mp.Label;
                row["SiteID"] = mp.SiteID;
                row["SiteVisitID"] = mp.SiteVisitID;
                row["MaterialID"] = mp.MaterialID;

                table.AddRow(row);
            }

            string labelLayerName = string.Format("{0} Labels", points.Name);

            RemoveLayerByName(points.Name);

            var shapeFileLayer = new VectorLayer(points.Name, new GeometryFeatureProvider(table)) {SmoothingMode = SmoothingMode.AntiAlias, Style = {Symbol = MapSymbolGenerator.GetSymbolForPointSet(points), PointSize = points.Size}};

            addLayer(shapeFileLayer, false, true);

            if (points.DrawLabels) {
                var labelLayer = new LabelLayer(labelLayerName) {DataSource = shapeFileLayer.DataSource, LabelColumn = "Label", LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection, Style = new SharpMap.Styles.LabelStyle {CollisionBuffer = new System.Drawing.SizeF(2f, 2f), CollisionDetection = true, Font = new System.Drawing.Font("Tahoma", 12), ForeColor = System.Drawing.Color.Black, Offset = new System.Drawing.PointF(25, 20), HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center}};
                addLayer(labelLayer, false, true);
            }
        }

        public void ClearPoints(bool refresh = false) {

            RemoveLayerByName("_pointLayer");

            if (refresh) {
                mapBox.Refresh();
            }
        }

        private void AddLayer(LayerDescriptor desc, bool zoomToExtents, bool addToTop) {
            var layer = AddLayer(desc.Filename, zoomToExtents, addToTop);
            if (layer != null) {
                var vl = layer as VectorLayer;
                if (vl != null) {
                    vl.Style.Fill = GraphicsUtils.CreateBrush(desc.FillColor, desc.HatchStyle);
                    vl.Style.EnableOutline = desc.DrawOutline;
                }
            }
        }

        private ILayer AddLayer(string filename, bool zoomToExtents, bool addToTop) {
            ILayer layer = LayerFileLoader.LoadLayer(filename);
            if (layer != null) {
                addLayer(layer, zoomToExtents, addToTop);
            } else {
                throw new Exception("Could not load layer file '" + filename + "'");
            }
            return layer;
        }

        private void addLayer(ILayer layer, bool zoomToExtent, bool addToTop) {

            if (addToTop) {
                mapBox.Map.Layers.Add(layer);
            } else {
                mapBox.Map.Layers.Insert(0, layer);
            }

            if (zoomToExtent) {
                mapBox.Map.ZoomToExtents();
            }

            mapBox.Refresh();

            if (Mode == MapMode.RegionSelect) {
                var topRegionLayer = FindFirstRegionLayer();
                if (_regionLayer != topRegionLayer) {
                    _regionLayer = topRegionLayer;
                    _regionModel = BuildRegionModel(topRegionLayer);
                }
            }

        }

        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e) {
            _resizeTimer.Change(RESIZE_TIMEOUT, Timeout.Infinite);
        }

        private void map_Click(object sender, EventArgs ea) {
        }

        private void btnZoomToWindow_Checked(object sender, System.Windows.RoutedEventArgs e) {
            mapBox.ActiveTool = MapBox.Tools.ZoomWindow;
        }

        private void btnPan_Checked(object sender, System.Windows.RoutedEventArgs e) {
            mapBox.ActiveTool = MapBox.Tools.Pan;
        }

        private void btnZoomToExtent_Click(object sender, System.Windows.RoutedEventArgs e) {
            mapBox.Map.ZoomToExtents();
            mapBox.Refresh();
        }

        private void btnArrow_Checked(object sender, System.Windows.RoutedEventArgs e) {
            mapBox.ActiveTool = MapBox.Tools.None;
        }

        public void Dispose() {
        }

        private void btnUpdate_Click(object sender, System.Windows.RoutedEventArgs e) {
            UpdateSelectedRegions();
        }

        private void UpdateSelectedRegions() {
            var regions = OptimizeSelectedRegions();
            // Now we add back the regions that the mapControl could not display (i.e. whose paths could not be resolved in the region layer)...
            regions.AddRange(_unmatchedRegions);
            // and notify the caller

            regions.Sort((a, b) => a.Path.CompareTo(b.Path));

            if (_callback != null) {
                _callback(regions);
            }
        }

        /// <summary>
        /// Builds up a tree structure based from each feature in the specified layer
        /// . It is assumed that the datasource for the layer contains a column called "BLREGHEIR", which
        /// contains a '\' delimited region path. This path is used to construct intermediate nodes (that have
        /// no geometry feature attached to them) and a leaf node, which represents the smallest selectable region
        /// for the mapControl.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        private RegionTreeNode BuildRegionModel(VectorLayer layer) {
            if (layer == null) {
                return null;
            }

            var root = new RegionTreeNode(null, "");
            using (var ds = layer.DataSource as ShapeFile) {
                if (ds != null) {
                    ds.Open();
                    for (uint i = 0; i < ds.GetFeatureCount(); ++i) {
                        var row = ds.GetFeature(i);
                        AddNodeFromPath(root, row);
                    }
                }
            }
            return root;
        }

        protected RegionTreeNode AddNodeFromPath(RegionTreeNode root, FeatureDataRow feature) {
            var path = feature["BLREGHIER"] as string;
            if (path != null) {
                var bits = path.Split('\\');
                var parent = root;
                foreach (var bit in bits) {
                    var pNode = parent.FindChildByName(bit);
                    parent = pNode ?? parent.AddChild(bit);
                }
                // leaf nodes represent the actual feature.
                parent.FeatureRow = feature;

                return parent;
            }
            return null;
        }

        /// <summary>
        /// Attempts to reduce the collection of selected regions to the minimum number required
        /// to describe the selection. It does this by first checking, depth first, if each child
        /// collection is all selected. if so, the selection value can be raised to the parent
        /// 
        /// Once all the selection properties have been normalized, the top most selected items are 
        /// going to be the minimum set of nodes required, and are converted into region descriptors
        /// for return
        /// </summary>
        /// <returns></returns>
        private List<RegionDescriptor> OptimizeSelectedRegions() {
            NormalizeSelection(_regionModel);
            var list = new List<RegionDescriptor>();
            FindTopMostSelectedRegions(_regionModel, list);
            return list;
        }

        /// <summary>
        /// Recursively traverses the region model looking for selected nodes. If a specified node is encountered
        /// it is added to the list, and the recursion goes no further down that path (hence, top most selected nodes are collected).
        /// If the specified node is not selected, each of its children are tested in the same way.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="list"></param>
        private void FindTopMostSelectedRegions(RegionTreeNode root, List<RegionDescriptor> list) {

            if (root.IsSelected && root.Parent != null) {
                // Create a new region descriptor for this top-most selected item...
                list.Add(new RegionDescriptor(root.Path, root.IsThroughoutRegion));
                return;
            }

            // Not selected? Check the children
            foreach (RegionTreeNode child in root.Children) {
                FindTopMostSelectedRegions(child, list);
            }
        }

        /// <summary>
        /// Returns true if each child of the specified node is selected
        /// </summary>
        /// <param name="treeNode"></param>
        /// <returns></returns>
        private bool AllChildrenSelected(RegionTreeNode treeNode) {
            foreach (RegionTreeNode node in treeNode.Children) {
                if (!node.IsSelected) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Depth first traversal of every tree node is performed (recursive)
        /// 
        /// If every child of the supplied node is selected, the node itself becomes selected
        /// (and the ThroughoutRegion property is also set).
        /// 
        /// </summary>
        /// <param name="root"></param>
        private void NormalizeSelection(RegionTreeNode root) {

            // No children, we've hit the bottom, so start returning back up
            if (root.Children.Count == 0) {
                return;
            }

            // For each child, make sure their selection model is normalized before checking if the current children
            // are all selected
            foreach (RegionTreeNode child in root.Children) {
                NormalizeSelection(child);
            }

            // By now, all my childrens selected properties should be normalized, so its quite
            // simple to determine if the current node's selecton property can replace the composite values of my children
            if (AllChildrenSelected(root) && root.Children.Count > 1) {
                if (!root.IsSelected) {
                    root.IsThroughoutRegion = true;
                }
                root.IsSelected = true;
            }

        }

        private void btnSaveImage_Click(object sender, System.Windows.RoutedEventArgs e) {
            SaveAsImage();
        }

        private void SaveAsImage() {

            var dlg = new SaveFileDialog {Filter = "Portable Network Graphics (*.PNG)|*.PNG|JPEG (*.JPG)|*.JPG|Windows bitmap file (*.BMP)|*.BMP|GIF (*.GIF)|*.GIF|TIFF (*.TIF)|*.TIF"};

            if (dlg.ShowDialog().ValueOrFalse()) {
                try {

                    var f = new FileInfo(dlg.FileName);
                    ImageFormat format = null;
                    switch (f.Extension.ToLower()) {
                        case ".png":
                            format = ImageFormat.Png;
                            break;
                        case ".jpg":
                            format = ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            format = ImageFormat.Bmp;
                            break;
                        case ".gif":
                            format = ImageFormat.Gif;
                            break;
                        case ".tif":
                        case ".tiff":
                            format = ImageFormat.Tiff;
                            break;
                    }
                    if (format == null) {
                        mapBox.Image.Save(dlg.FileName);
                    } else {
                        mapBox.Image.Save(dlg.FileName, format);
                    }

                } catch (Exception ex) {
                    ErrorMessage.Show("Failed to save mapControl image to file {0}. {1}", dlg.FileName, ex.Message);
                }
            }
        }

        private void CopyToClipboard() {
            BitmapSource bmp = GraphicsUtils.SystemDrawingImageToBitmapSource(mapBox.Image);
            System.Windows.Clipboard.SetImage(bmp);
        }

        private void btnCopyImage_Click(object sender, System.Windows.RoutedEventArgs e) {
            CopyToClipboard();
        }

        private void btnLayers_Click(object sender, System.Windows.RoutedEventArgs e) {
            ShowLayersControl();
        }

        private void ShowLayersControl() {
            var frm = new LayersWindow(this) {Owner = this.FindParentWindow()};
            if (frm.ShowDialog().ValueOrFalse()) {
                if (Mode == MapMode.RegionSelect) {
                    var topRegionLayer = FindFirstRegionLayer();
                    if (_regionLayer != topRegionLayer) {
                        _regionLayer = topRegionLayer;
                        _regionModel = BuildRegionModel(topRegionLayer);
                    }
                }
            }
        }


        public void AddRasterLayer(string filename) {
            RemoveRasterLayer(filename);
            AddLayer(filename, false, false);
        }

        public void RemoveRasterLayer(string filename) {
            string layername = Path.GetFileNameWithoutExtension(filename);
            RemoveLayerByName(layername);
        }

        private void btnFindRegion_Click(object sender, System.Windows.RoutedEventArgs e) {
            FindRegionByName();
        }

        private void FindRegionByName() {
            var layer = FindFirstRegionLayer();
            if (layer != null) {
                var frm = new FindRegionsWindow(layer, SelectRegionByPath) {Owner = this.FindParentWindow()};

                frm.ShowDialog();
            }
        }

        private void btnPointFeatures_Click(object sender, System.Windows.RoutedEventArgs e) {
            RunPointFeaturesReport();
        }

        private void RunPointFeaturesReport() {

            var pointLayers = new List<VectorLayer>();
            var featureLayers = new List<VectorLayer>();

            foreach (ILayer layer in mapBox.Map.Layers) {
                if (layer is VectorLayer) {
                    if (IsPointLayer(layer)) {
                        pointLayers.Add(layer as VectorLayer);
                    } else {
                        featureLayers.Add(layer as VectorLayer);
                    }
                }
            }

            if (pointLayers.Count == 0) {
                ErrorMessage.Show("No point layers found!");
                return;
            }

            if (featureLayers.Count == 0) {
                ErrorMessage.Show("No polygon (feature) layers found!");
            }

            PluginManager.Instance.RunReport(null, new PointsFeaturesReport(PluginManager.Instance.User, pointLayers, featureLayers));
        }



        public bool IsPointLayer(ILayer layer) {
            if (layer != null && layer is VectorLayer) {
                var vm = new VectorLayerViewModel(layer as VectorLayer);                
                return vm.Symbol != null;
            }
            return false;
        }

        private void btnLegend_Checked(object sender, System.Windows.RoutedEventArgs e) {
            mapBox.Legend.IsVisible = true;
            mapBox.Refresh();
        }

        private void btnLegend_Unchecked(object sender, System.Windows.RoutedEventArgs e) {
            mapBox.Legend.IsVisible = false;
            mapBox.Refresh();
        }

    }

    public enum MapMode {
        Normal,
        RegionSelect
    }

    public class LayerDescriptor {

        public string Filename { get; set; }
        public HatchStyle? HatchStyle { get; set; }
        public System.Drawing.Color FillColor { get; set; }
        public bool DrawOutline { get; set; }

    }

    public class FeatureDataRowLayerPair {

        public FeatureDataRow FeatureDataRow { get; set; }
        public ILayer Layer { get; set; }

    }

}
