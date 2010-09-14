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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for MultimediaControl.xaml
    /// </summary>
    public partial class MultimediaControl : DatabaseActionControl<SupportService> {

        private ObservableCollection<MultimediaItemViewModel> _model;

        private TempFileManager<int?> _tempFileManager;

        #region designer constructor
        public MultimediaControl() {
            InitializeComponent();
        }
        #endregion

        public MultimediaControl(User user, TraitCategoryType category, int? intraCatId)
            : base(new SupportService(user), "Multimedia:" + category.ToString() + ":" + intraCatId.Value) {

            this.User = user;
            this.CategoryType = category;
            this.IntraCategoryID = intraCatId.Value;
            InitializeComponent();

            txtMultimediaType.BindUser(user, PickListType.MultimediaType, null, TraitCategoryType.Taxon);

            _tempFileManager = new TempFileManager<int?>((mmId) => {
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
            var item = thumbList.SelectedItem as MultimediaItemViewModel;
            if (item != null) {
                detailGrid.DataContext = item;
                detailGrid.IsEnabled = true;
                txtCaption.Document.Blocks.Clear();
                if (!String.IsNullOrEmpty(item.Caption)) {
                    txtCaption.SelectAll();
                    txtCaption.Selection.Load(new MemoryStream(UTF8Encoding.Default.GetBytes(item.Caption)), DataFormats.Rtf);
                }
            }
        }

        public void PopulateControl() {
            List<MultimediaItem> data = Service.GetMultimediaItems(TraitCategoryType.Taxon.ToString(), IntraCategoryID);
            JobExecutor.QueueJob(() => {
                _model = new ObservableCollection<MultimediaItemViewModel>(data.ConvertAll((item) => new MultimediaItemViewModel(item)));
                this.InvokeIfRequired(() => {
                    this.thumbList.ItemsSource = _model;
                });

                foreach (MultimediaItemViewModel item in _model) {
                    this.BackgroundInvoke(() => {
                        GenerateThumbnail(item, 100);
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

        private void GenerateThumbnail(MultimediaItemViewModel item, int maxDimension) {

            string filename = _tempFileManager.GetContentFileName(item.MultimediaID, item.Extension);

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

                        item.Thumbnail = Resize(image, width, height, BitmapScalingMode.HighQuality);
                    }
                } catch (Exception) {
                    item.Thumbnail = ExtractIconForExtension(item.Extension);
                }
            }

        }

        public BitmapSource ExtractIconForExtension(string ext) {
            Icon icon = SystemUtils.GetIconFromExtension(ext);
            if (icon != null) {
                return FormatImage(icon);
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
            var item = thumbList.SelectedItem as MultimediaItemViewModel;
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
            var item = thumbList.SelectedItem as MultimediaItemViewModel;
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
                menu.Items.Add(builder.New("Delete").MenuItem);
            }
        }


        public bool IsPopulated { get; private set; }

        #region Properties

        public User User { get; private set; }

        public TraitCategoryType CategoryType { get; private set; }

        public int IntraCategoryID { get; private set; }

        #endregion

    }

    public class MultimediaItemViewModel : GenericViewModelBase<MultimediaItem> {

        private BitmapSource _thumb;

        public MultimediaItemViewModel(MultimediaItem model)
            : base(model) {
        }

        public int MultimediaID {
            get { return Model.MultimediaID; }
            set { SetProperty(() => Model.MultimediaID, value); }
        }

        public int MultimediaLinkID {
            get { return Model.MultimediaLinkID; }
            set { SetProperty(() => Model.MultimediaLinkID, value); }
        }

        public string MultimediaType {
            get { return Model.MultimediaType; }
            set { SetProperty(() => Model.MultimediaType, value); }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string Caption {
            get { return Model.Caption; }
            set { SetProperty(() => Model.Caption, value); }
        }

        public string Extension {
            get { return Model.Extension; }
            set { SetProperty(() => Model.Extension, value); }
        }

        public int SizeInBytes {
            get { return Model.SizeInBytes; }
            set { SetProperty(() => Model.SizeInBytes, value); }
        }

        public int Changes {
            get { return Model.Changes; }
            set { SetProperty(() => Model.Changes, value); }
        }

        public int BlobChanges {
            get { return Model.BlobChanges; }
            set { SetProperty(() => Model.BlobChanges, value); }
        }

        public BitmapSource Thumbnail {
            get { return _thumb; }
            set { SetProperty("ThumbNail", ref _thumb, value); }
        }

    }

}
