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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for MultimediaControl.xaml
    /// </summary>
    public partial class MultimediaControl : DatabaseActionControl<SupportService> {

        private ObservableCollection<MultimediaItemViewModel> _model;

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
        }

        public void PopulateControl() {
            List<MultimediaItem> data = Service.GetMultimediaItems(TraitCategoryType.Taxon.ToString(), IntraCategoryID);
            JobExecutor.QueueJob(() => {
                _model = new ObservableCollection<MultimediaItemViewModel>(data.ConvertAll((item) => new MultimediaItemViewModel(item)));
                this.InvokeIfRequired(() => {
                    this.thumbList.ItemsSource = _model;
                });

                foreach (MultimediaItemViewModel item in _model) {
                    this.InvokeIfRequired(() => {
                        GenerateThumbnail(item);
                    });
                }
            });
            IsPopulated = true;
        }

        private void GenerateThumbnail(MultimediaItemViewModel item) {
            
            byte[] bytes = Service.GetMultimediaBytes(item.MultimediaID);
            if (bytes != null && bytes.Length > 0) {
                MemoryStream stream = new MemoryStream(bytes, false);
                BitmapImage src = new BitmapImage();

                src.BeginInit();
                src.DecodePixelWidth = 100;
                src.StreamSource = stream;
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();

                item.Thumbnail = src;
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
