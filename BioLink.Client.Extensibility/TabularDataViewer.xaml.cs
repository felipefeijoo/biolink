using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
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


            foreach (MatrixRow row in Data.Rows) {
                object val = row[columnIndex];
                if (val == null) {
                    row[columnIndex] = "XXXX";
                } else {
                    if (val.GetType() != typeof(String)) {
                        row[columnIndex] = "YYYY";
                    }
                }
            }

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

        #region Properties

        public DataMatrix Data { get; private set; }

        #endregion



        public void Dispose() {
            this.Data = null;
            lvw.ItemsSource = null;
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
