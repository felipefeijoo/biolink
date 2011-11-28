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
using System.IO;
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for TraitElementControl.xaml
    /// </summary>
    public partial class TraitElementControl : UserControl {

        #region designer constructor
        public TraitElementControl() {
            InitializeComponent();
        }
        #endregion

        public TraitElementControl(User user, TraitViewModel model, TraitCategoryType categoryType) {
            this.Model = model;
            DataContext = model;
            InitializeComponent();
            txtValue.BindUser(user, PickListType.DistinctTraitList, model.Name, categoryType);
            if (!String.IsNullOrEmpty(model.Comment)) {
                commentLink.Inlines.Clear();
                commentLink.Inlines.Add(new Run("Edit comment"));
            }

            Model.DataChanged += new DataChangedHandler((vm) => {
                FireTraitChanged();
            });
            
        }

        protected void FireTraitChanged() {
            if (TraitChanged != null) {
                TraitChanged(this, Model);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            if (TraitDeleted != null) {
                TraitDeleted(this, this.Model);
            }
        }

        private void commentLink_Click(object sender, RoutedEventArgs e) {
            EditComment();
        }

        private void EditComment() {
            string oldComment = RTFUtils.StripMarkup(Model.Comment);
            InputBox.Show(this.FindParentWindow(), "Enter comment", "Enter a comment", oldComment, (newcomment) => {
                if (oldComment != newcomment) {
                    Model.Comment = newcomment;
                }
            });
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(TraitElementControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsReadOnlyChanged));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {

            var control = obj as TraitElementControl;
            if (control != null) {
                control.txtValue.IsReadOnly = (bool)args.NewValue;
                control.commentLink.IsEnabled = !(bool)args.NewValue;
                control.btnDelete.IsEnabled = !(bool)args.NewValue;
            }

        }

        public TraitViewModel Model { get; private set; }

        public event TraitEventHandler TraitChanged;

        public event TraitEventHandler TraitDeleted;

        public delegate void TraitEventHandler(object source, TraitViewModel model);

    }
}
