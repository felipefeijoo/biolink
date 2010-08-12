using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for TabularDataViewer.xaml
    /// </summary>
    public partial class TabularDataViewer : UserControl, IDisposable {

        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        #region DesignTime Constructor
        public TabularDataViewer() {
            InitializeComponent();
        }
        #endregion


        public TabularDataViewer(IBioLinkReport report, DataMatrix data) {
            InitializeComponent();
            this.Data = data;            
            GridView view = new GridView();

            foreach (DisplayColumnDefinition c in report.DisplayColumns) {
                DisplayColumnDefinition coldef = c;
                var column = new GridViewColumn { Header = BuildColumnHeader(coldef), DisplayMemberBinding = new Binding(String.Format("[{0}]", data.IndexOf(coldef.ColumnName))) };
                view.Columns.Add(column);
            }
            
            lvw.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));
            
            lvw.ItemsSource = Data.Rows;
            this.lvw.View = view;
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
            ICollectionView dataView = CollectionViewSource.GetDefaultView(lvw.ItemsSource);
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
                this.InvokeIfRequired(() => {
                    ClearFilter();
                });
            } else {
                this.InvokeIfRequired(() => {
                    SetFilter(text);
                });
            }
        }

        private void SetFilter(string text) {
            if (String.IsNullOrEmpty(text)) {
                return;
            }
            ICollectionView dataView = CollectionViewSource.GetDefaultView(lvw.ItemsSource);
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
        }

        private void ClearFilter() {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(lvw.ItemsSource);
            dataView.Filter = null;
            dataView.Refresh();
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
            TabularDataViewer viewer = new TabularDataViewer(report, reportData);
            return viewer;
        }

    }
}
