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

namespace BioLink.Client.Extensibility {

    public abstract class BiolinkPluginBase : IBioLinkPlugin {

        private ResourceDictionary _resourceDictionary;

        public User User { get; set; }
        public PluginManager PluginManager { get; set; }

        public BiolinkPluginBase(User user, PluginManager pluginManager ) {

            this.User = user;
            this.PluginManager = pluginManager;

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

        protected String _R(string key) {
            Object res = _resourceDictionary[key];
            if (res == null) {
                return key;
            } else {
                return res.ToString();
            }            
        }

        public ResourceDictionary ResourceDictionary {
            get { return _resourceDictionary; }
        }

        public abstract string Name { get; }

        public abstract List<IWorkspaceContribution> Contributions { get; }

        public virtual void Dispose() {            
        }

    }
}
