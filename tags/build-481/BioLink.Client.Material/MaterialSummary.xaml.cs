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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for MaterialSummary.xaml
    /// </summary>
    public partial class MaterialSummary : DatabaseCommandControl, ILazyPopulateControl {

        #region Designer Ctor
        public MaterialSummary() {
            InitializeComponent();
        }
        #endregion

        public MaterialSummary(User user, MaterialViewModel material)
            : base(user, "MaterialSummary:" + material.MaterialID) {
            InitializeComponent();
            this.Material = material;            
        }

        private void LoadSummary() {
            var service = new MaterialService(User);
            String rtf = service.GetMaterialSummary(Material.Model);
            txtSummary.RTF = rtf;
            IsPopulated = true;
        }

        public bool IsPopulated { get; private set; }

        public void Populate() {
            if (!IsPopulated) {
                LoadSummary();
            }
        }

        public MaterialViewModel Material { get; private set; }

        private void btnRefresh_Click(object sender, RoutedEventArgs e) {
            LoadSummary();
        }

        private void btnEditSite_Click(object sender, RoutedEventArgs e) {
            PluginManager.Instance.EditLookupObject(LookupType.Site, Material.SiteID);
        }

        private void btnEditSiteVisit_Click(object sender, RoutedEventArgs e) {
            PluginManager.Instance.EditLookupObject(LookupType.SiteVisit, Material.SiteVisitID);
        }
    }
}
