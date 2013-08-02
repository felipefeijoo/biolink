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
using System.Text.RegularExpressions;
using System.IO;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public interface IMapPointSetGenerator {

        MapPointSet GeneratePoints(bool showOptions);

    }

    public class DelegatingPointSetGenerator<T> : IMapPointSetGenerator where T : class {

        private Func<bool, T, MapPointSet> _delegate;
        private T _arg;

        public DelegatingPointSetGenerator(Func<bool, T, MapPointSet> @delegate, T arg) {
            _delegate = @delegate;
            _arg = arg;
        }

        public MapPointSet GeneratePoints(bool showOptions) {
            if (_delegate != null) {
                return _delegate(showOptions, _arg);
            }
            return null;
        }
    }

    public class XYFileMapPointSetGenerator : IMapPointSetGenerator {

        public XYFileMapPointSetGenerator(String filename) {
            this.Filename = filename;
        }

        public MapPointSet GeneratePoints(bool showOptions) {

            FileInfo f = new FileInfo(Filename);

            if (!f.Exists) {
                return null;
            }

            StreamReader reader = new StreamReader(f.FullName);
            string line;
            var set = new ListMapPointSet(Path.GetFileNameWithoutExtension(f.FullName));
            int lineNumber = 1;
            while ((line = reader.ReadLine()) != null) {
                double? lon = null;
                double? lat = null;
                String label = "";
                // Try splitting the line up by ',' and seeing if the components are in any of the Degrees Minutes Seconds format...
                var bits = line.Split(',','\t');
                if (bits.Length > 1) {
                    lon = GeoUtils.ParseCoordinate(bits[0].Trim());
                    lat = GeoUtils.ParseCoordinate(bits[1].Trim());
                    if (bits.Length > 2) {
                        label = bits[2].Trim();
                    }
                }

                // were we able to extract values?
                if (lon.HasValue && lat.HasValue) {
                    var point = new MapPoint { Longitude = lon.Value, Latitude = lat.Value, Label = label };
                    set.Add(point);
                } else {
                    if (line.Trim().Length > 0) {
                        String message = String.Format("On line {0}\n\n{1}", lineNumber, line);
                        InfoBox.Show(message, "Problem with coordinate pair!", PluginManager.Instance.ParentWindow);
                    }
                }
                lineNumber++;
            }
            return set;
        }

        public String Filename { get; private set; }

    }
}
