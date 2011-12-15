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
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Utility class for recursing through a file system. Can handle reparse points (hard/soft links)
    /// </summary>
    public class FileSystemTraverser {

        public delegate bool Filter(FileInfo file);
        public delegate void OnFile(FileInfo file);

        public void FilterFiles(string directory, Filter filter, OnFile matchHandler, bool recurseSubdirectories) {
            DirectoryUtils.OnFileHandler fh = (filedata, dir) => {
                var fileInfo = new FileInfo(dir + "\\" + filedata.cFileName);                
                if (filter(fileInfo)) {
                    matchHandler(fileInfo);
                }
            };            
            DirectoryUtils.TraverseDirectory(directory, fh, null, null, recurseSubdirectories);
        }
    }

    /// <summary>
    /// Utility functions used to traverse directories
    /// </summary>
    public class DirectoryUtils {

        public delegate void OnFileHandler(WIN32_FIND_DATA file, string directory);
        public delegate void OnDirectoryHandler(WIN32_FIND_DATA directory, string fullpath);
        public delegate bool CheckCancelHandler();

        #region "DllImports, Constants & Structs"

        public const int MaxPath = 260;
        public const int MaxAlternate = 14;

        [StructLayout(LayoutKind.Sequential)]
// ReSharper disable InconsistentNaming
        public struct FILETIME {
// ReSharper restore InconsistentNaming
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
// ReSharper disable InconsistentNaming
        public struct WIN32_FIND_DATA {
// ReSharper restore InconsistentNaming
            public FileAttributes dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxPath)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxAlternate)]
            public string cAlternate;
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

// ReSharper disable InconsistentNaming
        private const Int32 INVALID_HANDLE_VALUE = -1;
        private const Int32 OPEN_EXISTING = 3;
        private const Int32 FILE_FLAG_OPEN_REPARSE_POINT = 0x200000;
        private const Int32 FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;
        private const Int32 FSCTL_GET_REPARSE_POINT = 0x900A8;
// ReSharper restore InconsistentNaming

        /// <summary>

        /// If the path "REPARSE_GUID_DATA_BUFFER.SubstituteName" 

        /// begins with this prefix,

        /// it is not interpreted by the virtual file system.

        /// </summary>

        private const String NonInterpretedPathPrefix = "\\??\\";

        [StructLayout(LayoutKind.Sequential)]
