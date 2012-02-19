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
using System.IO;
using System.Windows.Resources;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class ResourceTempFileManager {

        private Dictionary<Uri, string> _tempFileMap = new Dictionary<Uri, string>();

        public string ProxyResource(Uri uri) {

            if (_tempFileMap.ContainsKey(uri)) {
                return _tempFileMap[uri];
            }

            StreamResourceInfo streamInfo = Application.GetResourceStream(uri);            
            if (streamInfo != null) {
                
                string strUri = uri.AbsolutePath;
                String tempFile = NewFilename(strUri.Substring(strUri.LastIndexOf(".")));
                FileInfo file = new FileInfo(tempFile);

                Logger.Debug("Creating temp file {0} from resource {1}", tempFile, uri.AbsolutePath);

                using(streamInfo.Stream) {
                    using(Stream dest = file.OpenWrite()) {
                        byte[] buffer = new byte[1024];
                        int bytes;
                        while((bytes = streamInfo.Stream.Read(buffer, 0, buffer.Length)) > 0) {
                            dest.Write(buffer, 0, bytes);
                        }
                    }
                }

                _tempFileMap[uri] = tempFile;
                return tempFile;
            }            
            return null;
        }

        private string NewFilename(String ext) {
            if (!ext.StartsWith(".")) {
                ext = "." + ext;
            }

            var dirName = System.IO.Path.GetTempPath() + "BiolinkTempFiles/";
            if (!Directory.Exists(dirName)) {
                Directory.CreateDirectory(dirName);
            }
            return dirName + Guid.NewGuid().ToString() + ext;
        }


        internal void CleanUp() {
            foreach (string path in _tempFileMap.Values) {
                try {
                    Logger.Debug("Deleting temp file {0}", path);
                    File.Delete(path);
                } catch (Exception ex) {
                    Logger.Debug("Failed to delete {0} - {1}", path, ex.Message);
                }
            }
        }
    }
}
