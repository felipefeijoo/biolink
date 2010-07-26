using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using Newtonsoft.Json;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Extensibility { 

    /// <summary>
    /// Global Biolink application preferences store
    /// </summary>
    public class Preferences {

        private static PreferenceStore _instance = new PreferenceStore("biolink.prefs");

        public static string GetPreference(string key) {
            return _instance.GetPreference(key, null);
        }

        public static T GetGlobal<T>(string key, T @default) {
            return _instance.Get<T>(key, @default);
        }

        public static void SetGlobal<T>(string key, T value) {
            _instance.Set<T>(key, value);
        }

        public static T GetUser<T>(User user, string key, T @default) {
            return _instance.Get<T>(UserKey(user,key), @default);
        }

        public static void SetUser<T>(User user, string key, T value) {
            _instance.Set<T>(UserKey(user, key), value);
        }

        public static T GetProfile<T>(User user, string key, T @default) {
            return _instance.Get<T>(ProfileKey(user, key), @default);
        }

        public static void SetProfile<T>(User user, string key, T value) {
            _instance.Set<T>(ProfileKey(user, key), value);
        }

        private static string UserKey(User user, string key) {
            string username = user.Username;
            if (String.IsNullOrEmpty(username)) {
                username = Environment.UserName;
            }
            return String.Format("USERKEY.{0}.{1}", username, key);
        }

        private static string ProfileKey(User user, string key) {
            string username = user.Username;
            if (String.IsNullOrEmpty(username)) {
                username = Environment.UserName;
            }
            return String.Format("USERPROFILEKEY.{0}.{1}.{2}", username, user.ConnectionProfile.Name, key);
        }

        public static PreferenceStore Instance {
            get { return _instance; }
        }

    }

    /// <summary>
    /// General preferences and settings class
    /// </summary>
    public class PreferenceStore : SQLiteServiceBase {

        // Default filename        
        private string _tableName = "Settings";
        private string _keyField = "Key";
        private string _valueField = "Value";

        /// <summary>
        /// Static initializer - establishes a preferences database if none exists
        /// </summary>
        public PreferenceStore(string filename) : base(filename) {
            
            if (!File.Exists(FileName)) {
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

            Logger.Debug("Setting preference: {0} = {1}", key, value);

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

            Logger.Debug("Getting preference: {0} = {1}", key, result);
            
            return result;
        }

        public T Get<T>(string key, T @default) {
            string str = GetPreference(key);
            if (str == null) {
                return @default;
            }

            return JsonConvert.DeserializeObject<T>(str);
        }

        public void Set<T>(string key, T value) {
            String str = JsonConvert.SerializeObject(value);
            SetPreference(key, str);
        }

        public void Traverse(PreferencesVisitor visitor) {
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

    public class UnhandledPreferenceTypeException : Exception {
        public UnhandledPreferenceTypeException(Type t) : base(String.Format("Preferences can't deal with type: {0}", t.FullName)) {
        }
    }

    public delegate void PreferencesVisitor(string key, string value);

}
