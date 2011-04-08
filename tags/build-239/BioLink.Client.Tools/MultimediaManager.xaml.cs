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
using System.IO;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace BioLink.Client.Tools {

    /// <summary>
    /// Interaction logic for MultimediaManager.xaml
    /// </summary>
    public partial class MultimediaManager : DatabaseActionControl {

        private List<string> _extensions;
        private List<string> _multimediaTypes;
        private KeyedObjectTempFileManager<int?> _tempFileManager;
        private ObservableCollection<MultimediaLinkViewModel> _model;
        private bool _IsDragging;
        private Point _startPoint;

        public MultimediaManager(User user) : base(user, "MultimediaManager") {
            InitializeComponent();

            var service = new SupportService(user);
            _extensions = service.GetMultimediaExtensions();
            var types = service.GetMultimediaTypes();
            _extensions.Insert(0, "(All)");

            _multimediaTypes = new List<string>();
            _multimediaTypes.Add("(All)");
            foreach (MultimediaType type in types) {
                if (!string.IsNullOrWhiteSpace(type.Name)) {
                    _multimediaTypes.Add(type.Name);
                }
            }

            cmbExtension.ItemsSource = _extensions;
            cmbExtension.SelectedIndex = 0;
            cmbType.ItemsSource = _multimediaTypes;
            cmbType.SelectedIndex = 0;

            _tempFileManager = new KeyedObjectTempFileManager<int?>((mmId) => {
                if (mmId.HasValue) {
                    byte[] bytes = service.GetMultimediaBytes(mmId.Value);
                    return new MemoryStream(bytes);
                }
                return null;
            });

            txtCriteria.KeyUp += new KeyEventHandler(txtCriteria_KeyUp);

            lvw.MouseRightButtonUp += new MouseButtonEventHandler(lvw_MouseRightButtonUp);

            lvw.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lvw_PreviewMouseLeftButtonDown);
            lvw.PreviewMouseMove += new MouseEventHandler(lvw_PreviewMouseMove);
        }

        void lvw_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseMove(e, lvw);
        }

        void lvw_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(lvw);
        }

        private void CommonPreviewMouseMove(MouseEventArgs e, ListView listView) {

            if (_startPoint == null) {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging) {
                Point position = e.GetPosition(listView);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    if (listView.SelectedItem != null) {

                        ListViewItem item = lvw.ItemContainerGenerator.ContainerFromItem(listView.SelectedItem) as ListViewItem;
                        if (item != null) {
                            StartDrag(e, lvw, item);
                        }
                    }
                }
            }
        }

        private void StartDrag(MouseEventArgs mouseEventArgs, ListView listView, ListViewItem item) {

            var selected = listView.SelectedItem as MultimediaLinkViewModel;
            if (selected != null) {
                var data = new DataObject("MultimediaLink", selected.Model);

                try {
                    _IsDragging = true;
                    DragDrop.DoDragDrop(item, data, DragDropEffects.Link);
                } finally {
                    _IsDragging = false;
                }
            }

            InvalidateVisual();
        }

        void txtCriteria_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                DoFind();
            }
        }

        private void ViewMultimedia(MultimediaLinkViewModel selected) {
            var filename = _tempFileManager.GetContentFileName(selected.MultimediaID, selected.Extension);
            if (!string.IsNullOrWhiteSpace(filename)) {
                SystemUtils.ShellExecute(filename);
            }
        }

        private void AddNew() {
            var dlg = new OpenFileDialog();
            dlg.Filter = "All files (*.*)|*.*";
            dlg.Multiselect = true;
            if (dlg.ShowDialog().ValueOrFalse()) {
                foreach (string filename in dlg.FileNames) {
                    FileInfo finfo = new FileInfo(filename);
                    if (finfo.Exists) {
                        MultimediaLink model = null;
                        MultimediaLinkViewModel viewModel = null;
                        Multimedia duplicate = null;
                        var action = CheckDuplicate(finfo, out duplicate);
                        switch (action) {
                            case MultimediaDuplicateAction.Cancel:
                                // Do nothing
                                break;
                            case MultimediaDuplicateAction.NoDuplicate:
                            case MultimediaDuplicateAction.InsertDuplicate:
                                // Insert new multimedia and new link
                                model = new MultimediaLink();
                                model.MultimediaID = NextNewId();
                                model.MultimediaLinkID = model.MultimediaID;
                                if (finfo.Name.Contains(".")) {
                                    model.Name = finfo.Name.Substring(0, finfo.Name.LastIndexOf("."));
                                    model.Extension = finfo.Extension.Substring(1);
                                } else {
                                    model.Name = finfo.Name;
                                }
                                viewModel = new MultimediaLinkViewModel(model);
                                _tempFileManager.CopyToTempFile(viewModel.MultimediaID, filename);
                                _model.Add(viewModel);
                                RegisterPendingChange(new InsertMultimediaAction(model, _tempFileManager.GetContentFileName(viewModel.MultimediaID, finfo.Extension.Substring(1))));
                                break;
                            case MultimediaDuplicateAction.UseExisting:
                                // Should never get here!
                                break;
                            case MultimediaDuplicateAction.ReplaceExisting:
                                // register an update for the multimedia,
                                // and insert a new link
                                // Link to existing multimedia
                                model = new MultimediaLink();
                                model.MultimediaID = duplicate.MultimediaID;
                                model.MultimediaLinkID = -1;
                                model.Name = duplicate.Name;
                                model.Extension = duplicate.FileExtension;
                                viewModel = new MultimediaLinkViewModel(model);                                
                                _model.Add(viewModel);
                                _tempFileManager.CopyToTempFile(viewModel.MultimediaID, filename);
                                RegisterPendingChange(new UpdateMultimediaBytesAction(model, filename));
                                break;
                        }

                        if (viewModel != null) {
                            viewModel.IsSelected = true;
                        }
                    }
                }
            }

        }

        public MultimediaDuplicateAction CheckDuplicate(FileInfo file, out Multimedia duplicate) {
            int sizeInBytes = 0;
            var service = new SupportService(User);
            duplicate = service.FindDuplicateMultimedia(file, out sizeInBytes);
            if (duplicate != null) {
                var frm = new DuplicateItemOptions(duplicate, sizeInBytes, true);
                frm.Owner = this.FindParentWindow();
                if (frm.ShowDialog().ValueOrFalse()) {
                    return frm.SelectedAction;
                } else {
                    return MultimediaDuplicateAction.Cancel;
                }
            }
            return MultimediaDuplicateAction.NoDuplicate;
        }

        private int NextNewId() {
            int newId = -1;
            foreach (MultimediaLinkViewModel model in _model) {
                if (model.MultimediaID <= newId) {
                    newId = model.MultimediaID - 1;
                }
            }
            return newId;
        }


        private void DeleteMultimedia(MultimediaLinkViewModel selected) {
            if (selected == null) {
                return;
            }

            if (this.Question(string.Format("Are you sure you wish to permanently delete '{0}'?", selected.Name), "Delete multimedia?")) {
                _model.Remove(selected);
                RegisterUniquePendingChange(new DeleteMultimedia(selected.MultimediaID));
            }
        }

        private void SaveMultimedia(MultimediaLinkViewModel selected) {
            if (selected == null) {
                return;
            }

            var dlg = new SaveFileDialog();
            dlg.DefaultExt = selected.Extension;
            dlg.FileName = selected.Name + "." + selected.Extension;
            if (dlg.ShowDialog() == true) {
                var filename = _tempFileManager.GetContentFileName(selected.MultimediaID, selected.Extension);
                var finfo = new FileInfo(filename);
                finfo.CopyTo(dlg.FileName);
            }
        }

        void lvw_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {

            var selected = lvw.SelectedItem as MultimediaLinkViewModel;
            if (selected == null) {
                return;
            }

            ContextMenuBuilder builder = new ContextMenuBuilder(null);

            builder.New("_Edit details...").Handler(() => { ShowMultimediaProperties(selected); }).End();
            builder.New("_View multimedia...").Handler(() => { ViewMultimedia(selected); }).End();
            builder.New("Save as...").Handler(() => { SaveMultimedia(selected); }).End();
            builder.Separator();
            var filename = _tempFileManager.GetContentFileName(selected.MultimediaID, selected.Extension);
            var verbMenuItems = SystemUtils.GetVerbsAsMenuItems(filename);
            foreach (MenuItem verbItem in verbMenuItems) {
                builder.AddMenuItem(verbItem);
            }
            builder.Separator();
            builder.New("_Add new...").Handler(() => { AddNew(); }).End();
            builder.New("_Delete").Handler(() => { DeleteMultimedia(selected); }).End();

            lvw.ContextMenu = builder.ContextMenu;
        }

        public override void Dispose() {
            if (_tempFileManager != null) {
                _tempFileManager.Dispose();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            DoFind();
        }

        private void DoFind() {
            var service = new SupportService(User);
            var extension = cmbExtension.Text == "(All)" ? "" : cmbExtension.Text;
            var category = cmbType.Text == "(All)" ? "" : cmbType.Text;
            var model = service.FindMultimedia(extension, category, txtCriteria.Text);

            _model = new ObservableCollection<MultimediaLinkViewModel>(model.Select((m) => {
                return new MultimediaLinkViewModel(m);
            }));

            lvw.ItemsSource = _model;
        }

        private void lvw_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = lvw.SelectedItem as MultimediaLinkViewModel;           
            DisplayMultimedia(selected);
        }

        private void DisplayMultimedia(MultimediaLinkViewModel selected) {

            if (selected != null) {
                JobExecutor.QueueJob(() => {
                    string filename = _tempFileManager.GetContentFileName(selected.MultimediaID, selected.Extension);
                    var image = GraphicsUtils.LoadImageFromFile(filename);
                    imgPreview.InvokeIfRequired(() => {
                        imgPreview.Stretch = Stretch.Uniform;
                        imgPreview.StretchDirection = StretchDirection.DownOnly;
                        imgPreview.Source = image;
                        gridInfo.DataContext = image;
                        FileInfo f = new FileInfo(filename);
                        lblImageInfo.Content = string.Format("{0}x{1}  {2} DPI  {3}", image.PixelWidth, image.PixelHeight, image.DpiX, ByteConverter.FormatBytes(f.Length));
                        lblFilename.Content = string.Format("Filename: {0}", filename);
                    });

                    
                });
            } else {
                imgPreview.Source = null;
                lblFilename.Content = "";
                lblImageInfo.Content = "";
            }
        }

        private void btnProperties_Click(object sender, RoutedEventArgs e) {
            var selected = lvw.SelectedItem as MultimediaLinkViewModel;
            ShowMultimediaProperties(selected);
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
                IBioLinkPlugin plugin = PluginManager.Instance.GetPluginByName("Tools");
                PluginManager.Instance.AddNonDockableContent(plugin, detailsControl, string.Format("Multimedia details [{0}]", model.MultimediaID), SizeToContent.Manual);
            }
        }

        private void btnLaunch_Click(object sender, RoutedEventArgs e) {
            ViewMultimedia(lvw.SelectedItem as MultimediaLinkViewModel);
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e) {
            SaveMultimedia(lvw.SelectedItem as MultimediaLinkViewModel);
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {
            AddNew();
        }

    }

    public class DeleteMultimedia : DatabaseAction {

        public DeleteMultimedia(int multimediaID) {
            this.MultimediaID = multimediaID;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteMultimedia(MultimediaID);
        }

        protected int MultimediaID { get; private set; }
    }
}
