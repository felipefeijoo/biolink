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
using System.Windows.Controls.Primitives;

namespace BioLink.Client.Tools {

    /// <summary>
    /// Interaction logic for MultimediaManager.xaml
    /// </summary>
    public partial class MultimediaManager : DatabaseActionControl {

        private List<string> _extensions;
        private List<string> _multimediaTypes;
        private KeyedObjectTempFileManager<int?> _tempFileManager;
        private ObservableCollection<MultimediaLinkViewModel> _model;

        public MultimediaManager(ToolsPlugin plugin, User user) : base(user, "MultimediaManager") {
            InitializeComponent();
            this.Plugin = plugin;

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
                    if (bytes != null) {
                        return new MemoryStream(bytes);
                    }
                }
                return null;
            });

            txtCriteria.KeyUp += new KeyEventHandler(txtCriteria_KeyUp);
            lvw.MouseRightButtonUp += new MouseButtonEventHandler(lvw_MouseRightButtonUp);
            lvw.KeyUp += new KeyEventHandler(lvw_KeyUp);

            ListViewDragHelper.Bind(lvw, CreateDragData);
        }

        void lvw_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                DeleteSelected();
            }
        }

        private DataObject CreateDragData(ViewModelBase dragged) {

            var selected = dragged as MultimediaLinkViewModel;
            if (selected != null) {
                return new DataObject("MultimediaLink", selected.Model);
            }
            return null;
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

        private void DeleteMultimedia(System.Collections.IList multimedia) {            
            if (multimedia != null && multimedia.Count > 0) {
                if (multimedia.Count == 1) {
                    DeleteSingleMultimedia(multimedia[0] as MultimediaLinkViewModel);
                } else {
                    if (this.Question(string.Format("Are you sure you wish to permanently delete these {0} multimedia items?", multimedia.Count), "Delete multiple multimedia items?")) {
                        var candidateList = new List<MultimediaLinkViewModel>();

                        foreach (MultimediaLinkViewModel item in multimedia) {
                            candidateList.Add(item);
                        }

                        foreach (MultimediaLinkViewModel item in candidateList) {
                            _model.Remove(item);
                            RegisterUniquePendingChange(new DeleteMultimedia(item.MultimediaID));
                        }
                    }
                }
            }
        }


        private void DeleteSingleMultimedia(MultimediaLinkViewModel selected) {
            if (selected == null) {
                return;
            }

            if (this.Question(string.Format("Are you sure you wish to permanently delete '{0}'?", selected.Name), "Delete multimedia?")) {
                var index = _model.IndexOf(selected);
                _model.Remove(selected);
                if (index >= 0) {
                    _model[index].IsSelected = true;
                }

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

            if (selected != null) {
                builder.Separator();
                builder.New("Show linked items...").Handler(() => { ShowLinkedItems(selected); }).End();
            }

            builder.Separator();
            builder.New("_Add new...").Handler(() => { AddNew(); }).End();
            builder.New("_Delete").Handler(() => { DeleteSelected(); }).End();

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

            using (new OverrideCursor(Cursors.Wait)) {
                var service = new SupportService(User);
                var extension = cmbExtension.Text == "(All)" ? "" : cmbExtension.Text;
                var category = cmbType.Text == "(All)" ? "" : cmbType.Text;
                var model = service.FindMultimedia(extension, category, txtCriteria.Text);

                _model = new ObservableCollection<MultimediaLinkViewModel>(model.Select((m) => {
                    return new MultimediaLinkViewModel(m);
                }));

                lvw.ItemsSource = _model;
                lvw.UpdateLayout();
            }
        }

        private void lvw_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = lvw.SelectedItem as MultimediaLinkViewModel;
            DisplayMultimedia(selected);
        }

        private void DisplayMultimedia(MultimediaLinkViewModel selected) {

            if (selected != null) {
                JobExecutor.QueueJob(() => {
                    BitmapSource image = null;
                    try {
                        string filename = _tempFileManager.GetContentFileName(selected.MultimediaID, selected.Extension);
                        selected.TempFilename = filename;
                        image = GraphicsUtils.LoadImageFromFile(filename);
                        if (image != null) {
                            imgPreview.InvokeIfRequired(() => {
                                imgPreview.Stretch = Stretch.Uniform;
                                imgPreview.StretchDirection = StretchDirection.DownOnly;
                                imgPreview.Source = image;
                                gridInfo.DataContext = image;
                                FileInfo f = new FileInfo(filename);

                                if (f.Length != selected.SizeInBytes) {
                                    selected.SuspendChangeMonitoring = true;
                                    selected.SizeInBytes = (int) f.Length;
                                    selected.SuspendChangeMonitoring = false;
                                }

                                lblImageInfo.Content = string.Format("{0}x{1}  {2} DPI  {3}", image.PixelWidth, image.PixelHeight, image.DpiX, ByteConverter.FormatBytes(f.Length));
                                lblFilename.Content = string.Format("Filename: {0}", filename);
                            });
                        }
                    } finally {
                        if (image == null) {
                            imgPreview.InvokeIfRequired(() => {
                                imgPreview.Source = null;
                                lblImageInfo.Content = "No image";
                                lblFilename.Content = "";
                            });
                        }
                    }

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

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelected();
        }

        private void DeleteSelected() {
            DeleteMultimedia(lvw.SelectedItems);
        }

        private void btnLinks_Click(object sender, RoutedEventArgs e) {
            ShowLinkedItems(lvw.SelectedItem as MultimediaLinkViewModel);
        }

        public ToolsPlugin Plugin { get; private set; }

        private void ShowLinkedItems(MultimediaLinkViewModel selected) {
            if (selected != null) {
                selected.Icon = GraphicsUtils.GenerateThumbnail(selected.TempFilename, 48);
                PluginManager.Instance.AddNonDockableContent(Plugin, new LinkedMultimediaItemsControl(selected), "Items linked to multimedia " + selected.MultimediaID, SizeToContent.Manual);
            }
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
