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
using System.IO;
using System.Data.SQLite;
using Newtonsoft.Json;

namespace BioLink.Data {
    /// <summary>
    /// Service for saving and retrieving configuration/preference style information
    /// </summary>
    public class ConfigurationStore : SQLiteServiceBase {

        // Default filename        
        private string _tableName = "Settings";
        private string _keyField = "Key";
        private string _valueField = "Value";

        /// <summary>
        /// Static initializer - establishes a preferences database if none exists
        /// </summary>
        public ConfigurationStore(string filename)
            : base(filename) {

            if (!File.Exists(FileName) || IsNew) {
                ResetPreferences();
            }
        }

        /// <summary>
        /// Recreates the preferences database
        /// </summary>
        public void ResetPreferences() {

            if (File.Exists(FileName)) {
                File.Delete(FileName);
            }

            try {
                SQLiteConnection.CreateFile(FileName);
                Command((cmd) => {
                    cmd.CommandText = String.Format("CREATE TABLE [{0}] ({1} TEXT PRIMARY KEY, {2} TEXT)", _tableName, _keyField, _valueField);
                    cmd.ExecuteNonQuery();
                });
            } catch (Exception ex) {
                // Clean up if we fail...
                if (File.Exists(FileName)) {
                    File.Delete(FileName);
                }
                throw ex;
            }
        }

        /// <summary>
        /// Sets (or replaces) a value for a particular preference key
        /// </summary>
        /// <param name="key">The preference key - should be unique</param>
        /// <param name="value">The value to set</param>
        public void SetPreference(string key, string value) {       

            Command((cmd) => {
                cmd.CommandText = String.Format(@"REPLACE INTO [{0}] VALUES (@key, @value)", _tableName);
                cmd.Parameters.Add(new SQLiteParameter("@key", key));
                cmd.Parameters.Add(new SQLiteParameter("@value", value));
                cmd.ExecuteNonQuery();
            });

        }

        /// <summary>
        /// Get a preference setting
        /// </summary>
        /// <param name="key">The preference key</param>
        /// <returns>The value for the preference, or null the key does not exist</returns>
        public String GetPreference(string key) {
            return GetPreference(key, null);
        }

        /// <summary>
        /// Retrieve a preference value
        /// </summary>
        /// <param name="key">the preference key</param>
        /// <param name="default">The default value if the preference key could not be found</param>
        /// <returns>The preference value, or the default value</returns>
        public String GetPreference(string key, string @default) {
            String result = @default;
            Command((cmd) => {
                cmd.CommandText = String.Format(@"SELECT [{0}] from Settings where [{1}] = @key", _valueField, _keyField);
                cmd.Parameters.Add(new SQLiteParameter("@key", key));
                String value = cmd.ExecuteScalar() as string;
                if (value != null) {
                    result = value;
                }
            });

            return result;
        }

        public T Get<T>(string key, T @default) {
            string str = GetPreference(key);
            if (str == null) {
                if (@default != null) {
                    Set(key, @default);
                }
                return @default;
            }

            return JsonConvert.DeserializeObject<T>(str);
        }

        public void Set<T>(string key, T value) {
            String str = JsonConvert.SerializeObject(value);
            SetPreference(key, str);
        }

        public void Traverse(ConfigurationItemAction visitor) {
            Command((cmd) => {
                cmd.CommandText = String.Format(@"SELECT [{0}],[{1}] from Settings;", _keyField, _valueField);
                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    if (reader.HasRows) {
                        while (reader.Read()) {
                            visitor(reader[_keyField] as string, reader[_valueField] as string);
                        }
                    }
                }
            });
        }

        private delegate object TypeParserDelegate(string s);

    }

    public delegate void ConfigurationItemAction(string key, string value);
}
