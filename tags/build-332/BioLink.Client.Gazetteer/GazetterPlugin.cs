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


        private GazetteerConverter _gazConverter;

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

            contrib.Add(new MenuWorkspaceContribution(this, "ShowEGazConverter", (obj, e) => { ShowEGazConverter(); },
                String.Format("{{'Name':'Tools', 'Header':'_Tools','InsertAfter':'File'}}"),
                String.Format("{{'Name':'eGaz', 'Header':'eGaz'}}"), String.Format("{{'Name':'ShowEGazConverter', 'Header':'Legacy eGaz file converter'}}")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "ShowCoordCalculator", (obj, e) => { ShowCoordCalculator(); },
                String.Format("{{'Name':'Tools', 'Header':'_Tools','InsertAfter':'File'}}"),
                String.Format("{{'Name':'eGaz', 'Header':'eGaz'}}"), String.Format("{{'Name':'ShowCoordCalculator', 'Header':'_Coordinate calculator'}}")
            ));


            _gazetter = new ExplorerWorkspaceContribution<Gazetteer>(this, "Gazetteer", new Gazetteer(this), _R("Gazetteer.Title"), (explorer) => {});

            contrib.Add(_gazetter);

            return contrib;            
        }

        private void ShowCoordCalculator() {
            ShowSingleton("Coordinate Calculator", () => { return new CoordinateCalculator(); });
        }

        private void ShowEGazConverter() {
            if (_gazConverter == null) {
                _gazConverter = new GazetteerConverter();
                _gazConverter.Owner = PluginManager.Instance.ParentWindow;
                _gazConverter.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                _gazConverter.Closed += new EventHandler((source,e) => {
                    _gazConverter = null;
                });
            }

            _gazConverter.Show();

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
