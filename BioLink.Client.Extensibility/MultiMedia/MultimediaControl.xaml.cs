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

        public static BitmapFrame Resize(BitmapFrame photo, int width, int height, BitmapScalingMode scalingMode) {
            var group = new DrawingGroup();

            RenderOptions.SetBitmapScalingMode(group, scalingMode);
            group.Children.Add(new ImageDrawing(photo, new Rect(0, 0, width, height)));
            var targetVisual = new DrawingVisual();
            var targetContext = targetVisual.RenderOpen();
            targetContext.DrawDrawing(group);
            var target = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            targetContext.Close();
            target.Render(targetVisual);
            var targetFrame = BitmapFrame.Create(target);
            return targetFrame;
        }

        private void GenerateThumbnail(MultimediaLinkViewModel item, int maxDimension) {
            string filename = _tempFileManager.GetContentFileName(item.MultimediaID, item.Extension);
            this.InvokeIfRequired(() => {
                item.Thumbnail = GenerateThumbnail(filename, maxDimension);
            });
        }

        private BitmapSource GenerateThumbnail(string filename, int maxDimension) {
            if (!String.IsNullOrEmpty(filename)) {
                try {
                    using (var fs = new FileStream(filename, FileMode.Open)) {
                        var imageDecoder = BitmapDecoder.Create(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        var image = imageDecoder.Frames[0];

                        int height = maxDimension;
                        int width = maxDimension;

                        if (image.Height > image.Width) {
                            width = (int)(image.Width * ((double)maxDimension / image.Height));
                        } else {
                            height = (int)(image.Height * ((double)maxDimension / image.Width));
                        }

                        return Resize(image, width, height, BitmapScalingMode.HighQuality);
                    }
                } catch (Exception) {
                    FileInfo finfo = new FileInfo(filename);
                    return ExtractIconForExtension(finfo.Extension.Substring(1));
                }
            }

            return null;
        }

        public BitmapSource ExtractIconForExtension(string ext) {
            if (ext != null) {
                Icon icon = SystemUtils.GetIconFromExtension(ext);
                if (icon != null) {
                    return FormatImage(icon);
                }
            }
            return null;
        }

        private System.Windows.Media.Imaging.BitmapSource FormatImage(Bitmap bitmap) {

            // allocate the memory for the bitmap            
            IntPtr bmpPt = bitmap.GetHbitmap();

            // create the bitmapSource
            System.Windows.Media.Imaging.BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmpPt,
                IntPtr.Zero,
                Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            // freeze the bitmap to avoid hooking events to the bitmap
            bitmapSource.Freeze();

            // free memory
            SystemUtils.DeleteObject(bmpPt);

            return bitmapSource;
        }

        private System.Windows.Media.Imaging.BitmapSource FormatImage(Icon icon) {
            //Create bitmap
            var bmp = icon.ToBitmap();
            return FormatImage(bmp);
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
                
                ProcessStartInfo pinfo = new ProcessStartInfo(filename);
                if (pinfo != null && pinfo.Verbs.Length > 0) {

                    foreach (string v in pinfo.Verbs) {
                        string verb = v;

                        string caption = verb.Substring(0, 1).ToUpper() + verb.Substring(1);
                        menu.Items.Add(builder.New(caption).Handler(() => {
                            try {
                                pinfo.Verb = verb;
                                Process p = new Process();
                                p.StartInfo = pinfo;
                                p.Start();
                            } catch (Exception ex) {
                                ErrorMessage.Show(ex.Message);
                            }
                        }).MenuItem);
                    }
                } else {
                    menu.Items.Add(builder.New("Open").Handler(()=> {
                        try {
                            Process.Start(filename);
                        } catch (Exception ex) {
                            ErrorMessage.Show(ex.Message);
                        }

                    }).MenuItem);
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
                                viewModel.Thumbnail = GenerateThumbnail(filename, THUMB_SIZE);
                                _tempFileManager.CopyToTempFile(viewModel.MultimediaID, filename);
                                _model.Add(viewModel);
                                RegisterPendingChange(new InsertMultimediaAction(model, _tempFileManager.GetContentFileName(viewModel.MultimediaID, finfo.Extension.Substring(1))));
                                RegisterPendingChange(new InsertMultimediaLinkAction(model, CategoryType, Owner));
                                break;
                            case MultimediaDuplicateAction.UseExisting:
                                // Link to existing multimedia
                                model = new MultimediaLink();
                                model.MultimediaID = duplicate.MultimediaID;
                                model.MultimediaLinkID = -1;
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
                                model.MultimediaLinkID = -1;
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
            PluginManager.Instance.AddNonDockableContent(plugin, detailsControl, "Multimedia details", SizeToContent.Manual);
        }

        #region Properties

        public TraitCategoryType CategoryType { get; private set; }

        public ViewModelBase Owner { get; private set; }

        protected SupportService Service {
            get { return new SupportService(User); }
        }

        public bool IsPopulated { get; private set; }

        #endregion

    }

    public enum MultimediaDuplicateAction {
        NoDuplicate,
        Cancel,
        UseExisting,
        ReplaceExisting,
        InsertDuplicate
    }

}
