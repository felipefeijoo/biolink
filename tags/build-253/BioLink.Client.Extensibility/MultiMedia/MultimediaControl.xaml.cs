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
using BioLink.Data.Model;
using BioLink.Data;
using BioLink.Client.Utilities;
using System.Collections.ObjectModel;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using Microsoft.Win32;

namespace BioLink.Client.Extensibility {

    /// <summary>
    /// Interaction logic for MultimediaControl.xaml
    /// </summary>
    public partial class MultimediaControl : DatabaseActionControl, ILazyPopulateControl {

        private ObservableCollection<MultimediaLinkViewModel> _model;

        private KeyedObjectTempFileManager<int?> _tempFileManager;
        private const int THUMB_SIZE= 100;

        #region designer constructor
        public MultimediaControl() {
            InitializeComponent();
        }
        #endregion

        public MultimediaControl(User user, TraitCategoryType category, ViewModelBase owner) : base(user, "Multimedia:" + category.ToString() + ":" + owner.ObjectID.Value) {

            this.CategoryType = category;
            this.Owner = owner;
            InitializeComponent();

            txtMultimediaType.BindUser(user, PickListType.MultimediaType, null, TraitCategoryType.Multimedia);

            _tempFileManager = new KeyedObjectTempFileManager<int?>((mmId) => {
                if (mmId.HasValue) {
                    byte[] bytes = Service.GetMultimediaBytes(mmId.Value);
                    return new MemoryStream(bytes);
                }
                return null;
            });

            detailGrid.IsEnabled = false;

            thumbList.SelectionChanged += new SelectionChangedEventHandler(thumbList_SelectionChanged);
        }

        void thumbList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = thumbList.SelectedItem as MultimediaLinkViewModel;
            if (item != null) {
                detailGrid.DataContext = item;
                detailGrid.IsEnabled = true;
            }
        }

        public void Populate() {
            List<MultimediaLink> data = Service.GetMultimediaItems(CategoryType.ToString(), Owner.ObjectID.Value);
            JobExecutor.QueueJob(() => {                
                _model = new ObservableCollection<MultimediaLinkViewModel>(data.ConvertAll((item) => {
                    MultimediaLinkViewModel viewmodel = null;
                    this.InvokeIfRequired(() => {
                        viewmodel = new MultimediaLinkViewModel(item);
                        viewmodel.DataChanged += new DataChangedHandler((m) => {
                            RegisterUniquePendingChange(new UpdateMultimediaLinkAction(viewmodel.Model, CategoryType));
                        });
                    });
                    return viewmodel;
                }));
                this.InvokeIfRequired(() => {
                    this.thumbList.ItemsSource = _model;
                });

                foreach (MultimediaLinkViewModel item in _model) {
                    this.BackgroundInvoke(() => {
                        GenerateThumbnail(item, THUMB_SIZE);
                    });
                }
            });
            IsPopulated = true;
        }

        private void GenerateThumbnail(MultimediaLinkViewModel item, int maxDimension) {
            string filename = _tempFileManager.GetContentFileName(item.MultimediaID, item.Extension);
            this.InvokeIfRequired(() => {
                item.Thumbnail = GraphicsUtils.GenerateThumbnail(filename, maxDimension);
            });
        }

        public override void Dispose() {
            if (_tempFileManager != null) {
                _tempFileManager.Dispose();
                _tempFileManager = null;
            }
            base.Dispose();
        }

