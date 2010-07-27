using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace BioLink.Client.Utilities {

    public class LegacySettings {

        public static String GetLastProfile() {

            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\CSIRO\\BioLink\\Client\\UserProfiles");

            String value = key.GetValue("LastUsedProfile") as String;

            return value;
        }

        public static T GetRegSetting<T>(String client, String section, String name, T defvalue) {
            String keyPath = String.Format("SOFTWARE\\CSIRO\\BioLink\\{0}\\{1}", client, section);
            RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key != null) {
                Object value = key.GetValue(name);
                return (T)value;
            } else {
                return defvalue;
            }
        }

        public static void TraverseSubKeys(string client, string section, RegistryKeyAction action) {
            if (action == null) {
                return;
            }
            String keyPath = String.Format("SOFTWARE\\CSIRO\\BioLink\\{0}\\{1}", client, section);
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath)) {
                if (key != null) {
                    foreach (string subkeyname in key.GetSubKeyNames()) {
                        RegistryKey subkey = key.OpenSubKey(subkeyname);
                        action(subkey);
                    }
                }
            }
        }

        public delegate void RegistryKeyAction(RegistryKey key);

    }
}
