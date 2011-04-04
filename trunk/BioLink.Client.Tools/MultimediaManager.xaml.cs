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

namespace BioLink.Client.Tools {

    /// <summary>
    /// Interaction logic for MultimediaManager.xaml
    /// </summary>
    public partial class MultimediaManager : DatabaseActionControl {

        private List<string> _extensions;
        private List<string> _multimediaTypes;
        private KeyedObjectTempFileManager<int?> _tempFileManager;

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

        private void SaveMultimedia(MultimediaLinkViewModel selected) {
        }

        private void AddNew() {
        }

        private void DeleteMultimedia(MultimediaLinkViewModel selected) {
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

            var viewModel = model.Select((m) => {
                return new MultimediaLinkViewModel(m);
            });

            lvw.ItemsSource = viewModel;
        }

        private void lvw_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = lvw.SelectedItem as MultimediaLinkViewModel;           
            DisplayMultimedia(selected);
        }

        private void DisplayMultimedia(MultimediaLinkViewModel selected) {

            if (selected != null) {
                JobExecutor.QueueJob(() => {
                    string filename = _tempFileManager.GetContentFileName(selected.MultimediaID, selected.Extension);
                    imgPreview.InvokeIfRequired(() => {
                        imgPreview.Stretch = Stretch.Uniform;
                        imgPreview.StretchDirection = StretchDirection.DownOnly;
                        imgPreview.Source = GraphicsUtils.LoadImageFromFile(filename);
                    });
                });
            } else {
                imgPreview.Source = null;
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

    }
}