        private void thumbList_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            ShowContextMenu();
        }

        private void thumbList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            OpenSelected();
        }

        public void OpenSelected() {
            var item = thumbList.SelectedItem as MultimediaLinkViewModel;
            if (item != null) {
                string filename = _tempFileManager.GetContentFileName(item.MultimediaID, item.Extension);
                try {
                    Process.Start(filename);
                } catch (Exception ex) {
                    ErrorMessage.Show(ex.Message);
                }

            }
        }

        public void ShowContextMenu() {
            var item = thumbList.SelectedItem as MultimediaLinkViewModel;
            if (item != null) {
                ContextMenu menu = new ContextMenu();
                MenuItemBuilder builder = new MenuItemBuilder();                

                string filename = _tempFileManager.GetContentFileName(item.MultimediaID, item.Extension);

                thumbList.ContextMenu = menu;

                var verbMenuItems = SystemUtils.GetVerbsAsMenuItems(filename);
                foreach (MenuItem verbItem in verbMenuItems) {
                    menu.Items.Add(verbItem);
                }
                
                menu.Items.Add(new Separator());
                menu.Items.Add(builder.New("Add multimedia").Handler(() => { AddMultimedia(); }).MenuItem);
                menu.Items.Add(new Separator());
                menu.Items.Add(builder.New("Delete").Handler(() => { DeleteSelectedMultimedia(); }).MenuItem);
                menu.Items.Add(new Separator());
                menu.Items.Add(builder.New("Edit Details...").Handler(() => { ShowProperties(); }).MenuItem);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            AddMultimedia();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelectedMultimedia();
        }

        private void AddMultimedia() {
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
                                viewModel.Thumbnail = GraphicsUtils.GenerateThumbnail(filename, THUMB_SIZE);
                                _tempFileManager.CopyToTempFile(viewModel.MultimediaID, filename);
                                _model.Add(viewModel);
                                RegisterPendingChange(new InsertMultimediaAction(model, _tempFileManager.GetContentFileName(viewModel.MultimediaID, finfo.Extension.Substring(1))));
                                RegisterPendingChange(new InsertMultimediaLinkAction(model, CategoryType, Owner));
                                break;
                            case MultimediaDuplicateAction.UseExisting:
                                // Link to existing multimedia                                
                                model = new MultimediaLink();
                                model.MultimediaID = duplicate.MultimediaID;
                                model.MultimediaLinkID = NewLinkID();
                                model.Name = duplicate.Name;
                                model.Extension = duplicate.FileExtension;
                                viewModel = new MultimediaLinkViewModel(model);
                                GenerateThumbnail(viewModel, THUMB_SIZE);
                                _model.Add(viewModel);
                                RegisterPendingChange(new InsertMultimediaLinkAction(model, CategoryType, Owner));
                                break;
                            case MultimediaDuplicateAction.ReplaceExisting:
                                // register an update for the multimedia,
                                // and insert a new link
                                // Link to existing multimedia
                                model = new MultimediaLink();
                                model.MultimediaID = duplicate.MultimediaID;
                                model.MultimediaLinkID = NewLinkID();
                                model.Name = duplicate.Name;
                                model.Extension = duplicate.FileExtension;
                                viewModel = new MultimediaLinkViewModel(model);
                                GenerateThumbnail(viewModel, THUMB_SIZE);
                                _model.Add(viewModel);
                                _tempFileManager.CopyToTempFile(viewModel.MultimediaID, filename);
                                RegisterPendingChange(new UpdateMultimediaBytesAction(model, filename));
                                RegisterPendingChange(new InsertMultimediaLinkAction(model, CategoryType, Owner));
                                break;
                        }

                        if (viewModel != null) {
                            viewModel.IsSelected = true;
                            thumbList.SelectedItem = viewModel;
                        }
                    }
                }
            }
            
        }

        public MultimediaDuplicateAction CheckDuplicate(FileInfo file, out Multimedia duplicate) {
            int sizeInBytes = 0;
            duplicate = Service.FindDuplicateMultimedia(file, out sizeInBytes);
            if (duplicate != null) {
                var frm = new DuplicateItemOptions(duplicate, sizeInBytes);
                frm.Owner = this.FindParentWindow();
                if (frm.ShowDialog().ValueOrFalse()) {
                    return frm.SelectedAction;
                } else {
                    return MultimediaDuplicateAction.Cancel;
                }
            }
            return MultimediaDuplicateAction.NoDuplicate;
        }

        private int NewLinkID() {
            int newId = -1;
            foreach (MultimediaLinkViewModel model in _model) {
                if (model.MultimediaLinkID <= newId) {
                    newId = model.MultimediaID - 1;
                }
            }
            return newId;
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

        private void DeleteSelectedMultimedia() {
            var selected = this.thumbList.SelectedItem as MultimediaLinkViewModel;
            if (selected != null) {
                if (this.Question("Are you sure you wish to delete item '" + selected.Name + "'", "Delete item?")) {
                    if (selected.MultimediaLinkID >= 0) {
                        RegisterPendingChange(new DeleteMultimediaLinkAction(selected.Model));
                    } else {
                        ClearMatchingPendingChanges((action) => {
                            if (action is InsertMultimediaAction) {
                                var candidate = action as InsertMultimediaAction;
                                if (candidate.Model.MultimediaLinkID == selected.MultimediaLinkID) {
                                    return true;
                                }
                            } else if (action is InsertMultimediaLinkAction) {
                                var candidate = action as InsertMultimediaLinkAction;
                                if (candidate.Model.MultimediaLinkID == selected.MultimediaLinkID) {
                                    return true;
                                }
                            }
                            return false;
                        });
                    }
                    _model.Remove(selected);
                }
            }
        }

        private void btnProperties_Click(object sender, RoutedEventArgs e) {
            ShowProperties();
        }

        private void ShowProperties() {
            var selected = this.thumbList.SelectedItem as MultimediaLinkViewModel;
            if (selected == null) {
                return;
            }
            if (selected.MultimediaID < 0) {
                ErrorMessage.Show("You must first apply the changes before editing the details of this item!");
                return;
            }
            var model = Service.GetMultimedia(selected.MultimediaID);
            var detailsControl = new MultimediaDetails(model, User);
            IBioLinkPlugin plugin = PluginManager.Instance.GetPluginByName("Tools");
            PluginManager.Instance.AddNonDockableContent(plugin, detailsControl, string.Format("Multimedia details [{0}]", model.MultimediaID), SizeToContent.Manual);
        }

        #region Properties

        public TraitCategoryType CategoryType { get; private set; }

        public ViewModelBase Owner { get; private set; }

        protected SupportService Service {
            get { return new SupportService(User); }
        }

        public bool IsPopulated { get; private set; }

        #endregion

        private void thumbList_DragOver(object sender, DragEventArgs e) {
            var link = e.Data.GetData("MultimediaLink") as  MultimediaLink;
            if (link != null) {
                e.Effects = DragDropEffects.Link;
            }
        }

        private void thumbList_Drop(object sender, DragEventArgs e) {
            var link = e.Data.GetData("MultimediaLink") as  MultimediaLink;
            if (link != null) {
                AddNewLinkFromExternalLink(link);
            }
        }

        private void AddNewLinkFromExternalLink(MultimediaLink externalLink) {
            if (externalLink != null) {
                // Link to existing multimedia
                var model = new MultimediaLink();
                model.MultimediaID = externalLink.MultimediaID;
                model.MultimediaLinkID = NewLinkID();
                model.Name = externalLink.Name;
                model.Extension = externalLink.Extension;
                var viewModel = new MultimediaLinkViewModel(model);
                GenerateThumbnail(viewModel, THUMB_SIZE);
                _model.Add(viewModel);
                RegisterPendingChange(new InsertMultimediaLinkAction(model, CategoryType, Owner));
            }

        }

        private void btnLinkToExisting_Click(object sender, RoutedEventArgs e) {
            var frm = new FindMultimediaDialog(User);
            frm.Owner = this.FindParentWindow();
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (frm.ShowDialog() == true) {                
                AddNewLinkFromExternalLink(frm.SelectedMultimedia.Model);
            }
        }

    }

    public enum MultimediaDuplicateAction {
        NoDuplicate,
        Cancel,
        UseExisting,
        ReplaceExisting,
        InsertDuplicate
    }

}
