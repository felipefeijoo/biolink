using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BioLink.Client.Utilities {

    public class ErrorMessage {

        public static void Show(string message, params object[] args) {
            MessageBox.Show(string.Format(message, args), "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            Logger.Warn("Showing error message: {0}", message);
        }
    }
}
