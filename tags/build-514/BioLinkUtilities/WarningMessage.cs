using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BioLink.Client.Utilities {

    public class WarningMessage {

        public static bool Show(string message, params object[] args) {
            var result = MessageBox.Show(string.Format(message, args), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            Logger.Warn("Showing warning message(s): {0} - result = {1}", message, result);
            return result == MessageBoxResult.Yes;
        }

    }
}
