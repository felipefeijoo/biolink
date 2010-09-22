using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BioLink.Client.Utilities {

    public class TempFileManager<T> : IDisposable {

        private Dictionary<T, string> _tempFileMap = new Dictionary<T, string>();

        private Func<T, Stream> _contentGenerator;

        public TempFileManager(Func<T, Stream> contentGenerator) {
            _contentGenerator = contentGenerator;
        }

        public string GetContentFileName(T key, string extension) {
            if (_tempFileMap.ContainsKey(key)) {
                return _tempFileMap[key];
            }

            if (_contentGenerator == null) {
                return null;
            }

            String tempFile = NewFilename(extension);
            using (Stream contentStream = _contentGenerator(key)) {                
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
            
            _tempFileMap[key] = tempFile;
            return tempFile;            
        }

        private string NewFilename(String ext) {            
            if (!ext.StartsWith(".")) {
                ext = "." + ext;
            }
            return System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ext;
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
