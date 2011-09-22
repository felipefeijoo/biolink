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
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;

namespace BioLink.Client.Extensibility {

    internal class TypographicFeatureListItem : TextBlock, IComparable {

        private readonly string _displayName;
        private readonly DependencyProperty _chooserProperty;

        public TypographicFeatureListItem(string displayName, DependencyProperty chooserProperty) {
            _displayName = displayName;
            _chooserProperty = chooserProperty;
            this.Text = displayName;
        }

        public DependencyProperty ChooserProperty {
            get { return _chooserProperty; }
        }

        public override string ToString() {
            return _displayName;
        }

        int IComparable.CompareTo(object obj) {
            return string.Compare(_displayName, obj.ToString(), true, CultureInfo.CurrentCulture);
        }
    }
}
