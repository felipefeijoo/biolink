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

using System.Windows;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Simple Error dialog that can be invoke in the same way as MessageBox (in fact, delegates to MessageBox)
    /// </summary>
    public class ErrorMessage {

        public static void Show(string message, params object[] args) {
            MessageBox.Show(string.Format(message, args), "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            Logger.Warn("Showing error message: {0}", message);
        }

    }

}
