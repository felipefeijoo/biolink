using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BioLink.Client.Utilities {

    public static class InfoBox {

        public static void Show(string message, string caption, FrameworkElement owner) {
            Window owningWindow = owner == null ? null : owner.FindParentWindow();
            MessageBox.Show(owningWindow, message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
