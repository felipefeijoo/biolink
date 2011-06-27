using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;

namespace BioLink.Client.Tools {

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

                    if (IsCancelled) {
                        return;
                    }
                }
            }
        }

        public override string Name {
            get { return "Gower Metric (DOMAIN)"; }
        }

    }
}
