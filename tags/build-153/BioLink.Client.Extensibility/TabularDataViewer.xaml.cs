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

            foreach (DisplayColumnDefinition c in report.DisplayColumns) {
                DisplayColumnDefinition coldef = c;
                var column = new GridViewColumn { Header = BuildColumnHeader(coldef), DisplayMemberBinding = new Binding(String.Format("[{0}]", data.IndexOf(coldef.ColumnName))) };
                view.Columns.Add(column);
            }
            
            lvw.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));

            lvw.MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(lvw_MouseRightButtonUp);
            
            lvw.ItemsSource = Data.Rows;
            this.lvw.View = view;
        }

        private void EditSite(int siteID) {            
            PluginManager.Instance.EditLookupObject(LookupType.Site, siteID);
        }

        void lvw_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            var row = lvw.SelectedItem as MatrixRow;
            if (row != null) {
                ContextMenuBuilder builder = new ContextMenuBuilder(null);

                AddLookupItem(builder, "SiteID", LookupType.Site);
                AddLookupItem(builder, "SiteVisitID", LookupType.SiteVisit);
                AddLookupItem(builder, "MaterialID", LookupType.Material);
                AddLookupItem(builder, "BiotaID", LookupType.Taxon);

                if (Data.IndexOf("Lat") >= 0 && Data.IndexOf("Long") >= 0) {
                    if (builder.ContextMenu.HasItems) {
                        builder.Separator();
                        builder.New("Plot selected rows on Map").Handler(() => { PlotSelected(); }).End();
                        builder.New("Plot all rows on Map").Handler(() => { PlotAll(); }).End();
                    }
                }

                if (builder.ContextMenu.HasItems) {
                    builder.Separator();
                }
                
                builder.New("Export data...").Handler(() => { Export(); }).End();


                lvw.ContextMenu = builder.ContextMenu;
            }

        }

        private IMapProvider GetMap() {
            var maps = PluginManager.Instance.GetExtensionsOfType<IMapProvider>();
            if (maps != null && maps.Count > 0) {
                return maps[0];
            }
            return null;
        }

        private void PlotSelected() {
            var list = new List<MatrixRow>();
            if (lvw.SelectedItems.Count > 0) {
                foreach (object selected in lvw.SelectedItems) {
                    list.Add(selected as MatrixRow);
                }                
            }
            Plot(list);
        }

        private void PlotAll() {            
            var list = new List<MatrixRow>();
            if (lvw.SelectedItems.Count > 0) {
                foreach (MatrixRow row in Data.Rows) {
                    list.Add(row);
                }
            }
            Plot(list);
        }

        private void Plot(List<MatrixRow> rows) {
            var map = GetMap();
            if (map != null && rows.Count > 0) {
                var set = new MapPointSet(_report.Name);

                int latIndex = Data.IndexOf("Lat");
                int longIndex = Data.IndexOf("Long");
                int siteIndex = Data.IndexOf("SiteID");
                int siteVisitIndex = Data.IndexOf("SiteVisitID");
                int materialIndex = Data.IndexOf("MaterialID");

                foreach (MatrixRow row in rows) {
                    double? lat = row[latIndex] as double?;
                    double? lon = row[longIndex] as double?;
                    if (lat.HasValue && lon.HasValue) {
                        MapPoint p = new MapPoint(lat.Value, lon.Value);
                        set.Add(p);
                        if (siteIndex >= 0) {
                            p.SiteID = (int)row[siteIndex];
                        }
                        if (siteVisitIndex >= 0) {
                            p.SiteVisitID = (int)row[siteVisitIndex];
                        }
                        if (materialIndex >= 0) {
                            p.MaterialID = (int)row[materialIndex];
                        }
                    }
                }

                map.Show();
                map.PlotPoints(set);
            }
        }

        private void AddLookupItem(ContextMenuBuilder builder, String fieldName, LookupType lookupType) {
            int index = Data.IndexOf(fieldName);
            var row = lvw.SelectedItem as MatrixRow;
            if (index > -1) {
                builder.New("Edit " + lookupType.ToString()).Handler(() => { PluginManager.Instance.EditLookupObject(lookupType, (int)row[index]); }).End();
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

    public class AugmentedTextBlock : TextBlock {

    }

    public class TabularDataViewerSource : IReportViewerSource {

        public string Name {
            get { return "Table Viewer"; }
        }

        public FrameworkElement ConstructView(IBioLinkReport report, DataMatrix reportData, IProgressObserver progress) {
            TabularDataViewer viewer = new TabularDataViewer(report, reportData, progress);
            return viewer;
        }

    }
}

