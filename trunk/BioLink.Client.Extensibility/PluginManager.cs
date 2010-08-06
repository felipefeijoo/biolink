using System;
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

        private User _user;
        private Dictionary<string, IBioLinkPlugin> _plugins;

        public PluginManager(User user, Window parentWindow) {
            _user = user;
            _plugins = new Dictionary<string, IBioLinkPlugin>();
            this.ParentWindow = parentWindow;
        }

        public Window ParentWindow { get; private set; }

        public void LoadPlugins(PluginAction pluginAction) {
            LoadPlugins(pluginAction, ".|^BioLink[.].*[.]dll$", "./plugins");
        }

        public event ProgressHandler ProgressEvent;

        public void LoadPlugins( PluginAction pluginAction, params string[] paths) {
            using (new CodeTimer("Plugin loader")) {
                FileSystemTraverser t = new FileSystemTraverser();
                NotifyProgress("Searching for plugins...", -1, ProgressEventType.Start);

                List<Type> pluginTypes = new List<Type>();

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
                    t.FilterFiles(path, filter, fileinfo => { ProcessAssembly(fileinfo, pluginTypes); }, false);
                }

                NotifyProgress("Loading plugins...", 0, ProgressEventType.Start);
                int i = 0;
                foreach (Type type in pluginTypes) {
                    Logger.Debug("Instantiating type {0}", type.FullName);

                    ConstructorInfo ctor = type.GetConstructor(new Type[] { typeof(User), typeof(PluginManager) });
                    IBioLinkPlugin plugin = null;
                    if (ctor != null) {                        
                        plugin = ctor.Invoke(new Object[] { _user, this }) as IBioLinkPlugin;
                    } else {
                        plugin = Activator.CreateInstance(type) as IBioLinkPlugin;
                        plugin.User = _user;                        
                        plugin.PluginManager = this;
                    }
                     
                    if (plugin != null) {
                        plugin.ParentWindow = ParentWindow;

                        // Allow the consumer to process this plugin...
                        if (pluginAction != null) {
                            pluginAction(plugin);
                        }

                        double percentComplete = ((double)++i / (double)pluginTypes.Count) * 100.0;
                        NotifyProgress(plugin.Name, percentComplete, ProgressEventType.Update);
                        _plugins.Add(plugin.Name, plugin);
                        DoEvents();
                    }
                }
                

                NotifyProgress("Plugin loading complete", 100, ProgressEventType.End);
            }
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
                    if (candidate.GetInterface("IBioLinkPlugin") != null && !candidate.IsAbstract) {
                        Logger.Debug("Found plugin type: {0}", candidate.Name);
                        discovered.Add(candidate);
                    }
                }
            } catch (Exception ex) {
                Logger.Debug(ex.ToString());
            }
        }

        public void Traverse(PluginAction action) {
            _plugins.ForEach(kvp => { action(kvp.Value); });
        }

        static void DoEvents() {
            DispatcherFrame frame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)delegate(object arg) {
                var f = arg as DispatcherFrame;
                f.Continue = false;
            },frame);
            Dispatcher.PushFrame(frame);
        }

        delegate void PluginAggregator(Type pluginType);

        public delegate void PluginAction(IBioLinkPlugin plugin);

        public delegate void ShowDockableContributionDelegate(IBioLinkPlugin plugin, string name);

        public event ShowDockableContributionDelegate RequestShowContent;

        public void Dispose(Boolean disposing) {
            if (disposing) {
                Logger.Debug("Disposing the Plugin Manager");
                _plugins.ForEach((pluginkvp) => {
                    Logger.Debug("Disposing plugin '{0}'", pluginkvp.Key);
                    try {
                        pluginkvp.Value.Dispose();
                    } catch (Exception ex) {
                        Logger.Warn("Exception occured whislt disposing plugin '{0}' : {1}", pluginkvp.Key, ex);
                    }
                });
            }
        }

        ~PluginManager() {
            Dispose(false);
        }
        
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool RequestShutdown() {
            foreach (IBioLinkPlugin plugin in _plugins.Values) {
                if (!plugin.RequestShutdown()) {
                    return false;
                }
            }
            return true;
        }
    }

}
