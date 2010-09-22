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

namespace BioLink.Client.Maps {
    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl {

        private Dictionary<string, ILayerFactory> _layerFactoryCatalog = new Dictionary<string, ILayerFactory>();
        private Timer _resizeTimer;

        private const int RESIZE_TIMEOUT = 100;

        public MapControl() {
            InitializeComponent();
            RegisterLayerFactories();
            _resizeTimer = new Timer(new TimerCallback((o)=> {
                this.InvokeIfRequired(() => {
                    map.Refresh();
                    _resizeTimer.Change(Timeout.Infinite, Timeout.Infinite);
                });
            }));

            map.PreviewMode = MapBox.PreviewModes.Fast;

            map.MouseMove += new MapBox.MouseEventHandler((p, e) => {
                string lat = GeoUtils.DecDegToDMS(p.X, CoordinateType.Longitude);
                string lng = GeoUtils.DecDegToDMS(p.Y, CoordinateType.Latitude);
                txtPosition.Text = String.Format("Pos: {0}, {1}", lat, lng);
            });

            // map.WheelZoomMagnitude = 1;
        }

        private void RegisterLayerFactories() {
            //			ConfigurationManager.GetSection("LayerFactories");
            _layerFactoryCatalog[".shp"] = new ShapeFileLayerFactory();
        }


        private void AddLayer() {
            string fileName = @"C:\Users\baird\Dev\OldBiolinkSrc\Data Files\Map Layers\Australia\oz.shp";

            string extension = System.IO.Path.GetExtension(fileName);
            ILayerFactory layerFactory = null;

            if (!_layerFactoryCatalog.TryGetValue(extension, out layerFactory)) {
                return;
            }

            ILayer layer = layerFactory.Create(System.IO.Path.GetFileNameWithoutExtension(fileName), fileName);

            addLayer(layer);

            IPoint p = GeometryFactory.CreatePoint(0, 0);
            VectorLayer shapeFileLayer = new VectorLayer("TempLayer", new SharpMap.Data.Providers.GeometryProvider(p));

            shapeFileLayer.Style.Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Wheat);
            shapeFileLayer.Style.Outline = new System.Drawing.Pen(new System.Drawing.SolidBrush(System.Drawing.Color.Black), 1);
            shapeFileLayer.Style.Enabled = true;
            shapeFileLayer.Style.EnableOutline = true;

            addLayer(shapeFileLayer);
        }

        private void addLayer(ILayer layer) {
            map.Map.Layers.Add(layer);

            map.Map.ZoomToExtents();
            map.Refresh();
            // LayersDataGridView.Rows.Insert(0, true, getLayerTypeIcon(layer.GetType()), layer.LayerName);
        }


        private void btnAddLayer_Click(object sender, RoutedEventArgs e) {
            AddLayer();
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

}
