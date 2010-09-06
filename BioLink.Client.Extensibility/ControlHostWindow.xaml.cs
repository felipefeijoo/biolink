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
    public partial class ControlHostWindow : Window {

        #region DesignerConstructor
        public ControlHostWindow() {
            InitializeComponent();
        }
        #endregion

        public ControlHostWindow(UIElement element, SizeToContent sizeToContent) {
            InitializeComponent();
            this.Control = element;
            this.SizeToContent = sizeToContent;
            ControlHost.Children.Add(element);

            IClosable closable = element as IClosable;
            if (closable != null) {
                closable.PendingChangedRegistered +=new PendingChangedRegisteredHandler((source, action) => {
                    btnApply.IsEnabled = true;
                });

                closable.PendingChangesCommitted += new PendingChangesCommittedHandler((source) => {
                    btnApply.IsEnabled = false;
                });

            }
        }

        public UIElement Control { get; private set; }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Control != null && Control is IClosable) {
                var closable = Control as IClosable;
                if (!closable.RequestClose()) {
                    e.Cancel = true;
                }
            }
        }

        public bool RequestClose() {
            if (Control != null && Control is IClosable) {
                var closable = Control as IClosable;
                return closable.RequestClose();
            }
            return true;
        }

        public void ApplyChanges() {

            if (Control != null && Control is IClosable) {
                var closable = Control as IClosable;
                closable.ApplyChanges();
            }
        }

        public void Dispose() {
            if (Control != null && Control is IDisposable) {
                (Control as IDisposable).Dispose();
            }
        }

        public bool HasPendingChanges {
            get {
                if (Control is IClosable) {
                    return (Control as IClosable).HasPendingChanges;
                }
                return false;
            }
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            //IClosable closable = Control as IClosable;
            //if (closable != null) {
            //    if (!closable.RequestClose()) {
            //        return;
            //    }
            //}
            this.Close();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e) {
            IClosable closable = Control as IClosable;
            if (closable != null && closable.HasPendingChanges) {
                closable.ApplyChanges();
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            IClosable closable = Control as IClosable;
            if (closable != null && closable.HasPendingChanges) {
                closable.ApplyChanges();
            }
            this.Close();
        }

    }

}
