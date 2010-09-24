using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using System.Windows;

namespace BioLink.Client.Maps {

    public class MapPlugin : BiolinkPluginBase {

        private ControlHostWindow _map;

        public MapPlugin() {            
        }

        public override string Name {
            get { return "Map"; }
        }

        public override List<IWorkspaceContribution> GetContributions() {
            
            List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();

            contrib.Add(new MenuWorkspaceContribution(this, "ShowMap", (obj, e) => { ShowMap(); },
                "{'Name':'View', 'Header':'View','InsertAfter':'File'}",
                "{'Name':'ShowMap', 'Header':'Show _Map'}"
            ));

            return contrib;
        }

        private void ShowMap() {
            if (_map == null) {
                _map = PluginManager.Instance.AddNonDockableContent(this, new MapControl(), "Map Tool", SizeToContent.Manual);
                _map.Closed += new EventHandler((sender, e) => {
                    _map = null;
                });
            }
            _map.Show();
            _map.Focus();
        }

        public override bool RequestShutdown() {
            return true;
        }


        public override List<Command> GetCommandsForObject(ViewModelBase obj) {
            return null;
        }
    }
}
