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

            IsCancelled = false;

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

                if (!IsCancelled) {
                    if (ProgressObserver != null) {
                        ProgressObserver.ProgressStart("Running model", false);
                    }
                    RunModelImpl(target, layers, points);
                    if (ProgressObserver != null) {
                        if (IsCancelled) {
                            ProgressObserver.ProgressEnd("Model cancelled!");
                        } else {
                            ProgressObserver.ProgressEnd("Model complete");
                        }
                    }
                }

            }

            return IsCancelled ? null : target;
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

        public void CancelModel() {
            IsCancelled = true;
        }

        public bool IsCancelled { get; protected set; }

        public virtual int? PresetIntervals { 
            get { return null; } 
        }

        public virtual double? PresetCutOff {
            get { return null; }
        }

    }

}
