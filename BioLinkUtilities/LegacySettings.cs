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
using Microsoft.Win32;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Helper class for extracting saved settings from the registry of a previously installed legacy version of BioLink (v2.5 and below)
    /// </summary>
    public class LegacySettings {

        public static String GetLastProfile() {
            using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\CSIRO\\BioLink\\Client\\UserProfiles")) {
                if (key != null) {
                    var value = key.GetValue("LastUsedProfile") as String;

                    return value;
                }
            }
            return null;
        }

        public static T GetRegSetting<T>(String client, String section, String name, T defvalue) {
            String keyPath = String.Format("SOFTWARE\\CSIRO\\BioLink\\{0}\\{1}", client, section);
            RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key != null) {
                Object value = key.GetValue(name);
                return (T)value;
            }
            return defvalue;
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
