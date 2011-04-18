using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;


namespace BioLink.Client.Tools {

    public abstract class DistributionModel : IBioLinkExtension {

        public DistributionModel() {
        }

        public GridLayer RunModel(IEnumerable<GridLayer> layers, IEnumerable<MapPointSet> pointSets) {
            GridLayer target = null;
            var list = new List<GridLayer>();
            if (layers.Count() > 0) {
                ProgressMessage("Initializing Model...");
                // The first layer sets the size and resolution of the resulting grid. Subsequent grid layers of different dimensions must be interpolated to fit this size...
                var first = layers.ElementAt(0);
                target = new GridLayer(first.Width, first.Height) { DeltaLatitude = first.DeltaLatitude, DeltaLongitude = first.DeltaLongitude, Flags = first.Flags, Latitude0 = first.Latitude0, Longitude0 = first.Longitude0, NoValueMarker = first.NoValueMarker };
                list.Add(first);
                for (int i = 1; i < layers.Count(); ++i) {
                    var layer = layers.ElementAt(i);
                    if (!first.MatchesResolution(layer)) {
                        ProgressMessage("Resizing layer {0}...", layer.Name);
                        var newlayer = new GridLayer(first.Width, first.Height) { DeltaLatitude = first.DeltaLatitude, DeltaLongitude = first.DeltaLongitude, Flags = first.Flags, Latitude0 = first.Latitude0, Longitude0 = first.Longitude0, NoValueMarker = first.NoValueMarker };
                        for (int y = 0; y < first.Height; ++y) {
                            double lat = first.Latitude0 + (y * first.DeltaLatitude);		// Work out Lat. of this cell.
                            for (int x = 0; x < first.Width; ++x) {
                                double lon = first.Longitude0 + (x * first.DeltaLongitude); // Work out Long. of this cell.
                                newlayer.SetCellValue(x,y, layer.GetValueAt(lat, lon, first.NoValueMarker));
                            }
                        }
                        list.Add(newlayer);

                    } else {
                        list.Add(layer);
                    }
                }

                // now get the points ready...
                var points = new ModelPointSet();
                ProgressMessage("Preparing points...");
                foreach (MapPointSet set in pointSets) {
                    foreach (MapPoint p in set) {

                        if (p.Latitude == 0 && p.Longitude == 0) {
                            continue;
                        }

	                    double fudge = (double) ( first.DeltaLatitude / 2.0 );

	                    var x = Math.Abs( (int) ((p.Longitude - ( first.Longitude0 - fudge) ) / first.DeltaLongitude));
	                    var y = Math.Abs( (int) ((p.Latitude - ( first.Latitude0 - fudge) ) / first.DeltaLatitude));

                        if (!points.ContainsCell(x, y)) {
                            points.AddPoint(new ModelPoint(layers.Count()) { X = p.Longitude, Y = p.Latitude, CellX = x, CellY = y });
                        }
                        
                    }
                }

                if (ProgressObserver != null) {
                    ProgressObserver.ProgressStart("Running model", false);
                }

                RunModelImpl(target, layers, points);

                if (ProgressObserver != null) {
                    ProgressObserver.ProgressEnd("Model complete");
                }

            }

            return target;
        }

        protected void ProgressMessage(string format, params object[] args) {
            if (ProgressObserver != null) {
                ProgressObserver.ProgressMessage(string.Format(format, args));
            }
        }

        protected abstract void RunModelImpl(GridLayer targetLayer, IEnumerable<GridLayer> layers, ModelPointSet points);

        public void Dispose() { }

        public abstract string Name { get; }

        public IProgressObserver ProgressObserver { get; set; }
    }

    public class GowerMetricDistributionModel : DistributionModel {

        protected override void RunModelImpl(GridLayer targetLayer, IEnumerable<GridLayer> layers, ModelPointSet points) {

            int layerCount = layers.Count();

            double noValue = targetLayer.NoValueMarker;

            // Preprocess points...
            layers.ForEachIndex((layer, i) => {
                foreach (ModelPoint p in points.Points) {                    
                    var value = layer.GetValueAt(p.Y, p.X, noValue);
                    p.SetValueForLayer(i, value);
                    p.UsePoint = value != noValue;
                }
            });

            double[] range = new double[layers.Count()];
            // Find ranges;
            layers.ForEachIndex((layer, layerIndex) => {
                bool rangeSet = false;
                double min = 0;
                double max = 0;

                points.Points.ForEach((p) => {
                    if (p.UsePoint) {
                        var value = p.GetValueForLayer(layerIndex);
                        if (!rangeSet) {
                            min = value;
                            max = value;
                            rangeSet = true;
                        } else {
                            if (value < min) {
                                min = value;
                            } else if (value > max) {
                                max = value;
                            }
                        }
                    }
                });

                var tmp = Math.Abs(max - min);
                range[layerIndex] = tmp == 0 ? 1 : tmp;                
            });

            int height = targetLayer.Height;
            int width = targetLayer.Width;

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    var fMinDist = noValue;
                    var minSet = false;

                    foreach (ModelPoint p in points.Points) {
                        double fdist = 0;

                        if (p.UsePoint) {
                            for (int layerIndex = 0; layerIndex < layerCount; layerIndex++) {

                                var layer = layers.ElementAt(layerIndex);

                                var fVal = p.GetValueForLayer(layerIndex);
                                var fCellVal = layer.GetCellValue(x, y);
                                if (fCellVal == noValue) {                                    
                                    fMinDist = noValue;                                    
                                    goto SetPoint;                                    
                                }
                                fdist = fdist + (Math.Abs(fCellVal - fVal) / range[layerIndex]);
                            };

                            fdist = fdist / layers.Count();
                            if (!minSet) {
                                fMinDist = fdist;
                                minSet = true;
                            } else {
                                if (fdist < fMinDist) {
                                    fMinDist = fdist;
                                }
                            }
                        }
                    }

                    fMinDist = (1.0 - fMinDist) * 100.0;
                SetPoint:
                    targetLayer.SetCellValue(x, y, fMinDist);
                }

                if (y % 20 == 0) {
                    var percent = ((double)y / (double)height) * 100;
                    if (ProgressObserver != null) {
                        ProgressObserver.ProgressMessage("Running Model...", percent);
                    }
                }
            }
        }

        public override string Name {
            get { return "Gower Metric (DOMAIN)"; }
        }

    }

    public class BoxCarDistributionModel : DistributionModel {

        protected override void RunModelImpl(GridLayer targetLayer, IEnumerable<GridLayer> layers, ModelPointSet points) {
            throw new NotImplementedException();
        }

        public override string Name {
            get { return "Boxcar (BIOCLIM)"; }
        }

    }


    public class ModelPoint {

        private double[] _layerValues;

        public ModelPoint(int layerCount) {
            UsePoint = true;
            _layerValues = new double[layerCount];
        }

        public double X { get; set; }
        public double Y { get; set; }
        public int CellX { get; set; }
        public int CellY { get; set; }
        public bool UsePoint { get; set; }

        public void SetValueForLayer(int index, double value) {
            _layerValues[index] = value;
        }

        public double GetValueForLayer(int index) {
                return _layerValues[index];
        }
    }

    public class ModelPointSet {

        private List<ModelPoint> _points = new List<ModelPoint>();

        public void AddPoint(ModelPoint p) {
            _points.Add(p);            
        }

        public IEnumerable<ModelPoint> Points {
            get { return _points;  }
        }

        public bool ContainsCell(int x, int y) {
            return _points.FirstOrDefault((p) => { return (p.CellX == x) && (p.CellY == y); }) != null;
        }

    }


}
