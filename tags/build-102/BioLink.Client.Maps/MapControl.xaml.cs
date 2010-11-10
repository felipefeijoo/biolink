using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

namespace BioLink.Client.Maps {

    public enum MapMode {
        Normal,
        RegionSelect
    }

    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl, IDisposable {

        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //private static extern bool DeleteObject(IntPtr hObject);


        private const int RESIZE_TIMEOUT = 0;
       
        private Timer _resizeTimer;
        private Point _distanceAnchor;
        private Point _lastMousePos;
        // private ObservableCollection<LayerViewModel> _layers;
        private IDegreeDistanceConverter _distanceConverter = new DegreesToKilometresConverter();
        private Action<List<RegionDescriptor>> _callback;
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
            this.Mode = mode;
            this._callback = callback;                            
            _resizeTimer = new Timer(new TimerCallback((o)=> {
                this.InvokeIfRequired(() => {
                    mapBox.Refresh();
                    _resizeTimer.Change(Timeout.Infinite, Timeout.Infinite);
                });
            }));

            if (mode == MapMode.Normal) {
                buttonRow.Height = new System.Windows.GridLength(0);
            }

            mapBox.PreviewMode = MapBox.PreviewModes.Fast;
            mapBox.MouseMove += new MapBox.MouseEventHandler((p, e) => {
                _lastMousePos = p;
                string lat = GeoUtils.DecDegToDMS(p.X, CoordinateType.Longitude);
                string lng = GeoUtils.DecDegToDMS(p.Y, CoordinateType.Latitude);
                txtPosition.Text = String.Format("Pos: {0}, {1}", lat, lng);

                if (_distanceAnchor != null && _distanceConverter != null) {
                    var distance = _distanceAnchor.Distance(_lastMousePos);
                    txtDistance.Text = String.Format("Distance from drop anchor: {0}", _distanceConverter.Convert(distance));
                }

            });

            mapBox.MouseUp += new MapBox.MouseEventHandler(map_MouseUp);

            var user = PluginManager.Instance.User;

            List<LayerDescriptor> filenames = Config.GetUser(user, "MapTool." + mode.ToString() + ".Layers", new List<LayerDescriptor>());
            foreach (LayerDescriptor desc in filenames) {
                AddLayer(desc);
            }

            System.Drawing.Color backcolor = Config.GetUser(user, "MapTool." + mode.ToString() + ".MapBackColor", System.Drawing.Color.White);
            mapBox.BackColor = backcolor;

            SerializedEnvelope env = Config.GetUser<SerializedEnvelope>(user, "MapTool." + mode.ToString() + ".LastExtent", null);

            this.Loaded += new System.Windows.RoutedEventHandler((source, e) => {
                if (env != null) {
                    mapBox.Map.ZoomToBox(env.CreateBoundingBox());
                } else {
                    if (mapBox.Map.Layers.Count > 0) {
                        mapBox.Map.ZoomToExtents();
                    }
                }
            });

            Unloaded += new System.Windows.RoutedEventHandler(MapControl_Unloaded);

        }

        void MapControl_Unloaded(object sender, System.Windows.RoutedEventArgs e) {
            var layers = new List<LayerDescriptor>();
            foreach (ILayer layer in mapBox.Map.Layers) {
                if (layer is VectorLayer) {
                    var vl = layer as VectorLayer;
                    if (vl.DataSource is ShapeFile) {
                        var filename = (vl.DataSource as ShapeFile).Filename;
                        var desc = new LayerDescriptor();
                        desc.Filename = filename;
                        desc.HatchStyle = GraphicsUtils.GetHatchStyleFromBrush(vl.Style.Fill);
                        desc.FillColor = GraphicsUtils.GetColorFromBrush(vl.Style.Fill);
                        desc.DrawOutline = vl.Style.EnableOutline;

                        layers.Add(desc);
                    }
                }
            }

            var user = PluginManager.Instance.User;

            Config.SetUser(user, "MapTool." + Mode.ToString() + ".Layers", layers);           
            var env = new SerializedEnvelope(mapBox.Map.Envelope);
            Config.SetUser(user, "MapTool." + Mode.ToString() + ".LastExtent", env);
            Config.SetUser(user, "MapTool." + Mode.ToString() + ".MapBackColor", mapBox.BackColor);
        }

        public MapMode Mode { get; private set; }

        private void BuildMenuItem(System.Windows.Forms.ContextMenu menu, string caption, Action action) {
            var menuItem = menu.MenuItems.Add(caption);
            menuItem.Click += new EventHandler((source, e) => {
                action();
            });            
        }

