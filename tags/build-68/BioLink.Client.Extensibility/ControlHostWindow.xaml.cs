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
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ControlHostWindow.xaml
    /// </summary>
    public partial class ControlHostWindow : ChangeContainer {

        #region DesignerConstructor
        public ControlHostWindow() {
            InitializeComponent();
        }
        #endregion

        public ControlHostWindow(User user, UIElement element, SizeToContent sizeToContent) : base(user) {           
            InitializeComponent();            
            this.Control = element;
            this.SizeToContent = sizeToContent;
            ControlHost.Children.Add(element);
            this.ChangeRegistered += new PendingChangedRegisteredHandler((source, a) => {
                btnApply.IsEnabled = true;
            });

            this.ChangesCommitted += new PendingChangesCommittedHandler((source) => {
                btnApply.IsEnabled = false;
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!RequestClose()) {
                e.Cancel = true;
            }
        }

        public bool RequestClose() {
            if (HasPendingChanges) {
                if (this.Question("You have unsaved changes. Are you sure you want to discard those changes?", "Discard changes?")) {
                    return true;
                } else {
                    return false;
                }
            }
            return true;
        }

        public void ApplyChanges() {

            if (HasPendingChanges) {
                CommitPendingChanges();
            }

        }

        public void Dispose() {
            if (Control != null && Control is IDisposable) {
                (Control as IDisposable).Dispose();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
            this.Close();
        }

        #region Properties

        public UIElement Control { get; private set; }

        #endregion

    }

}
