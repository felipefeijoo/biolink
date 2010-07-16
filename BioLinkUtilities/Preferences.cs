﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using Newtonsoft.Json;

namespace BioLink.Client.Utilities { 

    /// <summary>
    /// Global Biolink application preferences store
    /// </summary>
    public class Preferences {

        private static PreferenceStore _instance = new PreferenceStore("biolink.prefs");

        public static string GetPreference(string key) {
            return _instance.GetPreference(key, null);
        }

        public static string GetPreference(string key, string @default) {
            return _instance.GetPreference(key, @default);
        }

        public static void SetPreference(string key, string value) {
            _instance.SetPreference(key, value);
        }

        public static T Get<T>(string key, T @default) {
            return _instance.Get<T>(key, @default);
        }

        public static void Set<T>(string key, T value) {
            _instance.Set<T>(key, value);
        }

        public static PreferenceStore Instance {
            get { return _instance; }
        }

    }

    /// <summary>
    /// General preferences and settings class
    /// </summary>
    public class PreferenceStore {

        private static Dictionary<Type, TypeParserDelegate> TYPE_MAP = new Dictionary<Type, TypeParserDelegate>();

        static PreferenceStore() {
            TYPE_MAP[typeof(string)] = (s) => { return s; };
            TYPE_MAP[typeof(Int64)] = (s) => { return Int64.Parse(s); };
            TYPE_MAP[typeof(Int32)] = (s) => { return Int32.Parse(s); };
            TYPE_MAP[typeof(Decimal)] = (s) => { return Decimal.Parse(s); };
            TYPE_MAP[typeof(Boolean)] = (s) => { return Boolean.Parse(s); };
            TYPE_MAP[typeof(DateTime)] = (s) => { return DateTime.Parse(s); };            
        }

        // Default filename
        private string _fileName;
        private string _tableName = "Settings";
        private string _keyField = "Key";
        private string _valueField = "Value";

        /// <summary>
        /// Static initializer - establishes a preferences database if none exists
        /// </summary>
        public PreferenceStore(string filename) {
            _fileName = filename;
            if (!File.Exists(_fileName)) {
                ResetPreferences();
            }
        }

        /// <summary>
        /// Recreates the preferences database
        /// </summary>
        public void ResetPreferences() {

            if (File.Exists(_fileName)) {
                File.Delete(_fileName);
            }

            try {
                SQLiteConnection.CreateFile(_fileName);
                Command((cmd) => {
                    cmd.CommandText = String.Format("CREATE TABLE [{0}] ({1} TEXT PRIMARY KEY, {2} TEXT)", _tableName, _keyField, _valueField);
                    cmd.ExecuteNonQuery();
                });
            } catch (Exception ex) {
                // Clean up if we fail...
                if (File.Exists(_fileName)) {
                    File.Delete(_fileName);
                }
                throw ex;
            }
        }

        private void Command(SqliteCommandDelegate action) {
            if (action == null) {
                return;
            }
            using (SQLiteConnection conn = getConnection()) {
                conn.Open();
                using (SQLiteCommand cmd = conn.CreateCommand()) {
                    action(cmd);                    
                }
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private SQLiteConnection getConnection() {
            SQLiteConnection conn = new SQLiteConnection(String.Format("Data Source={0}", _fileName));
            return conn;
        }

        private delegate object TypeParserDelegate(string s);
        private delegate void SqliteCommandDelegate(SQLiteCommand command);

    }    

    public class UnhandledPreferenceTypeException : Exception {
        public UnhandledPreferenceTypeException(Type t) : base(String.Format("Preferences can't deal with type: {0}", t.FullName)) {
        }
    }

    public delegate void PreferencesVisitor(string key, string value);

}
