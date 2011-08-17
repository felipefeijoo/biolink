/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
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
                    string caption = string.Format("Permission Error [{0} {1}]", npex.PermissionCategory, npex.RequestedMask);
                    MessageBox.Show(txt, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                } else if (ex is System.ArgumentNullException && ex.Message == "Value cannot be null.\r\nParameter name: d") {
                    // ignore this for now... see: http://connect.microsoft.com/VisualStudio/feedback/details/561752/datagrid-crash-when-applying-scaletransform-to-datagridcell
                } else {
                    Logger.Debug("Unhandled Exception: {0}", ex.ToString());
                    MessageBox.Show(ex.ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);                    
                }
            } catch (Exception) {                
            }
        }
    }
}
