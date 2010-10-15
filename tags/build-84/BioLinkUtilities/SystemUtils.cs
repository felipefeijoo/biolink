using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Drawing;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Structure that encapsulates basic information of icon embedded in a file.
    /// </summary>
    public struct EmbeddedIconInfo {
        public string FileName;
        public int IconIndex;
    }

    public class SystemUtils {

        [DllImport("shell32.dll", EntryPoint = "ExtractIconA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr ExtractIcon(int hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static extern int DestroyIcon(IntPtr hIcon);

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

                RegistryKey itemKey = rkRoot.OpenSubKey(defaultValue as string);

                if (itemKey == null) {
                    return null;
                }



                string defaultIcon = defaultValue.ToString() + "\\DefaultIcon";
                
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
                RegistryKey currentVersionKey = Registry.ClassesRoot.OpenSubKey(key.GetValue("") as string);
                key.Close();
                if (currentVersionKey != null) {
                    return FindDefaultIconKey(currentVersionKey);
                }
            }

            return null;
        }

        public static Icon ExtractIconFromFile(string fileAndParam) {
            try {
                EmbeddedIconInfo embeddedIcon = getEmbeddedIconInfo(fileAndParam);
                IntPtr lIcon = ExtractIcon(0, embeddedIcon.FileName, embeddedIcon.IconIndex);
                return Icon.FromHandle(lIcon);
            } catch (Exception exc) {
                throw exc;
            }
        }

        public static Icon ExtractIconFromFile(string fileAndParam, bool isLarge) {

            uint readIconCount = 0;
            IntPtr[] hDummy = new IntPtr[1] { IntPtr.Zero };
            IntPtr[] hIconEx = new IntPtr[1] { IntPtr.Zero };

            try {
                EmbeddedIconInfo embeddedIcon = getEmbeddedIconInfo(fileAndParam);

                if (isLarge) {
                    readIconCount = ExtractIconEx(embeddedIcon.FileName, 0, hIconEx, hDummy, 1);
                } else {
                    readIconCount = ExtractIconEx(embeddedIcon.FileName, 0, hDummy, hIconEx, 1);
                }

                if (readIconCount > 0 && hIconEx[0] != IntPtr.Zero) {
                    // Get first icon.
                    Icon extractedIcon = (Icon)Icon.FromHandle(hIconEx[0]).Clone();
                    return extractedIcon;
                } else {
                    return null;
                }
            } catch (Exception exc) {
                throw new ApplicationException("Could not extract icon", exc);
            } finally {
                foreach (IntPtr ptr in hIconEx)
                    if (ptr != IntPtr.Zero)
                        DestroyIcon(ptr);

                foreach (IntPtr ptr in hDummy)
                    if (ptr != IntPtr.Zero)
                        DestroyIcon(ptr);
            }
        }

        protected static EmbeddedIconInfo getEmbeddedIconInfo(string fileAndParam) {
            EmbeddedIconInfo embeddedIcon = new EmbeddedIconInfo();

            if (String.IsNullOrEmpty(fileAndParam)) {
                return embeddedIcon;
            }

            //Use to store the file contains icon.
            string fileName = String.Empty;

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
                if (iconIndex < 0)
                    iconIndex = 0;  //To avoid the invalid index.
            }

            embeddedIcon.FileName = fileName;
            embeddedIcon.IconIndex = iconIndex;

            return embeddedIcon;
        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

    }
}

