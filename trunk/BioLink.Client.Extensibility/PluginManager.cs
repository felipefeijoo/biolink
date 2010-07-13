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

namespace BioLink.Client.Extensibility {

    public class PluginManager {

        private Dictionary<string, IBioLinkPlugin> _plugins;

        public PluginManager() {
            _plugins = new Dictionary<string, IBioLinkPlugin>();
        }

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
                    IBioLinkPlugin plugin = Activator.CreateInstance(type) as IBioLinkPlugin;
                    // Allow the consumer to process this plugin...
                    if (pluginAction != null) {
                        pluginAction(plugin);
                    }

                    double percentComplete = ((double)++i / (double) pluginTypes.Count) * 100.0;
                    NotifyProgress(plugin.Name, percentComplete, ProgressEventType.Update);
                    _plugins.Add(plugin.Name, plugin);
                    DoEvents();
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

        private void ProcessAssembly(FileInfo assemblyFileInfo, List<Type> discovered) {

            try {
                Logger.Debug("Checking assembly: {0}", assemblyFileInfo.FullName);
                Assembly candidateAssembly = Assembly.LoadFrom(assemblyFileInfo.FullName);
                foreach (Type candidate in candidateAssembly.GetExportedTypes()) {
                    // Logger.Debug("testing type {0}", candidate.FullName);
                    if (candidate.GetInterface("IBioLinkPlugin") != null) {
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

    }

    public static class Extensions {

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action) {
            foreach (T item in enumeration) {
                action(item);
            }
        }

        public static void InvokeIfRequired(this DispatcherObject control, Action action) { 
            if (control.Dispatcher.Thread != Thread.CurrentThread) {
                control.Dispatcher.Invoke(action);
            } else {
                action();
            }
        }

    }

}
