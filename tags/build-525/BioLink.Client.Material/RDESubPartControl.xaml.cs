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
using System.Windows;
using System.Windows.Controls;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for RDESubPartControl.xaml
    /// </summary>
    public partial class RDESubPartControl : UserControl {

        public RDESubPartControl() {
            InitializeComponent();
            if (!this.IsDesignTime()) {
                User = PluginManager.Instance.User;

                txtGender.BindUser(User, PickListType.Phrase, "Gender", TraitCategoryType.Material);
                txtLifeStage.BindUser(User, PickListType.Phrase, "Life Stage", TraitCategoryType.Material);
                txtSampleType.BindUser(User, PickListType.Phrase, "Sample Type", TraitCategoryType.Material);
                txtStorageMethod.BindUser(User, PickListType.Phrase, "Storage Method", TraitCategoryType.Material);
            }

            DataContextChanged +=delegate {
                                        gridMain.IsEnabled = DataContext != null;
                                        };
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(RDESubPartControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsReadOnlyChanged));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (RDESubPartControl)obj;
            if (control != null) {
                var isReadOnly = (bool)args.NewValue;
                control.txtGender.IsReadOnly = isReadOnly;
                control.txtLifeStage.IsReadOnly = isReadOnly;
                control.txtNoSpecimens.IsReadOnly = isReadOnly;
                control.txtNotes.IsReadOnly = isReadOnly;
                control.txtSampleType.IsReadOnly = isReadOnly;
                control.txtStorageMethod.IsReadOnly = isReadOnly;
            }
        }

        internal User User { get; private set; }
    }
}
