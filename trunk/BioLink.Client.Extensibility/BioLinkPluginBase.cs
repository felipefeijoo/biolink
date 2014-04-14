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
using System.Windows;
using System.Reflection;
using System.Xml;
using System.Xaml;
using System.IO;
using System.Windows.Resources;
using BioLink.Client.Utilities;
using BioLink.Data;
using System.Windows.Controls;

namespace BioLink.Client.Extensibility {

    public abstract class BiolinkPluginBase : IBioLinkPlugin {

        private ResourceDictionary _resourceDictionary;

        public User User { get; set; }
        public PluginManager PluginManager { get; set; }

        public BiolinkPluginBase() {
        }

        public virtual void InitializePlugin(User user, PluginManager pluginManager, Window parentWindow) {

            this.User = user;
            this.PluginManager = pluginManager;
            this.ParentWindow = parentWindow;

            string assemblyName = this.GetType().Assembly.GetName().Name;
            string packUri = String.Format("pack://application:,,,/{0};component/StringResources.xaml", assemblyName);
            Logger.Debug("Attempting resource discovery for {0} ({1})", assemblyName, packUri);

            try {
                Uri uri = new Uri(packUri, UriKind.Absolute);
                StreamResourceInfo info = Application.GetResourceStream(uri);
                if (info != null) {
                    _resourceDictionary = XamlServices.Load(info.Stream) as ResourceDictionary;
                    Logger.Debug("{0} resource keys loaded (Ok).", _resourceDictionary.Count);
                } else {
                    Logger.Debug("No resource stream found for assembly {0} - message keys will be used instead", assemblyName);
                    _resourceDictionary = new ResourceDictionary();
                }
            } catch (Exception ex) {
                Logger.Debug("Failed to read resources for {0} : {1}", assemblyName, ex.ToString());
                _resourceDictionary = new ResourceDictionary();
            }
        }

        protected String _R(string key, params object[] args) {
            Object res = _resourceDictionary[key];
            if (res == null) {
                return key;
            } else {
                if (args != null) {
                    return String.Format(res.ToString(), args);
                } else {
                    return res.ToString();
                }
            }
        }

        public ResourceDictionary ResourceDictionary {
            get { return _resourceDictionary; }
        }

        public string GetCaption(string key, params object[] args) {
            return _R(key, args);
        }

        public abstract string Name { get; }

        public abstract List<IWorkspaceContribution> GetContributions();

        public abstract bool RequestShutdown();

        public abstract List<Command> GetCommandsForSelected(List<ViewModelBase> selected);

        public virtual void Dispose() {
            Logger.Debug("Disposing {0}", GetType().Name);
            lock (_controlHosts) {
                foreach (ControlHostWindow chw in _controlHosts.Values) {
                    Logger.Debug("Closing singleton control host window: {0}", chw.Control.GetType().Name);
                    chw.Close();
                }
                _controlHosts.Clear();
            }
        }

        public virtual ViewModelBase CreatePinnableViewModel(PinnableObject pinnable) {
            return null;
        }

        public virtual List<Command> GetCommandsForObjectSet(List<int> objectIds, LookupType type) {
            return null;
        }

        public Window ParentWindow {
            get;
            set;
        }

        public virtual bool CanSelect<T>() {
            return false;
        }

        public virtual void Select<T>(LookupOptions lookupOptions, Action<SelectionResult> success, SelectOptions selectOptions) {
            throw new NotImplementedException();
        }

        public virtual void Dispatch(string command, Action<object> callback, params object[] args) {
        }

        public virtual bool CanEditObjectType(LookupType type) {
            return false;
        }

        public virtual void EditObject(LookupType type, int objectID) {
        }

        protected String BuildVersionString() {
            var version = this.GetType().Assembly.GetName().Version;
            return String.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        public PluginVersionInfo Version {
            get { return new PluginVersionInfo { Name = this.Name, Version = BuildVersionString() }; }
        }

        private Dictionary<string, ControlHostWindow> _controlHosts = new Dictionary<string, ControlHostWindow>();

        protected ControlHostWindow ShowSingleton(string title, Func<Control> controlFactoryFunction, SizeToContent sizeMode = SizeToContent.Manual, bool autoSavePosition = true, Action<ControlHostWindow> initFunc = null) {

            ControlHostWindow frm = null;
            lock (_controlHosts) {
                if (_controlHosts.ContainsKey(title)) {
                    frm = _controlHosts[title];
                } else {
                    var control = controlFactoryFunction();
                    if (control != null) {

                        frm = PluginManager.Instance.AddNonDockableContent(this, control, title, sizeMode, autoSavePosition, initFunc);
                        _controlHosts[title] = frm;
                        frm.Closed += new EventHandler((sender, e) => {
                            frm = null;
                            lock (_controlHosts) {
                                _controlHosts.Remove(title);
                            }
                        });

                        if (control is ILazyPopulateControl) {
                            var lazyLoadee = control as ILazyPopulateControl;
                            lazyLoadee.Populate();
                        }

                    }
                }
            }

            if (frm != null) {
                frm.Show();
                frm.Focus();
            }

            return frm;
        }

        public virtual T GetAdaptorForPinnable<T>(PinnableObject pinnable) {
            return default(T);
        }

    }
}