        void map_MouseUp(Point WorldPos, System.Windows.Forms.MouseEventArgs evt) {
            if (evt.Button == System.Windows.Forms.MouseButtons.Right) {
                mapBox.Focus();
                var menu = new System.Windows.Forms.ContextMenu();

                if (Mode == MapMode.Normal) {
                    if (_distanceAnchor == null) {
                        BuildMenuItem(menu, "Drop distance anchor", () => {
                            DropDistanceAnchor();
                        });
                    } else {
                        BuildMenuItem(menu, "Hide distance anchor", () => {
                            HideDistanceAnchor();
                            mapBox.Refresh();
                        });
                    }
                } else {
                    var pointClick = new SharpMap.Geometries.Point(WorldPos.X, WorldPos.Y);
                    var feature = FindRegionFeature(pointClick);
                    if (feature != null) {
                        string regionPath = feature["BLREGHIER"] as string;
                        if (regionPath != null) {
                            var bits = regionPath.Split('\\');
                            var sb = new StringBuilder();
                            foreach (string bit in bits) {
                                sb.Append("\\").Append(bit);
                                string path = sb.ToString().Substring(1); // trim off leading backslash
                                BuildMenuItem(menu, bit, () => {
                                    SelectRegionByPath(path);
                                });
                            }
                            menu.MenuItems.Add("-");
                        }
                    }
                    BuildMenuItem(menu, "Deselect all", () => {
                        DeselectAllRegions();
                    });

                }

                menu.Show(mapBox, evt.Location);
            } else {

                if (mapBox.ActiveTool == MapBox.Tools.None) {
                    var pointClick = new SharpMap.Geometries.Point(WorldPos.X, WorldPos.Y);

                    if (Mode == MapMode.RegionSelect) {
                        SelectRegion(pointClick);
                    }
                }
            }
        }