// ReSharper disable InconsistentNaming
        private struct REPARSE_GUID_DATA_BUFFER {
// ReSharper restore InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBePrivate.Local
            public UInt32 ReparseTag;

            public UInt16 ReparseDataLength;
            public UInt16 Reserved;
            public UInt16 SubstituteNameOffset;
            public UInt16 SubstituteNameLength;
            public UInt16 PrintNameOffset;
            public UInt16 PrintNameLength;


            /// <summary>
            /// Contains the SubstituteName and the PrintName.
            /// The SubstituteName is the path of the target directory.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)]
            public byte[] PathBuffer;
// ReSharper restore FieldCanBeMadeReadOnly.Local
// ReSharper restore MemberCanBePrivate.Local

        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(String lpFileName,
                                                Int32 dwDesiredAccess,
                                                Int32 dwShareMode,
                                                IntPtr lpSecurityAttributes,
                                                Int32 dwCreationDisposition,
                                                Int32 dwFlagsAndAttributes,
                                                IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern Int32 CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern Int32 DeviceIoControl(IntPtr hDevice,
                                                        Int32 dwIoControlCode,
                                                        IntPtr lpInBuffer,
                                                        Int32 nInBufferSize,
                                                        IntPtr lpOutBuffer,
                                                        Int32 nOutBufferSize,
                                                        out Int32 lpBytesReturned,
                                                        IntPtr lpOverlapped);


        #endregion

        public static void RecurseDirectory(string directory, OnFileHandler onfile, OnDirectoryHandler ondir, CheckCancelHandler checkCancel) {
            TraverseDirectory(directory, onfile, ondir, checkCancel, true);
        }

        public static void TraverseDirectory(string directory, OnFileHandler onfile, OnDirectoryHandler ondir, CheckCancelHandler checkCancel, bool recurse) {
            var invalidHandleValue = new IntPtr(-1);
            WIN32_FIND_DATA findData;

            if (directory.StartsWith(".")) {
                directory = Environment.CurrentDirectory + directory.Substring(1);
            }

            directory = directory.Replace("/", "\\");

            if (directory.EndsWith("\\")) {
                directory = directory.Substring(0, directory.Length - 1);
            }

            IntPtr findHandle = FindFirstFile(@"\\?\" + directory + @"\*", out findData);

            if (findHandle != invalidHandleValue) {

                do {
                    // Check if we should stop!
                    if (checkCancel != null) {
                        if (checkCancel()) {
                            FindClose(findHandle);
                            break;
                        }
                    }

                    if ((findData.dwFileAttributes & FileAttributes.Directory) != 0) {
                        if (findData.cFileName != "." && findData.cFileName != "..") {
                            string subdirectory = directory + (directory.EndsWith(@"\") ? "" : @"\") + findData.cFileName;
                            if (!IsLink(subdirectory)) {
                                if (ondir != null) {
                                    ondir(findData, subdirectory);
                                }

                                if (recurse) {
                                    TraverseDirectory(subdirectory, onfile, ondir, checkCancel, true);
                                }
                            }
                        }
                    } else {
                        // File
                        if (onfile != null) {
                            onfile(findData, directory);
                        }
                        // size += (long)findData.nFileSizeLow + (long)findData.nFileSizeHigh * 4294967296;
                    }
                } while (FindNextFile(findHandle, out findData));

                FindClose(findHandle);
            }
        }

        public static bool IsLink(string directory) {
            String targetDir = null;
            // Open the directory link:
            IntPtr hFile = CreateFile(directory, 0, 0, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OPEN_REPARSE_POINT, IntPtr.Zero);
            if (hFile.ToInt32() != INVALID_HANDLE_VALUE) {
                // Allocate a buffer for the reparse point data:
                Int32 outBufferSize = Marshal.SizeOf(typeof(REPARSE_GUID_DATA_BUFFER));
                IntPtr outBuffer = Marshal.AllocHGlobal(outBufferSize);
                try {
                    // Read the reparse point data:
                    Int32 bytesReturned;
                    Int32 readOk = DeviceIoControl(hFile, FSCTL_GET_REPARSE_POINT, IntPtr.Zero, 0, outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero);
                    if (readOk != 0) {
                        // Get the target directory from the reparse 
                        // point data:
                        var rgdBuffer = (REPARSE_GUID_DATA_BUFFER)Marshal.PtrToStructure(outBuffer, typeof(REPARSE_GUID_DATA_BUFFER));
                        if (rgdBuffer.ReparseTag == 0xA0000003 || rgdBuffer.ReparseTag == 0xA000000C) {
                            return true;
                        }

                        targetDir = Encoding.Unicode.GetString(rgdBuffer.PathBuffer, rgdBuffer.SubstituteNameOffset, rgdBuffer.SubstituteNameLength);
                        if (targetDir.StartsWith(NonInterpretedPathPrefix)) {
                            targetDir = targetDir.Substring(NonInterpretedPathPrefix.Length);
                        }
                    }
                } catch (Exception) {
                } finally {

                    // Free the buffer for the reparse point data:
                    Marshal.FreeHGlobal(outBuffer);

                    // Close the directory link:
                    CloseHandle(hFile);
                }
            }

            return !string.IsNullOrEmpty(targetDir);

        }

        public static String GetTargetDir(DirectoryInfo directoryInfo) {
            String targetDir = "";

            // Is it a directory link?
            if ((directoryInfo.Attributes & FileAttributes.ReparsePoint) != 0) {
                // Open the directory link:
                IntPtr hFile = CreateFile(directoryInfo.FullName, 0, 0, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OPEN_REPARSE_POINT, IntPtr.Zero);
                if (hFile.ToInt32() != INVALID_HANDLE_VALUE) {
                    // Allocate a buffer for the reparse point data:
                    Int32 outBufferSize = Marshal.SizeOf(typeof(REPARSE_GUID_DATA_BUFFER));
                    IntPtr outBuffer = Marshal.AllocHGlobal(outBufferSize);

                    try {
                        // Read the reparse point data:
                        Int32 bytesReturned;
                        var readOk = DeviceIoControl(hFile, FSCTL_GET_REPARSE_POINT, IntPtr.Zero, 0, outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero);
                        if (readOk != 0) {
                            // Get the target directory from the reparse 
                            // point data:
                            var rgdBuffer = (REPARSE_GUID_DATA_BUFFER)Marshal.PtrToStructure(outBuffer, typeof(REPARSE_GUID_DATA_BUFFER));
                            targetDir = Encoding.Unicode.GetString(rgdBuffer.PathBuffer, rgdBuffer.SubstituteNameOffset, rgdBuffer.SubstituteNameLength);
                            if (targetDir.StartsWith(NonInterpretedPathPrefix)) {
                                targetDir = targetDir.Substring(NonInterpretedPathPrefix.Length);
                            }
                        }
                    } catch (Exception) {
                        // ignore
                    }

                    // Free the buffer for the reparse point data:
                    Marshal.FreeHGlobal(outBuffer);

                    // Close the directory link:
                    CloseHandle(hFile);
                }
            }


            return targetDir;
        }

    }

}

