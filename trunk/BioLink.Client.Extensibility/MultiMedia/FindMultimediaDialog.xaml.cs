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
using System.Collections.ObjectModel;
using System.IO;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for FindMultimediaDialog.xaml
    /// </summary>
    public partial class FindMultimediaDialog : Window {

        private ObservableCollection<MultimediaLinkViewModel> _model;
        private KeyedObjectTempFileManager<int?> _tempFileManager;
        private List<string> _extensions;
        private List<string> _multimediaTypes;

        public FindMultimediaDialog(User user) {
            InitializeComponent();
            User = user;
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

            txtCriteria.KeyUp +=new KeyEventHandler(txtCriteria_KeyUp);

            this.Loaded += new RoutedEventHandler(FindMultimediaDialog_Loaded);
        }

        void FindMultimediaDialog_Loaded(object sender, RoutedEventArgs e) {
            txtCriteria.Focus();
        }

        protected User User { get; private set; }

        void txtCriteria_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                DoFind();
            }

            if (e.Key == Key.Down && lvw.Items.Count > 0) {
                lvw.Focus();
                lvw.SelectedIndex = 0;
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
                        lblImageInfo.Content = string.Format("{0}x{1}  {2} DPI  {3}", image.PixelWidth, image.PixelHeight, image.DpiX, ByteLengthConverter.FormatBytes(f.Length));
                    });


                });
            } else {
                imgPreview.Source = null;
                lblImageInfo.Content = "";
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        public MultimediaLinkViewModel SelectedMultimedia { get; private set; }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            SelectedMultimedia = lvw.SelectedItem as MultimediaLinkViewModel;
            if (SelectedMultimedia != null) {
                this.DialogResult = true;
                this.Close();
            }
        }

    }
}
