﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using BioLink.Data;
using System.Windows.Controls;

namespace BioLink.Client.Extensibility {

    public class PluginManager : IDisposable {

        private static PluginManager _instance;

        private List<IBioLinkExtension> _extensions;
        private ResourceTempFileManager _resourceTempFiles = new ResourceTempFileManager();

        public static void Initialize(User user, Window parentWindow) {
            if (_instance != null) {
                _instance.Dispose();
            }
            _instance = new PluginManager(user, parentWindow);
        }

        public static PluginManager Instance {
            get {
                if (_instance == null) {
                    throw new Exception("The Plugin Manager has not been initialized!");
                }
                return _instance;
            }
        }

        private PluginManager(User user, Window parentWindow) {
            User = user;            
            _extensions = new List<IBioLinkExtension>();
            this.ParentWindow = parentWindow;
        }

        public ResourceTempFileManager ResourceTempFileManager { 
            get { return _resourceTempFiles; } 
        }

        public List<IBioLinkExtension> Extensions {
            get { return _extensions; }
        }

        public Window ParentWindow { get; private set; }

        public void LoadPlugins(PluginAction pluginAction) {
            LoadPlugins(pluginAction, ".|^BioLink[.].*[.]dll$", "./plugins");
        }

        public event ProgressHandler ProgressEvent;

        public void LoadPlugins(PluginAction pluginAction, params string[] paths) {
            using (new CodeTimer("Plugin loader")) {
                FileSystemTraverser t = new FileSystemTraverser();
                NotifyProgress("Searching for extensions...", -1, ProgressEventType.Start);

                List<Type> extensionTypes = new List<Type>();

                foreach (string pathelement in paths) {
                    string path = pathelement;
                    string filterexpr = ".*[.]dll$";
                    if (path.Contains("|")) {
                        path = pathelement.Substring(0, pathelement.IndexOf("|"));
                        filterexpr = pathelement.Substring(pathelement.IndexOf("|") + 1);
                    }

                    Regex regex = new Regex(filterexpr, RegexOptions.IgnoreCase);

                    FileSystemTraverser.Filter filter = (fileinfo) => {
                        return regex.IsMatch(fileinfo.Name);
                    };

                    Logger.Debug("LoadPlugins: Scanning: {0}", path);
                    t.FilterFiles(path, filter, fileinfo => { ProcessAssembly(fileinfo, extensionTypes); }, false);
                }

                NotifyProgress("Loading plugins...", 0, ProgressEventType.Start);
                int i = 0;

                foreach (Type type in extensionTypes) {
                    Logger.Debug("Instantiating type {0}", type.FullName);

                    IBioLinkExtension extension = InstantiateExtension(type);

                    if (extension != null) {
                        if (extension is IBioLinkPlugin) {

                            IBioLinkPlugin plugin = extension as IBioLinkPlugin;
                            Logger.Debug("Initializing Plugin {0}...", plugin.Name);
                            plugin.InitializePlugin(User, this, this.ParentWindow);

                            Logger.Debug("Integrating Plugin...", plugin.Name);
                            // Allow the consumer to process this plugin...
                            if (pluginAction != null) {
                                pluginAction(plugin);
                            }
                        }

                        _extensions.Add(extension);

                        double percentComplete = ((double)++i / (double)extensionTypes.Count) * 100.0;
                        NotifyProgress(extension.Name, percentComplete, ProgressEventType.Update);
                    } 
                    
                    DoEvents();

                }
                

                NotifyProgress("Plugin loading complete", 100, ProgressEventType.End);
            }
        }

        private IBioLinkExtension InstantiateExtension(Type type) {
            ConstructorInfo ctor = type.GetConstructor(new Type[] { });
            IBioLinkExtension extension = null;
            if (ctor != null) {
                extension = Activator.CreateInstance(type) as IBioLinkExtension;
            } else {
                throw new Exception(String.Format("Could not load extension {0} - no default constructor", type.FullName));
            }

            return extension;
        }

        public void AddDockableContent(IBioLinkPlugin plugin, FrameworkElement content, string title) {

            if (DockableContentAdded != null) {
                DockableContentAdded(plugin, content, title);
            }

        }

