using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace BioLink.Client.Utilities {

    public static class ReflectionUtils {

        public static void CopyProperties<T>(T source, T dest) {
            Type t = typeof(T);
            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in props) {
                if (prop.CanRead && prop.CanWrite) {
                    var value = prop.GetValue(source, null);
                    prop.SetValue(dest, value, null);
                }
            }
        }

        /// <summary>
        /// Perform a deep Copy of the object.        
        /// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(T source) {
            if (!typeof(T).IsSerializable) {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null)) {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream) {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

    }

}
