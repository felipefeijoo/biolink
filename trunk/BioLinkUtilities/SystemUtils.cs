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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Microsoft.Win32;

namespace BioLink.Client.Utilities {
    /// <summary>
    /// Structure that encapsulates basic information of icon embedded in a file.
    /// </summary>
    public struct EmbeddedIconInfo {
        public string FileName;
        public int IconIndex;
    }

    /// <summary>
    /// A wrapper around a collection of Windows API calls and helpful lower level utilities
    /// </summary>
    public class SystemUtils {
        public const int KBYTES = 1024;
        public const int MBYTES = KBYTES*1024;
        public const int GBYTES = MBYTES*1024;
        private static readonly Regex ILLEGAL_FILENAME_CHARS_REGEX = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))));

        [DllImport("shell32.dll", EntryPoint = "ExtractIconA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr ExtractIcon(int hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static Icon GetIconFromExtension(string extension) {
            RegistryKey rkRoot = Registry.ClassesRoot;
            RegistryKey rkFileType = rkRoot.OpenSubKey("." + extension);
            try {
                if (rkFileType == null) {
                    return null;
                }

                object defaultValue = rkFileType.GetValue("");
                if (defaultValue == null) {
                    return null;
                }

                RegistryKey itemKey = null;
                var defValue = defaultValue as string;
                if (!string.IsNullOrEmpty(defValue)) {
                    itemKey = rkRoot.OpenSubKey(defValue);
                    if (itemKey == null) {
                        return null;
                    }
                }

                using (RegistryKey rkFileIcon = FindDefaultIconKey(itemKey)) {
                    if (rkFileIcon != null) {
                        object value = rkFileIcon.GetValue("");
                        if (value != null) {
                            string fileParam = value.ToString().Replace("\"", "");
                            return ExtractIconFromFile(fileParam);
                        }
                    }
                }
            } finally {
                rkRoot.Close();
                if (rkFileType != null) {
                    rkFileType.Close();
                }
            }
            return null;
        }

        protected static RegistryKey FindDefaultIconKey(RegistryKey itemKey) {
            RegistryKey key = itemKey.OpenSubKey("DefaultIcon");
            if (key != null) {
                return key;
            }

            key = itemKey.OpenSubKey("CurVer");
            if (key != null) {
                var value = key.GetValue("") as string;
                RegistryKey currentVersionKey = null;
                if (!string.IsNullOrEmpty((value))) {
                    currentVersionKey = Registry.ClassesRoot.OpenSubKey(value);
                }
                key.Close();
                if (currentVersionKey != null) {
                    return FindDefaultIconKey(currentVersionKey);
                }
            }

            return null;
        }

        public static Icon ExtractIconFromFile(string fileAndParam) {
            try {
                EmbeddedIconInfo embeddedIcon = GetEmbeddedIconInfo(fileAndParam);
                IntPtr lIcon = ExtractIcon(0, embeddedIcon.FileName, embeddedIcon.IconIndex);
                return Icon.FromHandle(lIcon);
            } catch (Exception) {
                return null;
                // throw exc;
            }
        }

        public static Icon ExtractIconFromFile(string fileAndParam, bool isLarge) {
            var hDummy = new[] {IntPtr.Zero};
            var hIconEx = new[] {IntPtr.Zero};

            try {
                EmbeddedIconInfo embeddedIcon = GetEmbeddedIconInfo(fileAndParam);

                uint readIconCount = isLarge ? ExtractIconEx(embeddedIcon.FileName, 0, hIconEx, hDummy, 1) : ExtractIconEx(embeddedIcon.FileName, 0, hDummy, hIconEx, 1);

                if (readIconCount > 0 && hIconEx[0] != IntPtr.Zero) {
                    // Get first icon.
                    var extractedIcon = (Icon) Icon.FromHandle(hIconEx[0]).Clone();
                    return extractedIcon;
                } else {
                    return null;
                }
            } catch (Exception exc) {
                throw new ApplicationException("Could not extract icon", exc);
            } finally {
                foreach (IntPtr ptr in hIconEx)
                    if (ptr != IntPtr.Zero) {
                        DestroyIcon(ptr);
                    }

                foreach (IntPtr ptr in hDummy)
                    if (ptr != IntPtr.Zero) {
                        DestroyIcon(ptr);
                    }
            }
        }

        protected static EmbeddedIconInfo GetEmbeddedIconInfo(string fileAndParam) {
            var embeddedIcon = new EmbeddedIconInfo();

            if (String.IsNullOrEmpty(fileAndParam)) {
                return embeddedIcon;
            }

            //Use to store the file contains icon.
            string fileName;

            //The index of the icon in the file.
            int iconIndex = 0;
            string iconIndexString = String.Empty;

            int commaIndex = fileAndParam.IndexOf(",");
            //if fileAndParam is some thing likes that: "C:\\Program Files\\NetMeeting\\conf.exe,1".
            if (commaIndex > 0) {
                fileName = fileAndParam.Substring(0, commaIndex);
                iconIndexString = fileAndParam.Substring(commaIndex + 1);
            } else {
                fileName = fileAndParam;
            }

            if (!String.IsNullOrEmpty(iconIndexString)) {
                //Get the index of icon.
                iconIndex = int.Parse(iconIndexString);
                if (iconIndex < 0) {
                    iconIndex = 0; //To avoid the invalid index.
                }
            }

            embeddedIcon.FileName = fileName;
            embeddedIcon.IconIndex = iconIndex;

            return embeddedIcon;
        }

        public static byte[] GetBytesFromFile(string filename) {
            Logger.Debug("Reading the bytes from file {0}", filename);

            var fileInfo = new FileInfo(filename);
            FileStream stream = fileInfo.OpenRead();
            var byteCount = (int) stream.Length;

            if (byteCount > 0) {
                var fileData = new byte[byteCount];
                stream.Read(fileData, 0, byteCount);
                stream.Close();
                Logger.Debug("Read {0} the bytes from {1}", byteCount, filename);
                return fileData;
            }
            Logger.Debug("No bytes read from {0}!", filename);
            return new byte[] {};
        }

        /// <summary>
        /// May potentially be able to improve what this does. For now just "start" it.
        /// </summary>
        /// <param name="filename"></param>
        public static void ShellExecute(string filename) {
            try {
                Process.Start(filename);
            } catch (Exception ex) {
                ErrorMessage.Show("Unable to launch file '{0}': {1}", filename, ex.Message);
            }
        }

        public static List<string> GetVerbs(string filename) {
            var verbs = new List<string>();
            var pinfo = new ProcessStartInfo(filename);
            if (pinfo.Verbs.Length > 0) {
                verbs.AddRange(pinfo.Verbs);
            }

            return verbs;
        }

        public static List<MenuItem> GetVerbsAsMenuItems(string filename) {
            var items = new List<MenuItem>();
            var pinfo = new ProcessStartInfo(filename);
            if (pinfo.Verbs.Length > 0) {
                foreach (string v in pinfo.Verbs) {
                    string verb = v;
                    string caption = "_" + verb.Substring(0, 1).ToUpper() + verb.Substring(1);
                    var item = new MenuItem {Header = caption};
                    item.Click += (s, e) => {
                                      try {
                                          pinfo.Verb = verb;
                                          var p = new Process {StartInfo = pinfo};
                                          p.Start();
                                      } catch (Exception ex) {
                                          ErrorMessage.Show(ex.Message);
                                      }
                                  };

                    items.Add(item);
                }
            } else {
                var item = new MenuItem {Header = "_Open"};
                item.Click += (s, e) => ShellExecute(filename);
                items.Add(item);
            }
            return items;
        }

        public static string ChangeExtension(string filename, string newExtension) {
            int index = filename.LastIndexOf(".");
            if (index > 0) {
                return string.Format("{0}.{1}", filename.Substring(0, index), newExtension);
            }

            return string.Format("{0}.{1}", filename, newExtension);
        }

        public static string StripIllegalFilenameChars(string filename) {
            return ILLEGAL_FILENAME_CHARS_REGEX.Replace(filename, "_");
        }

        public static string GetUserDataPath() {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            dir = Path.Combine(dir, "MySoftware");
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            return dir;
        }
    }
}