        public ControlHostWindow AddNonDockableContent(IBioLinkPlugin plugin, UIElement content, string title, bool autoSavePosition = true, Action<Window> initFunc = null) {

            ControlHostWindow form = new ControlHostWindow(content);
            form.Owner = ParentWindow;
            form.Title = title;
            form.Name = "HostFor_" + content.GetType().Name;

            form.Loaded += new RoutedEventHandler((source, e) => {
                Config.RestoreWindowPosition(User, form);
            });

            form.Closing += new System.ComponentModel.CancelEventHandler((source, e) => {
                Config.SaveWindowPosition(User, form);
            });
            
            if (initFunc != null) {
                initFunc(form);
            }
            form.Show();
            return form;
        }

        void form_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            throw new NotImplementedException();
        }

        private bool NotifyProgress(ProgressHandler handler, string format, params object[] args) {
            return NotifyProgress(String.Format(format, args), -1, ProgressEventType.Update);
        }

        private bool NotifyProgress(string message, double percentComplete, ProgressEventType eventType) {
            if (ProgressEvent != null) {
                return ProgressEvent(message, percentComplete, eventType);
            }
            return true;
        }

        public void EnsureVisible(IBioLinkPlugin plugin, string contentName) {
            if (RequestShowContent != null) {
                RequestShowContent(plugin, contentName);
            }
        }

        private void ProcessAssembly(FileInfo assemblyFileInfo, List<Type> discovered) {

            try {
                Logger.Debug("Checking assembly: {0}", assemblyFileInfo.FullName);
                Assembly candidateAssembly = Assembly.LoadFrom(assemblyFileInfo.FullName);
                foreach (Type candidate in candidateAssembly.GetExportedTypes()) {
                    // Logger.Debug("testing type {0}", candidate.FullName);
                    if (candidate.GetInterface("IBioLinkExtension") != null && !candidate.IsAbstract) {
                        Logger.Debug("Found extension type: {0}", candidate.Name);
                        discovered.Add(candidate);
                    }
                }
            } catch (Exception ex) {
                Logger.Debug(ex.ToString());
            }
        }

        public void TraversePlugins(PluginAction action) {
            _extensions.ForEach(ext => {
                if (ext is IBioLinkPlugin) {
                    action(ext as IBioLinkPlugin);
                }
            });
        }

        static void DoEvents() {
            DispatcherFrame frame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)delegate(object arg) {
                var f = arg as DispatcherFrame;
                f.Continue = false;
            },frame);
            Dispatcher.PushFrame(frame);
        }

        public void Dispose(Boolean disposing) {
            if (disposing) {
                Logger.Debug("Disposing the Plugin Manager");
                _extensions.ForEach((ext) => {
                    Logger.Debug("Disposing extension '{0}'", ext);
                    try {
                        ext.Dispose();
                    } catch (Exception ex) {
                        Logger.Warn("Exception occured whislt disposing plugin '{0}' : {1}", ext, ex);
                    }
                });
                Logger.Debug("Cleaning up temp files...");
                _resourceTempFiles.CleanUp();
            }
        }

        ~PluginManager() {
            Dispose(false);
        }
        
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public User User { get; private set; }

        internal List<IBioLinkPlugin> PlugIns {
            get {
                return GetExtensionsOfType<IBioLinkPlugin>();                
            }
        }

        public List<T> GetExtensionsOfType<T>() {
            return _extensions.FindAll((ext) => { return ext is T; }).ConvertAll((ext) => { return (T) ext; });
        }

        public bool RequestShutdown() {

            foreach (IBioLinkPlugin plugin in PlugIns) {
                if (!plugin.RequestShutdown()) {
                    return false;
                }
            }
            return true;
        }

        internal void CloseContent(FrameworkElement content) {
            if (DockableContentClosed != null) {
                DockableContentClosed(content);
            }
        }

        public event ShowDockableContributionDelegate RequestShowContent;

        public event AddDockableContentDelegate DockableContentAdded;

        public event CloseDockableContentDelegate DockableContentClosed;
        
        public delegate void PluginAction(IBioLinkPlugin plugin);

        public delegate void ShowDockableContributionDelegate(IBioLinkPlugin plugin, string name);

        public delegate void AddDockableContentDelegate(IBioLinkPlugin plugin, FrameworkElement content, string title);

        public delegate void CloseDockableContentDelegate(FrameworkElement content);

    }

}
