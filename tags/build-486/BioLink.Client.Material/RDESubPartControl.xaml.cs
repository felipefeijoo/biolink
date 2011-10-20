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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for RDESubPartControl.xaml
    /// </summary>
    public partial class RDESubPartControl : UserControl {

        public RDESubPartControl() {
            InitializeComponent();
            if (!this.IsDesignTime()) {
                this.User = PluginManager.Instance.User;

                txtGender.BindUser(User, PickListType.Phrase, "Gender", TraitCategoryType.Material);
                txtLifeStage.BindUser(User, PickListType.Phrase, "Life Stage", TraitCategoryType.Material);
                txtSampleType.BindUser(User, PickListType.Phrase, "Sample Type", TraitCategoryType.Material);
                txtStorageMethod.BindUser(User, PickListType.Phrase, "Storage Method", TraitCategoryType.Material);
            }

        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(RDESubPartControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (RDESubPartControl)obj;
            if (control != null) {
                bool isReadOnly = (bool)args.NewValue;
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
