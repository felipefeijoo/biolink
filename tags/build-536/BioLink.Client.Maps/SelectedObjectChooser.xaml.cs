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
using System.Windows.Shapes;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.ComponentModel;

namespace BioLink.Client.Maps {

    /// <summary>
    /// Interaction logic for SelectedObjectChooser.xaml
    /// </summary>
    public partial class SelectedObjectChooser : Window {

        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        #region Designer Ctor
        public SelectedObjectChooser() {
            InitializeComponent();
        }
        #endregion

        public SelectedObjectChooser(User user, List<ViewModelBase> items, LookupType objectType) {
            InitializeComponent();

            GridView view = new GridView();

            var m = new RDESiteVisit();

            switch (objectType) {
                case LookupType.Site:                    
                    AddColumn(view, "Site Name", "SiteName", 300);
                    AddColumn(view, "Site ID", "SiteID");
                    AddColumn(view, "Longitude","Longitude");
                    AddColumn(view, "Latitude","Latitude");
                    break;
                case LookupType.SiteVisit:                   
                    AddColumn(view, "Site Visit Name", "VisitName", 300);
                    AddColumn(view, "Site Name", "Site.SiteName", 300);
                    AddColumn(view, "Site Visit ID", "SiteVisitID");
                    AddColumn(view, "Site ID", "Site.SiteID");
                    AddColumn(view, "Longitude","Site.Longitude");
                    AddColumn(view, "Latitude","Site.Latitude");
                    break;
                case LookupType.Material:
                    AddColumn(view, "Material Name", "MaterialName", 100);
                    AddColumn(view, "BiotaID", "BiotaID");
                    AddColumn(view, "Site Visit Name", "SiteVisit.VisitName", 300);
                    AddColumn(view, "Site Name", "SiteVisit.Site.SiteName", 300);
                    AddColumn(view, "Material ID", "MaterialID");
                    AddColumn(view, "Site Visit ID", "SiteVisit.SiteVisitID");
                    AddColumn(view, "Site ID", "SiteVisit.Site.SiteID");
                    AddColumn(view, "Longitude","SiteVisit.Site.Longitude");
                    AddColumn(view, "Latitude","SiteVisit.Site.Latitude");
                    break;

            }

            lvw.View = view;
            lvw.ItemsSource = items;

            lvw.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));

            lblStatus.Content = string.Format("{0} items listed.", items.Count);

            lvw.MouseRightButtonUp += new MouseButtonEventHandler(lvw_MouseRightButtonUp);

            this.Title = string.Format("Multiple {0} records found at this location", objectType.ToString());
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e) {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null) {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding) {
                    if (headerClicked != _lastHeaderClicked) {
                        direction = ListSortDirection.Ascending;
                    } else {
                        if (_lastDirection == ListSortDirection.Ascending) {
                            direction = ListSortDirection.Descending;
                        } else {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    TextBlock b = headerClicked.Content as TextBlock;
                    if (b != null && b.Tag != null) {
                        Sort(direction, b.Tag as string);

                        if (direction == ListSortDirection.Ascending) {
                            headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowUp"] as DataTemplate;
                        } else {
                            headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowDown"] as DataTemplate;
                        }

                        // Remove arrow from previously sorted header
                        if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked) {
                            _lastHeaderClicked.Column.HeaderTemplate = null;
                        }

                        _lastHeaderClicked = headerClicked;
                        _lastDirection = direction;
                    }
                }
            }
        }

        private object BuildColumnHeader(string caption, string propertyName) {
            TextBlock t = new TextBlock();
            t.TextAlignment = TextAlignment.Left;
            t.Tag = propertyName;
            t.Text = caption;
            return t;
        }

        private void Sort(ListSortDirection direction, string propertyName) {
            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvw.ItemsSource) as ListCollectionView;
            dataView.SortDescriptions.Clear();


            SortDescription sd = new SortDescription(propertyName, direction);

            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }


        void lvw_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {

            // Clear the last context menu...
            lvw.ContextMenu = null;

            var viewModel = lvw.SelectedItem as ViewModelBase;
            if (viewModel != null) {
                ContextMenuBuilder builder = new ContextMenuBuilder(null);
                if (viewModel is RDESiteViewModel) {
                    var site = viewModel as RDESiteViewModel;
                    builder.New("Edit Site...").Handler(() => { PluginManager.Instance.EditLookupObject(LookupType.Site, site.SiteID); }).End();
                } else if (viewModel is RDESiteViewModel) {
                    var visit = viewModel as RDESiteVisitViewModel;
                    builder.New("Edit Site...").Handler(() => { PluginManager.Instance.EditLookupObject(LookupType.Site, visit.SiteID); }).End();
                    builder.New("Edit Site Visit...").Handler(() => { PluginManager.Instance.EditLookupObject(LookupType.SiteVisit, visit.SiteVisitID); }).End();
                } else if (viewModel is RDEMaterialViewModel) {
                    var material = viewModel as RDEMaterialViewModel;
                    builder.New("Edit Site...").Handler(() => { PluginManager.Instance.EditLookupObject(LookupType.Site, material.SiteVisit.SiteID); }).End();
                    builder.New("Edit Site Visit...").Handler(() => { PluginManager.Instance.EditLookupObject(LookupType.SiteVisit, material.SiteVisitID); }).End();
                    builder.New("Edit Material...").Handler(() => { PluginManager.Instance.EditLookupObject(LookupType.Material, material.MaterialID); }).End();
                    if (material.BiotaID != 0) {
                        builder.New("Edit Taxon...").Handler(() => { PluginManager.Instance.EditLookupObject(LookupType.Taxon, material.BiotaID); }).End();
                    }
                }
                if (builder.HasItems) {
                    lvw.ContextMenu = builder.ContextMenu;
                }
            }
            
        }

        private void AddColumn(GridView view, string columnName, string DisplayMemberPath, int width = 50) {
            var column = new GridViewColumn { Header = BuildColumnHeader(columnName, DisplayMemberPath), DisplayMemberBinding = new Binding(DisplayMemberPath), Width=width };
            view.Columns.Add(column);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
