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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for NumberUpDown.xaml
    /// </summary>
    public partial class NumberUpDown : UserControl {

        public NumberUpDown() {
            InitializeComponent();
            Delta = 1;
            AllowNegative = false;
            txt.TextChanged += new TextChangedEventHandler(txt_TextChanged);
        }

        void txt_TextChanged(object sender, TextChangedEventArgs e) {
            int val;
            if (Int32.TryParse(txt.Text, out val)) {
                Number = val;
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e) {
            NumberUp();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e) {
            NumberDown();
        }

        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register("Number", typeof(int), typeof(NumberUpDown), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnNumberChanged)));

        public int Number {
            get { return (int)GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value); }
        }

        private static void OnNumberChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (NumberUpDown)obj;
            control.txt.Text = "" + control.Number;
        }

        private void NumberUp() {
            var temp = Number + Delta;
            if (Maximum.HasValue && temp > Maximum.Value) {
                temp = Maximum.Value;
            }
            Number = temp;
        }

        private void NumberDown() {
            var temp = Number - Delta;
            if (temp < 0 && !AllowNegative) {
                temp = 0;
            }
            if (Minimum.HasValue && temp < Minimum.Value) {
                temp = Minimum.Value;
            }
            Number = temp;
        }

        public int Delta { get; set; }

        public bool AllowNegative { get; set; }

        public bool HasValue {
            get {
                if (string.IsNullOrEmpty(txt.Text)) {
                    return false;
                }

                int val;
                if (!Int32.TryParse(txt.Text, out val)) {
                    return false;
                }

                return true;
            }
        }

        public int? Minimum { get; set; }
        public int? Maximum { get; set; }

    }
}
