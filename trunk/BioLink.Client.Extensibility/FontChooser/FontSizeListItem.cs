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
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;
using System.Globalization;

namespace BioLink.Client.Extensibility {

    internal class FontSizeListItem : TextBlock, IComparable {

        private double _sizeInPoints;

        public FontSizeListItem(double sizeInPoints) {
            _sizeInPoints = sizeInPoints;
            this.Text = sizeInPoints.ToString();
        }

        public override string ToString() {
            return _sizeInPoints.ToString();
        }

        public double SizeInPoints {
            get { return _sizeInPoints; }
        }

        public double SizeInPixels {
            get { return PointsToPixels(_sizeInPoints); }
        }

        public static bool FuzzyEqual(double a, double b) {
            return Math.Abs(a - b) < 0.01;
        }

        int IComparable.CompareTo(object obj) {
            double value;

            if (obj is double) {
                value = (double)obj;
            } else {
                if (!double.TryParse(obj.ToString(), out value)) {
                    return 1;
                }
            }

            return FuzzyEqual(_sizeInPoints, value) ? 0 : (_sizeInPoints < value) ? -1 : 1;
        }

        public static double PointsToPixels(double value) {
            return value * (96.0 / 72.0);
        }

        public static double PixelsToPoints(double value) {
            return value * (72.0 / 96.0);
        }
    }
}
