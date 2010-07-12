using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Utilities;
using System.IO;
using System.Reflection;

namespace BioLink.Client.Extensibility {

    public class PluginManager {

        private Dictionary<string, IBioLinkPlugin> _plugins;

        public PluginManager() {
            _plugins = new Dictionary<string, IBioLinkPlugin>();
        }

        public void LoadPlugins(ProgressHandler progress, PluginAction pluginAction) {
            LoadPlugins(progress, pluginAction, ".", "./plugins");
        }

        public void LoadPlugins(ProgressHandler progress, PluginAction pluginAction, params string[] paths) {
            CodeTimer timer = new CodeTimer("Plugin loader");
            FileSystemTraverser t = new FileSystemTraverser();            
            SafeProgress(progress, "Searching for plugins...", -1, ProgressEventType.Start);

            List<Type> pluginTypes = new List<Type>();

            foreach (string path in paths) {                
                Debug.Log("LoadPlugins: Scanning: {0}", path);
                t.FilterFiles(path, fileinfo => { return fileinfo.Name.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase); }, fileinfo => { ProcessAssembly(fileinfo, progress, pluginTypes); }, false);
            }

            SafeProgress(progress, "Loading plugins...", 0, ProgressEventType.Start);
            int i = 0;
            foreach (Type type in pluginTypes) {
                Debug.Log("Instantiating type {0}", type.FullName);
                IBioLinkPlugin plugin = Activator.CreateInstance(type) as IBioLinkPlugin;
                // Allow the consumer to process this plugin...
                if (pluginAction != null) {
                    pluginAction(plugin);
                }

                int percentComplete = (++i / pluginTypes.Count) * 100;
                SafeProgress(progress, plugin.Name, percentComplete, ProgressEventType.Update);
                _plugins.Add(plugin.Name, plugin);
            }

            SafeProgress(progress, "Plugin loading complete", 100, ProgressEventType.End);
            timer.Stop();
        }

        private static bool SafeProgress(ProgressHandler handler, string format, params object[] args) {
            return SafeProgress(handler, String.Format(format, args), -1, ProgressEventType.Update);
        } 

        private static bool SafeProgress(ProgressHandler handler, string message, int percentComplete, ProgressEventType eventType) {
            if (handler != null) {
                return handler(message, percentComplete, eventType);
            }
            return true;
        }

        private void ProcessAssembly(FileInfo assemblyFileInfo, ProgressHandler progress, List<Type> discovered) {            

            try {                
                Debug.Log("Checking assembly: {0}", assemblyFileInfo.FullName);
                Assembly candidateAssembly = Assembly.LoadFrom(assemblyFileInfo.FullName);
                foreach (Type candidate in candidateAssembly.GetExportedTypes()) {
                    // Debug.Log("testing type {0}", candidate.FullName);
                    if (candidate.GetInterface("IBioLinkPlugin") != null) {
                        Debug.Log("Found plugin type: {0}", candidate.Name);
                        discovered.Add(candidate);
                    }
                }
            } catch (Exception ex) {
                Debug.Log(ex);
            }
        }

        public void Traverse(PluginAction action) {
            _plugins.ForEach(kvp => { action(kvp.Value); });
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

    }

}
