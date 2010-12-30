using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

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
    }
}
