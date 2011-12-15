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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.Windows;

namespace BioLink.Client.Tools {

    public class GowerMetricDistributionModel : DistributionModel {

        protected override void RunModelImpl(GridLayer targetLayer, IEnumerable<GridLayer> layers, ModelPointSet points) {

            int layerCount = layers.Count();

            double noValue = targetLayer.NoValueMarker;

            var pointsArray = points.Points.ToArray();
            var layerArray = layers.ToArray();

            // Preprocess points...
            layers.ForEachIndex((layer, i) => {
                foreach (ModelPoint p in pointsArray) {
                    var value = layer.GetValueAt(p.Y, p.X, noValue);
                    p.LayerValues[i] = value;                    
                    p.UsePoint = value != noValue;
                }
            });

            double[] range = new double[layers.Count()];
            // Find ranges;
            for (int layerIndex = 0; layerIndex < layerArray.Length; ++layerIndex) {

                bool rangeSet = false;
                double min = 0;
                double max = 0;

                for (int i = 0; i < pointsArray.Length; ++i) {
                    var p = pointsArray[i];

                    if (p.UsePoint) {
                        var value = p.LayerValues[layerIndex];
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
                }

                var tmp = Math.Abs(max - min);
                range[layerIndex] = tmp == 0 ? 1 : tmp;
            }

            int height = targetLayer.Height;
            int width = targetLayer.Width;


            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    var fMinDist = noValue;
                    var minSet = false;

                    for (int i = 0; i < pointsArray.Length; ++i) {
                        double fdist = 0;
                        var p = pointsArray[i];

                        if (p.UsePoint) {
                            for (int layerIndex = 0; layerIndex < layerCount; layerIndex++) {

                                var fCellVal = layerArray[layerIndex].Data[x, y];
                                if (fCellVal == noValue) {
                                    fMinDist = noValue;
                                    goto SetPoint;
                                }
                                fdist = fdist + (Math.Abs(fCellVal - p.LayerValues[layerIndex]) / range[layerIndex]);
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
                    targetLayer.Data[x, y] = fMinDist;
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
