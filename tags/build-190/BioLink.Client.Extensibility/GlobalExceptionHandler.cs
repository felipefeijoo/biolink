using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using BioLink.Client.Utilities;
using BioLink.Data;


namespace BioLink.Client.Extensibility {

    public class GlobalExceptionHandler {

        public static void Handle(Exception ex) {
            try {
                if (ex is NoPermissionException) {
                    var npex = ex as NoPermissionException;
                    string txt = ex.Message;
                    if (!string.IsNullOrEmpty(npex.DeniedMessage)) {
                        txt = npex.DeniedMessage;
                    }
                    string caption = string.Format("Permission Error [{0} {1}]", npex.RequestedPermission, npex.RequestedMask);
                    MessageBox.Show(txt, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                } else {
                    MessageBox.Show(ex.ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    Logger.Debug("Unhandled Exception: {0}", ex.ToString());
                }
            } catch (Exception) {
                // ignore
            }
        }
    }
}
