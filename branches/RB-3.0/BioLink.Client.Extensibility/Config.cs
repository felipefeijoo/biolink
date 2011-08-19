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
using System.Data.SQLite;
using System.IO;
using Newtonsoft.Json;
using BioLink.Client.Utilities;
using BioLink.Data;
using System.Windows;

namespace BioLink.Client.Extensibility {

    /// <summary>
    /// Global Biolink application configuration settings store
    /// </summary>
    public static class Config {

        // Singleton instance of a configuration store to hold BioLink settings
        private static ConfigurationStore _instance;

        /// <summary>
        /// Static initialiser
        /// </summary>
        static Config() {
            try {
                // config is stored on a per user basis in the local app data folder
                string path = String.Format("{0}\\BioLink", Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData));
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
                string configFile = string.Format("{0}\\BioLink.cfg", path);
                _instance = new ConfigurationStore(configFile);
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }
        }

        public static T GetGlobal<T>(string key, T @default) {
            return _instance.Get<T>(key, @default);
        }

        public static void SetGlobal<T>(string key, T value) {
            _instance.Set<T>(key, value);
        }

        public static T GetUser<T>(User user, string key, T @default) {
            return _instance.Get<T>(UserKey(user, key), @default);
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

        public static ConfigurationStore Instance {
            get { return _instance; }
        }

        public static void SaveWindowPosition(User user, Window window) {
            Rect rect = new Rect(window.Left, window.Top, window.Width, window.Height);
            SetUser(user, WindowPositionKey(window), new Nullable<Rect>(rect));
        }

        private static string WindowPositionKey(Window w) {
            String name = w.Name;
            if (String.IsNullOrEmpty(name)) {
                name = w.GetType().Name;
            }
            return String.Format("Windows.{0}.Position", name);
        }

        public static void RestoreWindowPosition(User user, Window window) {
            Nullable<Rect> r = GetUser<Nullable<Rect>>(user, WindowPositionKey(window), null);
            if (r.HasValue) {
                window.Width = r.Value.Width;
                window.Height = r.Value.Height;
                window.Top = r.Value.Top;
                window.Left = r.Value.Left;
            }
        }

    }

}
