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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using System.Collections.ObjectModel;
using System.IO;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for MultimediaThumbnailViewer.xaml
    /// </summary>
    public partial class MultimediaThumbnailViewer : UserControl, IDisposable {

        private KeyedObjectTempFileManager<int?> _tempFileManager;

        private ObservableCollection<MultimediaLinkViewModel> _model;

        private const int THUMB_SIZE = 100;

        public MultimediaThumbnailViewer() {
            InitializeComponent();            
        }

        public MultimediaThumbnailViewer(IBioLinkReport report, Data.DataMatrix reportData, Utilities.IProgressObserver progress) {
            InitializeComponent();
            this.Report = report;
            this.ReportData = reportData;
            this.Progress = progress;

            var service = new SupportService(PluginManager.Instance.User);

            _tempFileManager = new KeyedObjectTempFileManager<int?>((mmId) => {
                if (mmId.HasValue) {
                    byte[] bytes = service.GetMultimediaBytes(mmId.Value);
                    if (bytes != null) {
                        return new MemoryStream(bytes);
                    }
                }
                return null;
            });

            Loaded += new RoutedEventHandler(MultimediaThumbnailViewer_Loaded);            
        }

        private bool _threadRunning = false;

        void MultimediaThumbnailViewer_Loaded(object sender, RoutedEventArgs e) {

            var service = new SupportService(PluginManager.Instance.User);

            if (_model == null) {
                using (new OverrideCursor(Cursors.Wait)) {
                    if (ReportData != null && ReportData.ContainsColumn("MultimediaLink")) {
                        int index = ReportData.IndexOf("MultimediaLink");
                        _model = new ObservableCollection<MultimediaLinkViewModel>();
                        foreach (MatrixRow row in ReportData) {
                            var link = row[index] as MultimediaLink;
                            if (link != null) {
                                _model.Add(new MultimediaLinkViewModel(link));
                            }
                        }
                        this.thumbList.ItemsSource = _model;
                    }
                }

                lock (_model) {
                    if (!_threadRunning) {
                        _threadRunning = true;
                        JobExecutor.QueueJob(() => {
                            if (_model != null) {

                                if (Progress != null) {
                                    Progress.ProgressStart("Generating thumbnails...");
                                }

                                int count = 0;
                                foreach (MultimediaLinkViewModel vm in _model) {

                                    bool generate = false;

                                    vm.InvokeIfRequired(() => {
                                        generate = vm.Thumbnail == null;
                                    });

                                    if (generate) {
                                        GenerateThumbnail(vm, THUMB_SIZE);
                                    }

                                    count++;
                                    if (Progress != null) {
                                        double percent = (((double) count) / ((double) _model.Count)) * 100.0;
                                        Progress.ProgressMessage(string.Format("Generating thumbnails ({0} of {1})", count, _model.Count), percent);
                                    }

                                }

                                if (Progress != null) {
                                    Progress.ProgressEnd(string.Format("{0} thumbnails generated",count));
                                }

                            }
                            _threadRunning = false;
                        });
                    }
                }
            }
        }

        private void GenerateThumbnail(MultimediaLinkViewModel item, int maxDimension) {
            string filename = _tempFileManager.GetContentFileName(item.MultimediaID, item.Extension);
            item.TempFilename = filename;
            this.InvokeIfRequired(() => {
                item.Thumbnail = GraphicsUtils.GenerateThumbnail(filename, maxDimension);
            });
        }


        protected IBioLinkReport Report { get; private set; }
        protected DataMatrix ReportData { get; private set; }
        protected IProgressObserver Progress { get; private set; }

        private void thumbList_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var selected = thumbList.SelectedItem as MultimediaLinkViewModel;
            if (selected != null) {
                var builder = new ContextMenuBuilder(null);
                string filename = _tempFileManager.GetContentFileName(selected.MultimediaID, selected.Extension);

                var verbMenuItems = SystemUtils.GetVerbsAsMenuItems(filename);
                foreach (MenuItem verbItem in verbMenuItems) {
                    builder.AddMenuItem(verbItem);                    
                }

                builder.Separator();
                builder.New("Show linked items...").Handler(() => ShowLinkedItems(selected)).End();
                builder.Separator();                
                builder.New("Save as...").Handler(() => SaveAs(selected)).End();
                builder.Separator();
                builder.New("Open in system editor...").Handler(()=>OpenSelected()).End();
                builder.Separator();
                builder.New("Properties...").Handler(() => ShowMultimediaProperties(selected)).End();

                thumbList.ContextMenu = builder.ContextMenu;
            }

        }

        protected User User { 
            get { return PluginManager.Instance.User; } 
        }

        private void ShowMultimediaProperties(MultimediaLinkViewModel selected) {
            if (selected == null) {
                return;
            }
            if (selected.MultimediaID < 0) {
                ErrorMessage.Show("You must first apply the changes before editing the details of this item!");
                return;
            }
            var service = new SupportService(User);
            var model = service.GetMultimedia(selected.MultimediaID);
            if (model != null) {
                var detailsControl = new MultimediaDetails(model, User);                
                PluginManager.Instance.AddNonDockableContent(Plugin, detailsControl, string.Format("Multimedia details [{0}]", model.MultimediaID), SizeToContent.Manual);
            }
        }

        protected IBioLinkPlugin Plugin {
            get { return PluginManager.Instance.GetPluginByName("Tools"); }
        }


        private void SaveAs(MultimediaLinkViewModel selected) {
            var dlg = new Microsoft.Win32.SaveFileDialog();
            var filename = string.Format("{0}.{1}", selected.Name, selected.Extension);
            dlg.FileName = filename;
            if (dlg.ShowDialog() == true) {
                var srcFile = _tempFileManager.GetContentFileName(selected.MultimediaID, selected.Extension);
                File.Copy(srcFile, dlg.FileName);
                InfoBox.Show("Item has been saved to " + dlg.FileName, "File saved", this);
            }
        }

        private void ShowLinkedItems(MultimediaLinkViewModel selected) {
            if (selected != null) {
                selected.Icon = GraphicsUtils.GenerateThumbnail(selected.TempFilename, 48);                
                PluginManager.Instance.AddNonDockableContent(Plugin, new LinkedMultimediaItemsControl(selected), "Items linked to multimedia " + selected.MultimediaID, SizeToContent.Manual);
            }
        }

        public void OpenSelected() {
            var item = thumbList.SelectedItem as MultimediaLinkViewModel;
            if (item != null) {
                string filename = _tempFileManager.GetContentFileName(item.MultimediaID, item.Extension);
                try {
                    System.Diagnostics.Process.Start(filename);
                } catch (Exception ex) {
                    ErrorMessage.Show(ex.Message);
                }
            }
        }


        private void thumbList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            OpenSelected();
        }

        public void Dispose() {
            if (_tempFileManager != null) {
                _tempFileManager.Dispose();
                _tempFileManager = null;
            }
        }

        private void btnProperties_Click(object sender, RoutedEventArgs e) {
            var selected = thumbList.SelectedItem as MultimediaLinkViewModel;
            if (selected != null) {
                ShowMultimediaProperties(selected);
            }
        }

        private void btnLinks_Click(object sender, RoutedEventArgs e) {
            var selected = thumbList.SelectedItem as MultimediaLinkViewModel;
            if (selected != null) {
                ShowLinkedItems(selected);
            }
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e) {
            var selected = thumbList.SelectedItem as MultimediaLinkViewModel;
            if (selected != null) {
                SaveAs(selected);
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e) {
            SaveAll();
        }

        private void SaveAll() {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;            
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {
                JobExecutor.QueueJob(() => {
                    
                    if (Progress != null) {
                        Progress.ProgressStart("Exporting files...");
                    }
                    int count = 0;
                    foreach (MultimediaLinkViewModel vm in _model) {
                        var destFile = string.Format("{0}\\{1}.{2}", dlg.SelectedPath, vm.Name, vm.Extension);
                        var filename = _tempFileManager.GetContentFileName(vm.MultimediaID, vm.Extension);
                        try {
                            File.Copy(filename, destFile, true);
                            count++;
                            if (Progress != null) {
                                double percent = (((double)count) / ((double)_model.Count)) * 100.0;
                                Progress.ProgressMessage(string.Format("Exporting files ({0} of {1})", count, _model.Count), percent);
                            }

                        } catch (Exception ex) {
                            GlobalExceptionHandler.Handle(ex);
                        }

                    }
                    if (Progress != null) {
                        Progress.ProgressEnd(string.Format("{0} files exported.", count));
                    }
                    
                });
            }
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
            ListCollectionView dataView = CollectionViewSource.GetDefaultView(thumbList.ItemsSource) as ListCollectionView;
            text = text.ToLower();
            dataView.Filter = (obj) => {
                var mm = obj as MultimediaLinkViewModel;

                if (mm != null) {
                    if (!string.IsNullOrWhiteSpace(mm.MultimediaType)) {
                        if (mm.MultimediaType.ToLower().Contains(text)) {
                            return true;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(mm.Name)) {
                        if (mm.Name.ToLower().Contains(text)) {
                            return true;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(mm.Caption)) {
                        if (mm.Caption.ToLower().Contains(text)) {
                            return true;
                        }
                    }
                    
                }
                return false;
            };

            dataView.Refresh();
            if (Progress != null) {
                Progress.ProgressMessage(String.Format("Showing {0} of {1} items", dataView.Count, ReportData.Rows.Count));
            }
        }


        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e) {
            if (String.IsNullOrEmpty(txtFilter.Text)) {
                ClearFilter();
            }
        }

        private void ClearFilter() {
            System.ComponentModel.ICollectionView dataView = CollectionViewSource.GetDefaultView(thumbList.ItemsSource);
            dataView.Filter = null;
            dataView.Refresh();
            if (Progress != null) {
                Progress.ProgressMessage(String.Format("Displaying all {0} items.", ReportData.Rows.Count));
            }

        }
    }

    public class MultimediaThumbnailViewerSource : IReportViewerSource {

        public string Name {
            get { return "Thumbnails"; }
        }

        public FrameworkElement ConstructView(IBioLinkReport report, Data.DataMatrix reportData, Utilities.IProgressObserver progress) {
            var viewer = new MultimediaThumbnailViewer(report, reportData, progress);

            return viewer;
        }
    }
}
