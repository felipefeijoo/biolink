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

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for TaxonTooltipContent.xaml
    /// </summary>
    public partial class TaxonTooltipContent : UserControl {

        public TaxonTooltipContent(User user, int taxonId) {
            InitializeComponent();
            TaxonID = taxonId;            
            User = user;
            Loaded += new RoutedEventHandler(TaxonTooltipContent_Loaded);
        }

        void TaxonTooltipContent_Loaded(object sender, RoutedEventArgs e) {
            var service = new TaxaService(PluginManager.Instance.User);
            var Model = service.GetTaxon(TaxonID);
            if (Model == null) {
                return;
            }

            var elementRank = service.GetTaxonRank(Model);

            var header = Model.TaxaFullName;

            if (Model.AvailableName.ValueOrFalse()) {
                header += " (Available name)";
            } else if (Model.LiteratureName.ValueOrFalse()) {
                header += " (Literature name)";
            }

            lblHeader.Content = header;
            var rankName = (elementRank == null ? "Unranked" : elementRank.LongName + (Model.AvailableName.ValueOrFalse() ? " Available Name" : ""));

            var kingdom = string.IsNullOrEmpty(Model.KingdomCode) ? "No Kingdom" : Model.KingdomCode;

            lblSystem.Content = string.Format("[{0}] {1} <{2}> Last updated: {3:g} by {4}", Model.TaxaID.Value, rankName, kingdom, Model.DateLastUpdated, Model.WhoLastUpdated);
            imgIcon.Source = TaxonViewModel.ConstructIcon(Model.AvailableName.ValueOrFalse() || Model.LiteratureName.ValueOrFalse(), Model.ElemType, false);            

            // Ancestry
            var bits = Model.Parentage.Split('\\');

            int i = bits.Length - 1;
            int j = 0;
            var parents = new Stack<Taxon>();
            while (--i >= 0 && j++ < 3) {
                if (!string.IsNullOrEmpty(bits[i])) {
                    var parentId = Int32.Parse(bits[i]);
                    var parent = service.GetTaxon(parentId);
                    parents.Push(parent);
                }
            }

            i = 0;
            grdAncestry.Children.Clear();
            
            foreach (Taxon t in parents) {
                var parentPanel = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(i * 15, i * 25, 0, 0) };
                var parentIcon = new Image() { VerticalAlignment = System.Windows.VerticalAlignment.Top, UseLayoutRounding = true, SnapsToDevicePixels = true, Stretch = Stretch.None, Margin = new Thickness(6, 0, 6, 0) };                
                parentIcon.Source = TaxonViewModel.ConstructIcon(t.AvailableName.ValueOrFalse() || t.LiteratureName.ValueOrFalse(), t.ElemType, false);                
                parentPanel.Children.Add(parentIcon);
                var rank = service.GetTaxonRank(t);
                rankName = (rank == null ? "Unranked" : rank.LongName);
                var txt = new TextBlock() {VerticalAlignment = System.Windows.VerticalAlignment.Top, Text = string.Format("{1}   ({0})", rankName, t.TaxaFullName) };
                parentPanel.Children.Add(txt);
                grdAncestry.Children.Add(parentPanel);
                i++;
            }

        }

        protected int TaxonID { get; private set; }        

        protected User User { get; private set; }
    }
}
