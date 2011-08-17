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
    /// Interaction logic for EllipsisTextBox.xaml
    /// </summary>
    public partial class EllipsisTextBox : UserControl {
        public EllipsisTextBox() {
            InitializeComponent();
        }

        private void btn_Click(object sender, RoutedEventArgs e) {

            if (this.Click != null) {
                Click(this, e);
            }

        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(EllipsisTextBox), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnTextChanged)));

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (EllipsisTextBox)obj;
            control.txt.Text = args.NewValue as String;
            control.FireValueChanged(control.txt.Text);
        }

        public String Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        protected void FireValueChanged(string text) {
            if (this.TextChanged != null) {
                TextChanged(this, text);
            }
        }

        public static readonly DependencyProperty SelectedTextProperty = DependencyProperty.Register("SelectedText", typeof(string), typeof(EllipsisTextBox), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectedTextChanged)));

        private static void OnSelectedTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (EllipsisTextBox)obj;
            control.txt.SelectedText = args.NewValue as String;
        }

        public String SelectedText {
            get { return (string)GetValue(SelectedTextProperty); }
            set { SetValue(SelectedTextProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(EllipsisTextBox), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (EllipsisTextBox)obj;
            if (control != null) {
                control.btn.IsEnabled = !(bool)args.NewValue;
                control.txt.IsReadOnly = (bool)args.NewValue;
            }
        }

        public event RoutedEventHandler Click;

        public event TextChangedHandler TextChanged;

        private void txt_TextChanged(object sender, TextChangedEventArgs e) {
            this.Text = txt.Text;
        }
    }
}
