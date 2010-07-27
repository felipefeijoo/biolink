using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;


namespace BioLink.Client.Utilities {

    public class GlobalExceptionHandler {

        public static void Handle(Exception ex) {
            try {
                MessageBox.Show(ex.ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Debug("Unhandled Exception: {0}", ex.ToString());
            } catch (Exception) {
                // ignore
            }
        }
    }
}
