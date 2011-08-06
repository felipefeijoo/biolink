using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BioLink.Client.Utilities;
using BioLink.Data;
using System.Collections;
using System.Collections.Generic;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for TabularDataViewer.xaml
    /// </summary>
    public partial class TabularDataViewer : UserControl, IDisposable {

        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;
        private IProgressObserver _progress;
        private IBioLinkReport _report;

        #region DesignTime Constructor
        public TabularDataViewer() {
            InitializeComponent();
        }
        #endregion

        public TabularDataViewer(IBioLinkReport report, DataMatrix data, IProgressObserver progress) {
            InitializeComponent();
            this.Data = data;
            _progress = progress;
            _report = report;
            GridView view = new GridView();

            var columns = report.DisplayColumns;
            if (columns == null || columns.Count == 0) {
                columns = GenerateDefaultColumns(data);
            }

            var hcs = viewerGrid.Resources["hcs"] as Style;

            foreach (DisplayColumnDefinition c in columns) {
                DisplayColumnDefinition coldef = c;
                var column = new GridViewColumn { Header = BuildColumnHeader(coldef), DisplayMemberBinding = new Binding(String.Format("[{0}]", data.IndexOf(coldef.ColumnName))) , HeaderContainerStyle = hcs};
                view.Columns.Add(column);
            }
            
            lvw.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));

            lvw.MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(lvw_MouseRightButtonUp);
            
            lvw.ItemsSource = Data.Rows;
            this.lvw.View = view;
        }

        private List<DisplayColumnDefinition> GenerateDefaultColumns(DataMatrix data) {
            var list = new List<DisplayColumnDefinition>();
            foreach (MatrixColumn col in data.Columns) {
                if (!col.IsHidden) {
                    var colDef = new DisplayColumnDefinition { ColumnName = col.Name, DisplayName = col.Name };
                    list.Add(colDef);
                }
            }
            return list;
        }

        private void AddMapItems(ContextMenuBuilder builder, params string[] colAliases) {
            foreach (string colpair in colAliases) {
                var bits = colpair.Split(',');
                if (bits.Length == 2) {
                    if (Data.ContainsColumn(bits[0]) && Data.ContainsColumn(bits[1])) {
                        var latColName = bits[0];
                        var longColName = bits[1];
                        if (!string.IsNullOrEmpty(longColName) && !string.IsNullOrEmpty(latColName)) {
                            if (builder.ContextMenu.HasItems) {
                                builder.Separator();
                            }
                            builder.New("Plot selected rows on Map").Handler(() => { PlotSelected(longColName, latColName); }).End();
                            builder.New("Plot all rows on Map").Handler(() => { PlotAll(longColName, latColName); }).End();
                        }
                        break;
                    }
                }
            }
        }

        void lvw_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            var row = lvw.SelectedItem as MatrixRow;
            if (row != null) {
                ContextMenuBuilder builder = new ContextMenuBuilder(null);

                AddLookupItem(builder, LookupType.Site, "SiteID", "intSiteID", "Site Identifier");
                AddLookupItem(builder, LookupType.SiteVisit, "SiteVisitID", "intSiteVisitID", "Visit Identifier");
                AddLookupItem(builder, LookupType.Material, "MaterialID", "intMaterialID", "Material Identifier");
                AddLookupItem(builder, LookupType.Taxon, "BiotaID", "intBiotaID", "Taxon Identifier", "TaxonID");

                string latColName = null;
                string longColName = null;

                foreach (MatrixColumn col in Data.Columns) {
                    if (latColName == null && col.Name.Contains("Lat")) {
                        latColName = col.Name;
                    }

                    if (longColName == null && col.Name.Contains("Long")) {
                        longColName = col.Name;
                    }
                }

                AddMapItems(builder, "Lat,Long", "Latitude,Longitude", "Y,X", "Y1,X1");

                if (builder.ContextMenu.HasItems) {
                    builder.Separator();
                }
                
                builder.New("Export data...").Handler(() => { Export(); }).End();


                lvw.ContextMenu = builder.ContextMenu;
            }

        }

        private void PlotSelected(string longColName, string latColName) {

            var selectedRowIndexes = new int[lvw.SelectedItems.Count];
            if (lvw.SelectedItems.Count > 0) {
                int i = 0;
                foreach (object selected in lvw.SelectedItems) {
                    var row = selected as MatrixRow;
                    selectedRowIndexes[i++] = Data.Rows.IndexOf(row);
                }                
            }
            Plot(selectedRowIndexes, longColName, latColName);
        }

        private void PlotAll(string longColName, string latColName) {            
            Plot(null, longColName, latColName);
        }

        private string FindColumnFromAlias(string @default, params string[] aliases) {

            foreach (string alias in aliases) {
                if (Data.ContainsColumn(alias)) {
                    return alias;
                }
            }

            return @default;
        }

        private void Plot(int[] selectedRowIndexes, string longColName, string latColName) {
            var map = PluginManager.Instance.GetMap();
            if (map != null) {
                var set = new MatrixMapPointSet(_report.Name, Data, selectedRowIndexes);
                set.LongitudeColumn = longColName;
                set.LatitudeColumn = latColName;

                set.SiteIDColumn = FindColumnFromAlias(set.SiteIDColumn, "SiteID", "intSiteID", "Site Identifier");
                set.SiteVisitIDColumn = FindColumnFromAlias(set.SiteVisitIDColumn, "SiteVisitID", "intSiteVisitID", "Visit Identifier");
                set.MaterialIDColumn = FindColumnFromAlias(set.MaterialIDColumn, "MaterialID", "intMaterialID", "Material Identifier");

                map.Show();
                map.PlotPoints(set);
            }
        }

        private void AddLookupItem(ContextMenuBuilder builder, LookupType lookupType, params String[] aliases) {

            int index = -1;
            foreach (string alias in aliases) {
                var field = alias;

                for (int i = 0; i < Data.Columns.Count; ++i) {
                    var col = Data.Columns[i];
                    if (col.Name.Contains(alias)) {
                        index = i;
                        break;
                    }
                }
                
                if (index >= 0) {
                    break;
                }
            }

            if (index > -1) {
                var row = lvw.SelectedItem as MatrixRow;
                var enabled = row[index] != null;
                builder.New("Edit " + lookupType.ToString()).Handler(() => { PluginManager.Instance.EditLookupObject(lookupType, (int)row[index]); }).Enabled(enabled).End();
            }

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
                    Sort(b.Tag as DisplayColumnDefinition, direction);

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

        private void Sort(DisplayColumnDefinition coldef, ListSortDirection direction) {
            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvw.ItemsSource) as ListCollectionView;
            dataView.SortDescriptions.Clear();
            int columnIndex = Data.IndexOf(coldef.ColumnName);

            SortDescription sd = new SortDescription(String.Format("[{0}]", columnIndex), direction);

            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();            
        }

        private object BuildColumnHeader(DisplayColumnDefinition coldef) {
            TextBlock t = new TextBlock();
            t.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            t.TextAlignment = TextAlignment.Left;
            t.Tag = coldef;
            t.Text = coldef.DisplayName;
            return t;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e) {
            Export();
        }

        private void Export() {
            ExportData exporter = new ExportData(Data, _progress);
            exporter.Owner = PluginManager.Instance.ParentWindow;
            bool ok = exporter.ShowDialog().GetValueOrDefault(false);
        }

        #region Properties

        public DataMatrix Data { get; private set; }

        #endregion



        public void Dispose() {
            this.Data = null;
            lvw.ItemsSource = null;
        }

        private void txtFilter_TypingPaused(string text) {
            if (String.IsNullOrEmpty(text)) {
                ClearFilter();
            } else {
                SetFilter(text);
            }
        }

        private void SetFilter(string text) {
            if (String.IsNullOrEmpty(text)) {
                return;
            }
            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvw.ItemsSource) as ListCollectionView;
            text = text.ToLower();
            dataView.Filter = (obj) => { 
                var row = obj as MatrixRow;
                
                if (row != null) {
                    object match = row.First((colval) => {
                        if (colval != null) {
                            return colval.ToString().ToLower().Contains(text);
                        }
                        return false;
                    });

                    if (match != null) {
                        return true;
                    }
                }
                return false; 
            };            

            dataView.Refresh();
            if (_progress != null) {
                _progress.ProgressMessage(String.Format("Showing {0} of {1} rows", dataView.Count, Data.Rows.Count));
            }
        }

        private void ClearFilter() {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(lvw.ItemsSource);
            dataView.Filter = null;
            dataView.Refresh();
            if (_progress != null) {
                _progress.ProgressMessage(String.Format("{0} rows retrieved.", Data.Rows.Count));
            }
        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e) {
            if (String.IsNullOrEmpty(txtFilter.Text)) {
                ClearFilter();
            }
        }
    }

    public class TabularDataViewerSource : IReportViewerSource {

        public string Name {
            get { return "Table"; }
        }

        public FrameworkElement ConstructView(IBioLinkReport report, DataMatrix reportData, IProgressObserver progress) {
            TabularDataViewer viewer = new TabularDataViewer(report, reportData, progress);
            return viewer;
        }

    }

}

