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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for NoteProperties.xaml
    /// </summary>
    public partial class NoteProperties : Window {

        #region Designer Constructor
        public NoteProperties() {
            InitializeComponent();
        }
        #endregion

        public NoteProperties(User user, NoteViewModel model) {
            InitializeComponent();
            this.User = user;
            this.SourceModel = model;

            txtReference.BindUser(user, LookupType.Reference);

            // Make a copy of the model so that changes are only applied when the ok button is clicked.
            var copy = new Note();
            ReflectionUtils.CopyProperties(model.Model, copy);
            Model = new NoteViewModel(copy);
            this.DataContext = Model;
        }

        #region Properties

        public User User { get; private set; }

        public NoteViewModel Model { get; private set; }

        protected NoteViewModel SourceModel { get; set; }

        #endregion

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Hide();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            // Copy over any changes to the source model...
            ReflectionUtils.CopyProperties(Model, SourceModel);

            this.DialogResult = true;
            this.Hide();
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(NoteProperties), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (NoteProperties)obj;
            if (control != null) {
                var readOnly = (bool)args.NewValue;
                control.txtAuthor.IsReadOnly = readOnly;
                control.txtComment.IsReadOnly = readOnly;
                control.txtPage.IsReadOnly = readOnly;
                control.txtReference.IsReadOnly = readOnly;
                control.chkUseInReports.IsEnabled = !readOnly;
            }
        }

    }
}
