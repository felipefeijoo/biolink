using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BioLink.Client.Extensibility {

    public interface IBioLinkPlugin {
        string Name { get; }
        List<IWorkspaceContribution> Contributions { get; }
    }

    public interface IWorkspaceContribution {
    }

    public class WorkspaceMenuContribution : IWorkspaceContribution {

        public MenuItemDescriptor[] Path { get; private set; }        
        public RoutedEventHandler Action { get; private set; }

        public WorkspaceMenuContribution(RoutedEventHandler action, MenuItemDescriptor[] path) {
            this.Action = action;
            this.Path = path;
        }

        public WorkspaceMenuContribution(RoutedEventHandler action, params string[] path) {            
            List<MenuItemDescriptor> items = new List<MenuItemDescriptor>();
            foreach (string pathdesc in path) {
                MenuItemDescriptor desc = null;
                if (pathdesc.StartsWith("{") && pathdesc.EndsWith("}")) {
                    desc = JsonConvert.DeserializeObject<MenuItemDescriptor>(pathdesc);
                } else {
                    desc = new MenuItemDescriptor();
                    desc.Name = pathdesc.StartsWith("_") ? pathdesc.Substring(1) : pathdesc;
                    desc.Header = pathdesc;
                }
                items.Add(desc);
            }            
            this.Action = action;
            this.Path = items.ToArray();
        }

        public String Name {
            get { return Path[Path.Length -1].Name; }
        }

    }

    public class MenuItemDescriptor {
        public string Name { get; set; }
        public string Header { get; set; }
        public string InsertBefore { get; set; }
        public string InsertAfter { get; set; }
        public bool SeparatorBefore { get; set; }
        public bool SeparatorAfter { get; set; }
    }


}
