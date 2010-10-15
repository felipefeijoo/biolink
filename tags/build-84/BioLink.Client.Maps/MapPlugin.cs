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
        private ControlHostWindow _regionMap;

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
                _map = PluginManager.Instance.AddNonDockableContent(this, new MapControl(MapMode.Normal), "Map Tool", SizeToContent.Manual);
                _map.Closed += new EventHandler((sender, e) => {
                    _map = null;
                });
            }
            _map.Show();
            _map.Focus();
        }

        private void ShowRegionMap(Action<object> callback, List<string> selectedRegions) {
            if (_regionMap == null) {
                var map = new MapControl(MapMode.RegionSelect, callback);
                _regionMap = PluginManager.Instance.AddNonDockableContent(this, map, "Region Selection Tool", SizeToContent.Manual);
                _regionMap.Closed += new EventHandler((sender, e) => {
                    _regionMap = null;
                });                
            }
            (_regionMap.Control as MapControl).SelectRegions(selectedRegions);
            

            _regionMap.Show();
            _regionMap.Focus();
        }


        public override bool RequestShutdown() {
            return true;
        }


        public override List<Command> GetCommandsForObject(ViewModelBase obj) {
            return null;
        }

        public override void Dispatch(string command, Action<object> callback, params object[] args) {
            switch (command.ToLower()) {
                case "showregionmap":
                    ShowRegionMap(callback, args[0] as List<string>);
                    break;
                case "showmap":
                    ShowMap();
                    break;
                default:
                    break;
            }
        }
    }

}
