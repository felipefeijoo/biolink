using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
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
using GeoAPI.Geometries;
using SharpMap.Converters.Geometries;
using BioLink.Client.Extensibility;
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Win32;

namespace BioLink.Client.Maps {

    public enum MapMode {
        Normal,
        RegionSelect
    }

    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl, IDisposable {

        private const int RESIZE_TIMEOUT = 0;

        private Dictionary<string, ILayerFactory> _layerFactoryCatalog = new Dictionary<string, ILayerFactory>();
        private Timer _resizeTimer;
        private ICoordinate _distanceAnchor;
        private ICoordinate _lastMousePos;
        private ObservableCollection<LayerViewModel> _layers;
        private IDegreeDistanceConverter _distanceConverter = new DegreesToKilometresConverter();
        private Action<List<string>> _callback;
        private RegionTreeNode _regionModel;
        private VectorLayer _regionLayer;

        #region Designer constructor
        public MapControl() {
            InitializeComponent();
        }
        #endregion

        public MapControl(MapMode mode, Action<List<string>> callback = null) {
            InitializeComponent();
            this.Mode = mode;
            this._callback = callback;                
            RegisterLayerFactories();
            _resizeTimer = new Timer(new TimerCallback((o)=> {
                this.InvokeIfRequired(() => {
                    map.Refresh();
                    _resizeTimer.Change(Timeout.Infinite, Timeout.Infinite);
                });
            }));

            if (mode == MapMode.Normal) {
                buttonRow.Height = new GridLength(0);
            }

            _layers = new ObservableCollection<LayerViewModel>();

            lstLayers.ItemsSource = _layers;

            map.PreviewMode = MapBox.PreviewModes.Fast;

            map.MouseMove += new MapBox.MouseEventHandler((p, e) => {
                _lastMousePos = p;
                string lat = GeoUtils.DecDegToDMS(p.X, CoordinateType.Longitude);
                string lng = GeoUtils.DecDegToDMS(p.Y, CoordinateType.Latitude);
                txtPosition.Text = String.Format("Pos: {0}, {1}", lat, lng);

                if (_distanceAnchor != null && _distanceConverter != null) {
                    var distance = _distanceAnchor.Distance(_lastMousePos);
                    txtDistance.Text = String.Format("Distance from drop anchor: {0}", _distanceConverter.Convert(distance));
                }

            });

            map.MouseUp += new MapBox.MouseEventHandler(map_MouseUp);

            var user = PluginManager.Instance.User;

            List<string> filenames = Config.GetUser(user, "MapTool." + mode.ToString() + ".Layers", new List<string>());
            foreach (string filename in filenames) {
                AddLayer(filename);
            }

            SerializedEnvelope env = Config.GetUser<SerializedEnvelope>(user, "MapTool." + mode.ToString() + ".LastExtent", null);

            this.Loaded += new RoutedEventHandler((source, e) => {
                if (env != null) {
                    map.Map.ZoomToBox(env.CreateEnvelope());
                } else {
                    if (map.Map.Layers.Count > 0) {
                        map.Map.ZoomToExtents();
                    }
                }
            });

        }

        public MapMode Mode { get; private set; }

        private void BuildMenuItem(System.Windows.Forms.ContextMenu menu, string caption, Action action) {
            var menuItem = menu.MenuItems.Add(caption);
            menuItem.Click += new EventHandler((source, e) => {
                action();
            });            
        }

        void map_MouseUp(ICoordinate WorldPos, System.Windows.Forms.MouseEventArgs evt) {
            if (evt.Button == System.Windows.Forms.MouseButtons.Right) {
                map.Focus();
                var menu = new System.Windows.Forms.ContextMenu();

                if (Mode == MapMode.Normal) {
                    if (_distanceAnchor == null) {
                        BuildMenuItem(menu, "Drop distance anchor", () => {
                            DropDistanceAnchor();
                        });
                    } else {
                        BuildMenuItem(menu, "Hide distance anchor", () => {
                            HideDistanceAnchor();
                            map.Refresh();
                        });
                    }
                } else {
                    var pointClick = GeometryFactory.CreatePoint(WorldPos);
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

                menu.Show(map, evt.Location);
            } else {

                if (map.ActiveTool == MapBox.Tools.None) {
                    var pointClick = GeometryFactory.CreatePoint(WorldPos);

                    if (Mode == MapMode.RegionSelect) {
                        SelectRegion(pointClick);
                    }
                }
            }
        }

        private void DeselectAllRegions() {
        }
        
        public void SelectRegionByPath(string regionPath) {
            var layer = FindFirstRegionLayer();            
            if (layer != null) {
                using (var ds = layer.DataSource) {
                    ds.Open();
                    for (uint i = 0; i < ds.GetFeatureCount(); ++i) {
                        var row = ds.GetFeature(i);
                        if (row.Table.Columns.Contains("BLREGHIER")) {
                            string candidatePath = row["BLREGHIER"] as string;
                            if (candidatePath.StartsWith(regionPath)) {
                                var candidateNode = _regionModel.FindByPath(candidatePath);
                                candidateNode.IsSelected = true;
                            }                            
                        }
                    }                    
                }
                DrawSelectionLayer();
            }
        }

        internal void SelectRegions(List<string> selectedRegions) {
            var layer = FindFirstRegionLayer();
            if (layer != null) {
                if (_regionModel != null) {
                    _regionModel.DeselectAll();
                } else {
                    _regionModel = BuildRegionModel(layer);
                }

                using (var ds = layer.DataSource) {
                    ds.Open();
                    for (uint i = 0; i < ds.GetFeatureCount(); ++i) {
                        var row = ds.GetFeature(i);
                        if (row.Table.Columns.Contains("BLREGHIER")) {
                            string regionPath = row["BLREGHIER"] as string;
                            var node = _regionModel.FindByPath(regionPath);
                            foreach (string selectedPath in selectedRegions) {
                                if (regionPath.StartsWith(selectedPath)) {
                                    node.IsSelected = true;
                                }
                            }
                        }
                    }
                }
            }

            DrawSelectionLayer();
        }

        private void SelectRegion(IPoint pointClick) {
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

            var geometries = new Collection<IGeometry>();
            var selectedFeatures = _regionModel.FindSelectedRegions();
            foreach (RegionTreeNode node in selectedFeatures) {
                geometries.Add(node.FeatureRow.Geometry);
            }

            var selectLayer = new SharpMap.Layers.VectorLayer("_regionSelectLayer");
            var provider = new SharpMap.Data.Providers.GeometryProvider(geometries);
            selectLayer.DataSource = provider;
            selectLayer.Style.Fill = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(100, 0, 0, 0));

            selectLayer.Style.Outline = new System.Drawing.Pen(System.Drawing.SystemColors.Highlight, 1);
            selectLayer.Style.EnableOutline = true;

            addLayer(selectLayer, null, false);

            map.Refresh();
        }

        private VectorLayer FindFirstRegionLayer() {
            foreach (ILayer layer in map.Map.Layers) {
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

        public FeatureDataRow FindRegionFeature(IPoint point) {

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

        public SharpMap.Data.FeatureDataRow FindGeoNearPoint(IPoint pos, SharpMap.Layers.VectorLayer layer) {
            IEnvelope bbox = pos.EnvelopeInternal;
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

        private void RegisterLayerFactories() {
            //			ConfigurationManager.GetSection("LayerFactories");
            _layerFactoryCatalog[".shp"] = new ShapeFileLayerFactory();
        }

        private void RemoveLayerByName(string name) {            
            // First find the layer...
            ILayer layer = map.Map.GetLayerByName(name);

            if (layer != null) {
                // Then find the view model for the layer...
                LayerViewModel viewModel = _layers.First((vm) => { return vm.Model == layer; });
                if (viewModel != null) {
                    _layers.Remove(viewModel);
                }
                map.Map.Layers.Remove(layer);
            }

        }

        private void HideDistanceAnchor() {
            RemoveLayerByName("_distanceAnchor");
            _distanceAnchor = null;
            txtDistance.Text = "";
        }

        private void DropDistanceAnchor() {
            if (_lastMousePos != null) {
                IPoint p = GeometryFactory.CreatePoint(_lastMousePos.X, _lastMousePos.Y);

                HideDistanceAnchor();

                VectorLayer shapeFileLayer = new VectorLayer("_distanceAnchor", new SharpMap.Data.Providers.GeometryProvider(p));
                shapeFileLayer.Style.Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Blue);
                shapeFileLayer.Style.Outline = new System.Drawing.Pen(new System.Drawing.SolidBrush(System.Drawing.Color.Black), 1);
                shapeFileLayer.Style.Enabled = true;
                shapeFileLayer.Style.EnableOutline = true;
                _distanceAnchor = p.Coordinate;
                addLayer(shapeFileLayer, null, false);
            }
        }

        private void AddLayer(string filename) {
            string extension = System.IO.Path.GetExtension(filename);
            ILayerFactory layerFactory = null;

            if (!_layerFactoryCatalog.TryGetValue(extension, out layerFactory)) {
                return;
            }

            ILayer layer = layerFactory.Create(System.IO.Path.GetFileNameWithoutExtension(filename), filename);

            addLayer(layer, filename);
        }

        private void addLayer(ILayer layer, String filename, bool zoomToExtent = true) {
            map.Map.Layers.Add(layer);

            if (zoomToExtent) {
                map.Map.ZoomToExtents();
            }

            map.Refresh();
            _layers.Add(new LayerViewModel(layer, filename));

            var topRegionLayer = FindFirstRegionLayer();
            if (Mode == MapMode.RegionSelect && _regionLayer != topRegionLayer) {
                _regionLayer = topRegionLayer;
                _regionModel = BuildRegionModel(topRegionLayer);
            }
        }

        private void btnAddLayer_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Shape Files (*.shp)|*.shp|All files (*.*)|*.*";
            if (dlg.ShowDialog().ValueOrFalse()) {
                AddLayer(dlg.FileName);
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) {
            _resizeTimer.Change(RESIZE_TIMEOUT, Timeout.Infinite);
        }

        private void map_Click(object sender, EventArgs ea) {            
        }

        private void btnZoomToWindow_Checked(object sender, RoutedEventArgs e) {
            map.ActiveTool = MapBox.Tools.ZoomWindow;
        }

        private void btnPan_Checked(object sender, RoutedEventArgs e) {
            map.ActiveTool = MapBox.Tools.Pan;
        }

        private void btnZoomToExtent_Click(object sender, RoutedEventArgs e) {
            map.Map.ZoomToExtents();
            map.Refresh();
        }

        private void btnArrow_Checked(object sender, RoutedEventArgs e) {
            map.ActiveTool = MapBox.Tools.None;
        }

        public void Dispose() {
            var filename = new List<string>();
            foreach (LayerViewModel layer in _layers) {
                if (layer.Filename != null) {
                    filename.Add(layer.Filename);
                }
            }

            var user = PluginManager.Instance.User;

            Config.SetUser(user, "MapTool." + Mode.ToString() + ".Layers", filename);
            var env = new SerializedEnvelope(map.Map.Envelope);
            Config.SetUser(user, "MapTool." + Mode.ToString() + ".LastExtent", env);
        }

        private void lstLayers_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            ContextMenu menu = new ContextMenu();
            var builder = new MenuItemBuilder();

            var layer = lstLayers.SelectedItem as LayerViewModel;

            if (layer != null) {
                int index = map.Map.Layers.IndexOf(layer.Model);

                menu.Items.Add(builder.New("Move up").Handler(() => { MoveUp(layer); }).Enabled(index < map.Map.Layers.Count - 1).MenuItem);
                menu.Items.Add(builder.New("Move down").Handler(() => { MoveDown(layer); }).Enabled(index > 0).MenuItem);

                if (index > 0) {

                }

                menu.Items.Add(builder.New("Remove").Handler(() => { RemoveLayer(layer); }).MenuItem);
            }

            lstLayers.ContextMenu = menu;
        }

        private void MoveUp(LayerViewModel layer) {
            int index = map.Map.Layers.IndexOf(layer.Model);
            bool bAdd = index >= map.Map.Layers.Count;
            map.Map.Layers.Remove(layer.Model);
            if (bAdd) {
                map.Map.Layers.Add(layer.Model);
            } else {
                map.Map.Layers.Insert(index + 1, layer.Model);
            }
            
            _layers.Remove(layer);
            _layers.Insert(index+1, layer);

            map.Refresh();
        }

        private void MoveDown(LayerViewModel layer) {
            int index = map.Map.Layers.IndexOf(layer.Model);
            map.Map.Layers.Remove(layer.Model);
            map.Map.Layers.Insert(index - 1, layer.Model);

            _layers.Remove(layer);
            _layers.Insert(index - 1, layer);

            map.Refresh();
        }

        private void RemoveLayer(LayerViewModel layer) {            
            if (layer != null) {
                map.Map.Layers.Remove(layer.Model);
                _layers.Remove(layer);
            } 
            map.Refresh();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e) {
            UpdateSelectedRegions();
        }

        private void UpdateSelectedRegions() {
            var regions = OptimizeSelectedRegions();
            if (_callback != null) {
                _callback(regions);
            }
        }

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

        private List<string> OptimizeSelectedRegions() {

            var layer = FindFirstRegionLayer();
            if (layer == null) {
                return null;
            }

            // First get a list of all the selected region paths...
            var list = _regionModel.FindSelectedRegions();

            // For each selected element we basically want to workout if it (and all of its siblings) can be replaced by its parent
            // This can only happen if all of its siblings are selected as well...
            List<String> finalList = new List<string>();
            while (list.Count > 0) {
                // process the first one in the list, until there are no more...
                var node = list[0];
                if (node.Parent != null) {
                    // Work out if there is an ancestor node whose children are all selected. If not the current node is returned (no common ancestor)
                    node = DetermineCommonSelectedAncestor(node);
                    // add this node to the final 'optimized' list of regions...
                    finalList.Add(node.Path);
                    // and remove all descendent regions from the source list...
                    node.Traverse((n) => {
                        list.Remove(n);
                    });

                } else {
                    // add this region path to the optimized list (no optimization possible)
                    finalList.Add(node.Path);
                    // and remove it from the source list...
                    list.Remove(node);
                }
            }

            return finalList;
        }

        /// <summary>
        /// Attempts to find the highest (oldest?) ancestor for which each of its descedants are selected...
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private RegionTreeNode DetermineCommonSelectedAncestor(RegionTreeNode node) {

            if (node.Parent == null) {
                return node;
            }
            
            var ancestor = node;
            while (ancestor.Parent != null && AllSiblingsSelected(ancestor)) {
                ancestor = ancestor.Parent;
            }
            return ancestor;
        }

        /// <summary>
        /// Takes a node, and checks all of its siblings to see if their paths exist in the selectedPaths list
        /// </summary>
        /// <param name="node"></param>
        /// <param name="selectedPaths"></param>
        /// <returns></returns>
        private bool AllSiblingsSelected(RegionTreeNode node) {

            var siblings = node.Parent == null ? null : node.Parent.Children;
            if (siblings != null) {
                foreach (RegionTreeNode sibling in siblings) {                    
                    if (!sibling.IsSelected) {
                        return false;
                    }
                }                
            }
            return true;
        }

    }

    class RegionTreeNode {

        public RegionTreeNode(RegionTreeNode parent, string name) {
            this.Parent = parent;
            this.Name = name;            
            this.Children = new List<RegionTreeNode>();
            Path = CalculatePath();
        }

        public RegionTreeNode Parent { get; set; }
        public string Name { get; set; }
        public List<RegionTreeNode> Children { get; private set; }
        public string Path { get; private set; }
        public bool IsSelected { get; set; }
        internal FeatureDataRow FeatureRow { get; set; }

        internal string CalculatePath() {
            if (Parent == null) {
                return Name;
            } else {
                var parentPath = Parent.CalculatePath();
                if (parentPath.Length > 0) {
                    return parentPath + "\\" + Name;
                } else {
                    return Name;
                }
            }
        }

        public RegionTreeNode FindChildByName(string regionName) {
            var match = Children.Find((n) => { return n.Name.Equals(regionName); });
            return match;
        }

        internal RegionTreeNode AddChild(string regionName) {
            var newNode = new RegionTreeNode(this, regionName);
            Children.Add(newNode);
            return newNode;
        }

        public override string ToString() {
            return Name;
        }

        public RegionTreeNode FindByPath(string path) {
            string[] bits = path.Split('\\');
            var parent = this;
            foreach (string bit in bits) {
                parent = parent.FindChildByName(bit);
                if (parent == null) {
                    break;
                }
            }
            return parent;
        }

        public void Traverse(Action<RegionTreeNode> action) {
            if (action != null) {
                action(this);
            }
            foreach (RegionTreeNode child in Children) {
                child.Traverse(action);
            }
        }

        public List<RegionTreeNode> FindSelectedRegions() {
            var result = new List<RegionTreeNode>();
            Traverse((node) => {
                if (node.IsSelected) {
                    result.Add(node);
                }
            });
            return result;
        }

        public void DeselectAll() {
            this.Traverse((node) => {
                node.IsSelected = false;
            });
        }
    }

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
    

    public class LayerViewModel : GenericViewModelBase<ILayer> {

        private string _filename;

        public LayerViewModel(ILayer layer, string filename)
            : base(layer) {
            _filename = filename;
        }

        public string LayerName {
            get { return Model.LayerName; }
            set { SetProperty(() => Model.LayerName, value); }
        }

        public string Filename {
            get { return _filename; }
            set { SetProperty("Filename", ref _filename, value); }
        }

    }

    public interface ILayerFactory {
        ILayer Create(string layerName, string connectionInfo);
    }

    public class ShapeFileLayerFactory : ILayerFactory {

        public ILayer Create(string layerName, string connectionInfo) {
            ShapeFile shapeFileData = new ShapeFile(connectionInfo);
            VectorLayer shapeFileLayer = new VectorLayer(layerName, shapeFileData);
            shapeFileLayer.Style.Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Wheat);
            shapeFileLayer.Style.Outline = new System.Drawing.Pen(new System.Drawing.SolidBrush(System.Drawing.Color.Black), 1);
            shapeFileLayer.Style.Enabled = true;
            shapeFileLayer.Style.EnableOutline = true;
            
            return shapeFileLayer;
        }

    }

    public interface IDegreeDistanceConverter {
        string Convert(double degree);
    }

    public class DegreesToKilometresConverter : IDegreeDistanceConverter {

        private const double KmsPerMinute = 1.852;

        public string Convert(double degrees) {
            double minutes = degrees * 60.0;
            return String.Format("{0:0.##} km", minutes * KmsPerMinute);
        }
    }

    public static class EnvelopeExtensions {

        public static IEnvelope Grow(this IEnvelope envelope, double growAmount) {
            return GeometryFactory.CreateEnvelope(envelope.MinX - growAmount, envelope.MaxX + growAmount, envelope.MinY - growAmount, envelope.MaxY + growAmount);
        }
    }

}
