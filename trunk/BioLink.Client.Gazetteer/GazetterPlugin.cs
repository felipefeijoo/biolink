/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
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

        private Gazetteer _gazetteer;

        private ExplorerWorkspaceContribution<Gazetteer> _gazetteerContrib;

        public const string GAZETTEER_PLUGIN_NAME = "Gazetteer";


        private GazetteerConverter _gazConverter;
        private FindNearestNamePlace _nearestNamePlace;
        private ControlHostWindow _gazWindow;

        public GazetterPlugin() {
        }

        public override string Name {
            get { return GAZETTEER_PLUGIN_NAME; }
        }

        public override List<IWorkspaceContribution> GetContributions() {
            List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();
            contrib.Add(new MenuWorkspaceContribution(this, "ShowGazetteer", (obj, e) => { ShowEGaz(); },
                String.Format("{{'Name':'View', 'Header':'{0}','InsertAfter':'File'}}", _R("Gazetteer.Menu.View")),
                String.Format("{{'Name':'ShowGazetteer', 'Header':'{0}'}}", _R("Gazetteer.Menu.ShowExplorer"))
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "ShowEGazConverter", (obj, e) => { ShowEGazConverter(); },
                String.Format("{{'Name':'Tools', 'Header':'_Tools','InsertAfter':'File'}}"),
                String.Format("{{'Name':'eGaz', 'Header':'eGaz'}}"), String.Format("{{'Name':'ShowEGazConverter', 'Header':'Legacy eGaz file converter'}}")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "ShowFindNearestPlace", (obj, e) => { ShowNearestNamedPlace(); },
                String.Format("{{'Name':'Tools', 'Header':'_Tools','InsertAfter':'File'}}"),
                String.Format("{{'Name':'eGaz', 'Header':'eGaz'}}"), String.Format("{{'Name':'ShowFindNearestPlace', 'Header':'_Find nearest named place'}}")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "ShowCoordCalculator", (obj, e) => { ShowCoordCalculator(); },
                String.Format("{{'Name':'Tools', 'Header':'_Tools','InsertAfter':'File'}}"),
                String.Format("{{'Name':'eGaz', 'Header':'eGaz'}}"), String.Format("{{'Name':'ShowCoordCalculator', 'Header':'_Coordinate calculator'}}")
            ));


            var floating = Config.GetUser(PluginManager.Instance.User, "Gazetteer.ShowEgazFloating", false);
            _gazetteer = new Gazetteer(this);
            if (!floating) {                
                _gazetteerContrib = new ExplorerWorkspaceContribution<Gazetteer>(this, "Gazetteer", _gazetteer, _R("Gazetteer.Title"), (explorer) => { });
                contrib.Add(_gazetteerContrib);
            }

            return contrib;            
        }

        private Gazetteer ShowEGaz() {

            var floating = Config.GetUser(PluginManager.Instance.User, "Gazetteer.ShowEgazFloating", false);

            if (floating) {
                if (_gazetteerContrib != null) {
                    ErrorMessage.Show("You will need to restart BioLink so that your preferences can be applied");
                    return null;
                }

                if (_gazWindow == null) {                    
                    _gazWindow = PluginManager.AddNonDockableContent(this, _gazetteer, _R("Gazetteer.Title"), SizeToContent.Manual, true);
                    _gazWindow.Owner = PluginManager.Instance.ParentWindow;
                    _gazWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    _gazWindow.Closed += new EventHandler((s, e) => {                        
                        _gazWindow = null;
                    });
                }

                _gazWindow.Show();
                _gazWindow.Focus();
                return _gazWindow.Control as Gazetteer;
            } else {
                if (_gazetteerContrib != null) {
                    PluginManager.EnsureVisible(this, "Gazetteer");
                    return _gazetteerContrib.ContentControl as Gazetteer;
                } else {
                    ErrorMessage.Show("You will need to restart BioLink so that your preferences can be applied");
                    return null;
                }
            }
        }

        private void ShowCoordCalculator() {
            ShowSingleton("Coordinate Calculator", () => { return new CoordinateCalculator(); });
        }

        public void ShowEGazConverter() {
            if (_gazConverter == null) {
                _gazConverter = new GazetteerConverter();
                _gazConverter.Owner = PluginManager.Instance.ParentWindow;
                _gazConverter.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                _gazConverter.Closed += new EventHandler((source,e) => {
                    _gazConverter.Content = null;
                    _gazConverter = null;
                    
                });
            }

            _gazConverter.Show();
        }

        public void ShowNearestNamedPlace() {
            if (_nearestNamePlace== null) {
                _nearestNamePlace = new FindNearestNamePlace(this);
                _nearestNamePlace.Owner = PluginManager.Instance.ParentWindow;
                _nearestNamePlace.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                _nearestNamePlace.Closed += new EventHandler((source, e) => {
                    _nearestNamePlace = null;
                });
            }
            _nearestNamePlace.Show();
        }

        public override bool RequestShutdown() {
            return true;
        }

        public override void Dispose() {
            if (_gazetteerContrib != null) {
                (_gazetteerContrib.Content as Gazetteer).Dispose();
            }

            if (_gazetteer != null) {
                _gazetteer.Dispose();
            }

            if (_gazWindow != null) {
                _gazWindow.Dispose();
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

        public GazetteerService CurrentGazetteer {
            get {

                if (_gazetteer!= null) {
                    return (_gazetteer).Service;
                }

                return null;
            }
        }

        public PlaceName CurrentSelectedPlace {
            get {
                if (_gazetteer != null) {
                    return _gazetteer.SelectedPlace;
                }

                return null;
            }
        }

        public override void Select<T>(LookupOptions options, Action<SelectionResult> success) {
            Gazetteer g = ShowEGaz();

            if (g != null) {
                g.BindSelectCallback(success);
            }
        }
    }
}
