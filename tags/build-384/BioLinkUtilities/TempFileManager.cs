using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Global Generic Temp file manager that simply tracks the temp file names as they are issued, and attempts to delete them when disposed.
    /// </summary>
    public class TempFileManager : IDisposable {

        private static HashSet<string> _filenames = new HashSet<string>();

        static TempFileManager() {
        }

        public static string NewTempFilename(string extension, string prefix = "") {
            if (!extension.StartsWith(".")) {
                extension = "." + extension;
            }

            Func<string> generateUniqueName = () => {

                var dirName = System.IO.Path.GetTempPath() + "BiolinkTempFiles/";
                if (!Directory.Exists(dirName)) {
                    Directory.CreateDirectory(dirName);
                }

                var guid = Guid.NewGuid().ToString();
                return string.Format("{0}{1}-{2}{3}", dirName, prefix, guid.Substring(guid.LastIndexOf("-") + 1), extension);
            };
            string filename;
            while (File.Exists(filename = generateUniqueName())) {
            }

            _filenames.Add(filename);
            return filename;
        }

        public static bool Detach(string filename) {
            if (_filenames.Contains(filename)) {
                Logger.Debug("Detaching file from temp file manager: {0}", filename);
                _filenames.Remove(filename);
                return true;
            } else {
                Logger.Debug("Attempt to detach file from temp file manager failed: {0}", filename);
                return false;
            }
        }

        public static void CleanUp() {
            foreach (string path in _filenames) {
                try {
                    Logger.Debug("Deleting temp file {0}", path);
                    File.Delete(path);
                } catch (Exception ex) {
                    Logger.Debug("Failed to delete {0} - {1}", path, ex.Message);
                }
            }
            _filenames.Clear();
        }

        public void Dispose() {
            CleanUp();
        }


        public static void Attach(string projFilename) {
            _filenames.Add(projFilename);
        }
    }

    public class KeyedObjectTempFileManager<T> : IDisposable {

        private Dictionary<T, string> _tempFileMap = new Dictionary<T, string>();

        private Func<T, Stream> _contentGenerator;

        public KeyedObjectTempFileManager(Func<T, Stream> contentGenerator = null) {
            _contentGenerator = contentGenerator;
        }

        public void CopyToTempFile(T key, string filename) {
            if (File.Exists(filename)) {
                FileInfo finfo = new FileInfo(filename);
                var extension = finfo.Extension.Substring(1);
                var tempfile = NewFilename(extension);
                finfo.CopyTo(tempfile);
                _tempFileMap[key] = tempfile;
            }
        }

        public string GetContentFileName(T key, string extension) {
            if (_tempFileMap.ContainsKey(key)) {
                return _tempFileMap[key];
            }

            if (_contentGenerator == null) {
                return null;
            }

            if (extension == null) {
                extension = "";
            }

            String tempFile = NewFilename(extension);
            using (Stream contentStream = _contentGenerator(key)) {
                if (contentStream != null) {
                    FileInfo file = new FileInfo(tempFile);
                    Logger.Debug("Creating temp file {0} from stream (key={1})", tempFile, key);
                    using (Stream dest = file.OpenWrite()) {
                        byte[] buffer = new byte[4096];
                        int bytes;
                        while ((bytes = contentStream.Read(buffer, 0, buffer.Length)) > 0) {
                            dest.Write(buffer, 0, bytes);
                        }
                    }
                }
            }

            _tempFileMap[key] = tempFile;
            return tempFile;
        }

        private string NewFilename(String ext) {
            if (!ext.StartsWith(".")) {
                ext = "." + ext;
            }

            var dirName = System.IO.Path.GetTempPath() + "BiolinkTempFiles/";
            if (!Directory.Exists(dirName)) {
                Directory.CreateDirectory(dirName);
            }

            return string.Format("{0}{1}{2}", dirName, Guid.NewGuid().ToString(), ext);
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
            _tempFileMap.Clear();
        }

        public void Dispose() {
            CleanUp();
        }
    }

}
