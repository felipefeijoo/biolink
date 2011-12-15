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
using BioLink.Client.Extensibility;
using System.Windows;

namespace BioLink.Client.Maps {

    public class MapPlugin : BiolinkPluginBase, IRegionSelector, IMapProvider {

        private ControlHostWindow _map;
        private ControlHostWindow _regionMap;

        public override string Name {
            get { return "Map"; }
        }

        public override List<IWorkspaceContribution> GetContributions() {
            
            var contrib = new List<IWorkspaceContribution> {
                                                               new MenuWorkspaceContribution(this, "ShowMap", (obj, e) => ShowMap(),
                                                                                             "{'Name':'View', 'Header':'View','InsertAfter':'File'}",
                                                                                             "{'Name':'ShowMap', 'Header':'Show _Map'}"
                                                                   )
                                                           };

            return contrib;
        }

        private void ShowMap() {
            if (_map == null) {
                _map = PluginManager.Instance.AddNonDockableContent(this, new MapControl(MapMode.Normal), "Map Tool", SizeToContent.Manual);
                _map.Closed += (sender, e) => {
                                   _map = null;
                               };
            }
            _map.Show();
            _map.Focus();
        }

        public override bool RequestShutdown() {
            return true;
        }


        public override List<Command> GetCommandsForSelected(List<ViewModelBase> selected) {
            return null;
        }

        public void SelectRegions(List<RegionDescriptor> preselectedRegions, Action<List<RegionDescriptor>> updatefunc) {
            if (_regionMap == null) {
                var map = new MapControl(MapMode.RegionSelect, updatefunc);
                _regionMap = PluginManager.Instance.AddNonDockableContent(this, map, "Region Selection Tool", SizeToContent.Manual);
                _regionMap.Closed += (sender, e) => {
                                         _regionMap = null;
                                     };
            }
            var mapControl = _regionMap.Control as MapControl;
            if (mapControl != null) {
                mapControl.SelectRegions(preselectedRegions);
            }
            _regionMap.Show();
            _regionMap.Focus();
        }

        public void Show() {
            ShowMap();
        }

        public void DropAnchor(double longitude, double latitude, string caption) {
            if (_map != null && _map.IsVisible) {
                var mapControl = _map.Control as MapControl;
                if (mapControl != null) {
                    mapControl.DropDistanceAnchor(longitude, latitude, caption);
                }
            }
        }

        public void PlotPoints(MapPointSet points) {
            if (_map != null && _map.IsVisible) {
                var mapControl = _map.Control as MapControl;
                if (mapControl != null) {
                    mapControl.PlotPoints(points);
                }
            }
        }

        public void ClearPoints() {
            if (_map != null && _map.IsVisible) {
                var mapControl = _map.Control as MapControl;
                if (mapControl != null) {
                    mapControl.ClearPoints();
                }
            }
        }


        public void HideAnchor() {
            if (_map != null && _map.IsVisible) {
                var mapControl = _map.Control as MapControl;
                if (mapControl != null) {
                    mapControl.HideDistanceAnchor(true);
                }
            }
        }


        public void AddRasterLayer(string filename) {
            if (_map != null && _map.IsVisible) {
                var mapControl = _map.Control as MapControl;
                if (mapControl != null) {
                    mapControl.AddRasterLayer(filename);
                }
            }
        }

        public void RemoveRasterLayer(string filename) {
            if (_map != null && _map.IsVisible) {
                var mapControl = _map.Control as MapControl;
                if (mapControl != null) {
                    mapControl.RemoveRasterLayer(filename);
                }
            }
        }
    }

}