        private void DeselectAllRegions() {
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
                            string regionPath = row["BLREGHIER"] as string;
                            var node = _regionModel.FindByPath(regionPath);
                            foreach (RegionDescriptor selectedRegion in selectedRegions) {
                                if (regionPath.StartsWith(selectedRegion.Path)) {
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

        private void SelectRegion(SharpMap.Geometries.Point pointClick) {

            if (_regionModel == null) {
                return;
            }

            var selectedFeature = FindRegionFeature(pointClick);
            if (selectedFeature != null) {
                string regionPath = selectedFeature["BLREGHIER"] as string;
                var node = _regionModel.FindByPath(regionPath);
                Debug.Assert(node != null, "could not find region node for path: " + regionPath);
                node.IsSelected = !node.IsSelected;

                DrawSelectionLayer();
            }
        }

        private void DrawSelectionLayer() {
            // Now remove any exsiting overlay layer...
            RemoveLayerByName("_regionSelectLayer");

            if (_regionModel == null) {
                return;
            }

            var geometries = new Collection<SharpMap.Geometries.Geometry>();
            var selectedFeatures = _regionModel.FindSelectedRegions();
            foreach (RegionTreeNode node in selectedFeatures) {
                if (node.FeatureRow != null) {
                    geometries.Add(node.FeatureRow.Geometry);
                }
            }

            var selectLayer = new SharpMap.Layers.VectorLayer("_regionSelectLayer");
            var provider = new SharpMap.Data.Providers.GeometryProvider(geometries);
            selectLayer.DataSource = provider;
            selectLayer.Style.Fill = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(100, 0, 0, 0));

            selectLayer.Style.Outline = new System.Drawing.Pen(System.Drawing.SystemColors.Highlight, 1);
            selectLayer.Style.EnableOutline = true;

            addLayer(selectLayer, null, false);

            mapBox.Refresh();
        }

        private VectorLayer FindFirstRegionLayer() {
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
            return null;
        }

        public FeatureDataRow FindRegionFeature(SharpMap.Geometries.Point point) {

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

        public SharpMap.Data.FeatureDataRow FindGeoNearPoint(SharpMap.Geometries.Point pos, SharpMap.Layers.VectorLayer layer) {
            BoundingBox bbox = pos.GetBoundingBox();
            if (bbox != null) {
                SharpMap.Data.FeatureDataSet ds = new SharpMap.Data.FeatureDataSet();
                layer.DataSource.Open();
                layer.DataSource.ExecuteIntersectionQuery(bbox, ds);
                DataTable tbl = ds.Tables[0] as SharpMap.Data.FeatureDataTable;
                GisSharpBlog.NetTopologySuite.IO.WKTReader reader = new GisSharpBlog.NetTopologySuite.IO.WKTReader();
                GeoAPI.Geometries.IGeometry point = reader.Read(pos.ToString());
                if (tbl.Rows.Count == 0) {
                    layer.DataSource.Close();
                    return null;
                }

                double distance = point.Distance(reader.Read((tbl.Rows[0] as SharpMap.Data.FeatureDataRow).Geometry.ToString()));
                SharpMap.Data.FeatureDataRow selectedFeature = tbl.Rows[0] as SharpMap.Data.FeatureDataRow;

                if (tbl.Rows.Count > 1) {
                    for (int i = 1; i < tbl.Rows.Count; i++) {
                        GeoAPI.Geometries.IGeometry line = reader.Read((tbl.Rows[i] as SharpMap.Data.FeatureDataRow).Geometry.ToString());
                        if (point.Distance(line) < distance) {
                            distance = point.Distance(line);
                            selectedFeature = tbl.Rows[i] as SharpMap.Data.FeatureDataRow;
                        }
                    }
                }
                layer.DataSource.Close();
                return selectedFeature;
            }
            return null;
        }

        private void RemoveLayerByName(string name) {                        
            ILayer layer = mapBox.Map.GetLayerByName(name);
            if (layer != null) {
                mapBox.Map.Layers.Remove(layer);
            }
        }

        private void HideDistanceAnchor() {
            RemoveLayerByName("_distanceAnchor");
            _distanceAnchor = null;
            txtDistance.Text = "";
        }

        private void DropDistanceAnchor() {
            if (_lastMousePos != null) {
                Point p = new Point(_lastMousePos.X, _lastMousePos.Y);

                HideDistanceAnchor();

                VectorLayer shapeFileLayer = new VectorLayer("_distanceAnchor", new SharpMap.Data.Providers.GeometryProvider(p));
                shapeFileLayer.Style.Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Blue);
                shapeFileLayer.Style.Outline = new System.Drawing.Pen(new System.Drawing.SolidBrush(System.Drawing.Color.Black), 1);
                shapeFileLayer.Style.Enabled = true;
                shapeFileLayer.Style.EnableOutline = true;
                _distanceAnchor = p;
                addLayer(shapeFileLayer, null, false);
            }
        }

        private void AddLayer(LayerDescriptor desc) {
            var layer = AddLayer(desc.Filename);
            if (layer != null) {
                var vl = layer as VectorLayer;
                if (vl != null) {
                    vl.Style.Fill = GraphicsUtils.CreateBrush(desc.FillColor, desc.HatchStyle);
                    vl.Style.EnableOutline = desc.DrawOutline;
                }
            }
        }

        private ILayer AddLayer(string filename) {
            ILayer layer = LayerFileLoader.LoadLayer(filename);
            if (layer != null) {
                addLayer(layer, filename);
            } else {
                throw new Exception("Could not load layer file '" + filename + "'");
            }
            return layer;
        }

        private void addLayer(ILayer layer, String filename, bool zoomToExtent = true) {
            mapBox.Map.Layers.Add(layer);

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

            regions.Sort((a, b) => {
                return a.Path.CompareTo(b.Path);
            });

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
                ds.Open();
                for (uint i = 0; i < ds.GetFeatureCount(); ++i) {
                    var row = ds.GetFeature(i);                    
                    var node = AddNodeFromPath(root, row);
                }
            }
            return root;
        }

        private RegionTreeNode AddNodeFromPath(RegionTreeNode root, FeatureDataRow feature) {
            var path = feature["BLREGHIER"] as string;
            string[] bits = path.Split('\\');
            var parent = root;
            for (int i = 0; i < bits.Length; ++i) {
                string bit = bits[i];            
                var pNode = parent.FindChildByName(bit);
                if (pNode == null) {
                    // intermediate tree nodes do not have a feature row attached to them...
                    parent = parent.AddChild(bit);
                } else {
                    parent = pNode;
                }
            }
            // leaf nodes represent the actual feature.
            parent.FeatureRow = feature;

            return parent;
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

            if (root.IsSelected) {
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
            if (AllChildrenSelected(root)) {
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

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Portable Network Graphics (*.PNG)|*.PNG|JPEG (*.JPG)|*.JPG|Windows bitmap file (*.BMP)|*.BMP|GIF (*.GIF)|*.GIF|TIFF (*.TIF)|*.TIF";

            if (dlg.ShowDialog().ValueOrFalse()) {
                try {

                    FileInfo f = new FileInfo(dlg.FileName);
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
            var frm = new LayersWindow(this);
            frm.Owner = this.FindParentWindow();
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

    }

    //public static class EnvelopeExtensions {

    //    public static IEnvelope Grow(this IEnvelope envelope, double growAmount) {
    //        return GeometryFactory.CreateEnvelope(envelope.MinX - growAmount, envelope.MaxX + growAmount, envelope.MinY - growAmount, envelope.MaxY + growAmount);
    //    }
    //}

    public class LayerDescriptor {

        public string Filename { get; set; }
        public System.Drawing.Drawing2D.HatchStyle? HatchStyle { get; set; }
        public System.Drawing.Color FillColor { get; set; }
        public bool DrawOutline { get; set; }

    }

}
