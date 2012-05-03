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

namespace BioLink.Client.Tools {

    public class BoxCarDistributionModel : DistributionModel {

        protected override void RunModelImpl(GridLayer targetLayer, IEnumerable<GridLayer> layers, ModelPointSet points) {
            double fVal, fDestNoVal;
            bool bTestFailed;

            // Work out ranges for the varying percentile bands across all layers...
            if (layers.Count() <= 0) {
                return;
            }

            List<EnvironmentalLayerRange[]> Range = new List<EnvironmentalLayerRange[]>();
            foreach (GridLayer layer in layers) {
                var array = new EnvironmentalLayerRange[4];
                array[0] = layer.GetRangeForPoints(points.Points, 0.0);
                array[1] = layer.GetRangeForPoints(points.Points, 0.05);
                array[2] = layer.GetRangeForPoints(points.Points, 0.1);
                array[3] = layer.GetRangeForPoints(points.Points, 0.25);
                Range.Add(array);
            }

            // For each cell for each layer...
            var firstLayer = layers.ElementAt(0);
            fDestNoVal = firstLayer.NoValueMarker;

            targetLayer.SetAllCells(fDestNoVal);

            for (int y = 0; y < firstLayer.Height; ++y) {
                for (int x = 0; x < firstLayer.Width; ++x) {
                    // First test %0 - %100
                    bTestFailed = false;
                    for (int i = 0; i < layers.Count(); ++i) {
                        var layer = layers.ElementAt(i);
                        fVal = layer.GetCellValue(x, y);
                        if (fVal == layer.NoValueMarker) {
                            bTestFailed = true;
                            break;
                        } else {
                            if ((fVal < Range[i][0].Min) || (fVal > Range[i][0].Max)) { // %0-%100					
                                bTestFailed = true;
                                targetLayer.SetCellValue(x, y, fDestNoVal);
                                break;
                            }
                        }
                    }

                    if (!bTestFailed) {

                        targetLayer.SetCellValue(x, y, 1);
                        // Second test %5 - %95
                        for (int i = 0; i < layers.Count(); ++i) {
                            var layer = layers.ElementAt(i);
                            fVal = layer.GetCellValue(x, y);
                            if ((fVal < Range[i][1].Min) || (fVal > Range[i][1].Max)) { // %5-%95					
                                bTestFailed = true;
                                break;
                            }
                        }

                        if (!bTestFailed) {
                            targetLayer.SetCellValue(x, y, 2);
                            for (int i = 0; i < layers.Count(); ++i) {
                                var layer = layers.ElementAt(i);
                                fVal = layer.GetCellValue(x, y);
                                if ((fVal < Range[i][2].Min) || (fVal > Range[i][2].Max)) { // %10-%90						
                                    bTestFailed = true;
                                    break;
                                }
                            }

                            if (!bTestFailed) {
                                targetLayer.SetCellValue(x, y, 3);
                                for (int i = 0; i < layers.Count(); ++i) {
                                    var layer = layers.ElementAt(i);
                                    fVal = layer.GetCellValue(x, y);
                                    if ((fVal < Range[i][3].Min) || (fVal > Range[i][3].Max)) { // %25-%75							
                                        bTestFailed = true;
                                        break;
                                    }
                                }

                                if (!bTestFailed) {
                                    targetLayer.SetCellValue(x, y, 4);
                                }
                            }
                        }
                    }
                }

                if ((y % 20) == 0) {
                    int percent = (int)(((float)y / (float)targetLayer.Height) * (float)100);
                    if (ProgressObserver != null) {
                        ProgressObserver.ProgressMessage("Running BOXCAR Model...", percent);
                    }

                    if (IsCancelled) {
                        return;
                    }
                }
            }

            if (ProgressObserver != null) {
                ProgressObserver.ProgressMessage("Running BOXCAR Model...", 100);
            }


        }

        public override double? PresetCutOff {
            get {
                return 0;
            }
        }

        public override int? PresetIntervals {
            get {
                return 4;
            }
        }

        public override string Name {
            get { return "Boxcar (BIOCLIM)"; }
        }

    }

}
