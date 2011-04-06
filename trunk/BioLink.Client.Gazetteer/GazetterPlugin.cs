using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using System.Windows;

namespace BioLink.Client.Gazetteer {

    public class GazetterPlugin : BiolinkPluginBase {

        private ExplorerWorkspaceContribution<Gazetteer> _gazetter;

        public const string GAZETTEER_PLUGIN_NAME = "Gazetteer";

        public GazetterPlugin() {
        }

        public override string Name {
            get { return GAZETTEER_PLUGIN_NAME; }
        }

        public override List<IWorkspaceContribution> GetContributions() {
            List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();
            contrib.Add(new MenuWorkspaceContribution(this, "ShowGazetteer", (obj, e) => { PluginManager.EnsureVisible(this, "Gazetteer"); },
                String.Format("{{'Name':'View', 'Header':'{0}','InsertAfter':'File'}}", _R("Gazetteer.Menu.View")),
                String.Format("{{'Name':'ShowGazetteer', 'Header':'{0}'}}", _R("Gazetteer.Menu.ShowExplorer"))
            ));

            _gazetter = new ExplorerWorkspaceContribution<Gazetteer>(this, "Gazetteer", new Gazetteer(this), _R("Gazetteer.Title"), (explorer) => {});

            contrib.Add(_gazetter);

            return contrib;            
        }

        public override bool RequestShutdown() {
            return true;
        }

        public override void Dispose() {
            if (_gazetter != null) {
                (_gazetter.Content as Gazetteer).Dispose();
            }
        }

        public override List<Command> GetCommandsForSelected(List<ViewModelBase> obj) {
            return null;
        }

        public override ViewModelBase CreatePinnableViewModel(PinnableObject pinnable) {
            var placeName = pinnable.GetState<PlaceName>();
            if (placeName != null) {
                return new PlaceNameViewModel(placeName);
            }

            return null;
        }

        public override bool CanSelect<T>() {
            return typeof(T).IsAssignableFrom(typeof(PlaceName));
        }

        public override void Select<T>(LookupOptions options, Action<SelectionResult> success) {
            PluginManager.EnsureVisible(this, "Gazetteer");
            var g = _gazetter.Content as Gazetteer;
            if (g != null) {
                g.BindSelectCallback(success);
            }
        }
    }
}
