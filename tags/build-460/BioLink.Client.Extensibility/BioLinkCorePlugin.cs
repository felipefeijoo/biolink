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

namespace BioLink.Client.Extensibility {

    public class BioLinkCorePlugin : BiolinkPluginBase {

        public override string Name {
            get { return "BioLinkCorePlugin"; }
        }

        private ExplorerWorkspaceContribution<PinBoard> _pinboard;

        public override List<IWorkspaceContribution> GetContributions() {

            PluginManager.Instance.PluginsLoaded += new Action<Extensibility.PluginManager>(Instance_PluginsLoaded);
            List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();

            contrib.Add(new MenuWorkspaceContribution(this, "ShowPinBoardMenu", (obj, e) => { PluginManager.EnsureVisible(this, "PinBoard"); },
                String.Format("{{'Name':'View', 'Header':'{0}','InsertAfter':'File'}}", "_View"),
                String.Format("{{'Name':'ShowPinBoard', 'Header':'{0}'}}", "Show _Pin board")
            ));

            _pinboard = new ExplorerWorkspaceContribution<PinBoard>(this, "PinBoard", new PinBoard(this), "Pin board", (pb) => { });

            contrib.Add(_pinboard);

            return contrib;
        }

        void Instance_PluginsLoaded(PluginManager obj) {
            _pinboard.ContentControl.InitializePinBoard();
        }

        public override bool RequestShutdown() {
            _pinboard.ContentControl.PersistPinnedItems();
            return true;
        }

        internal void PinObject(PinnableObject pinnable) {
            this._pinboard.ContentControl.Pin(pinnable);
        }

        internal void RefreshPinBoard() {
            _pinboard.ContentControl.RefreshPinBoard();
        }

        public override List<Command> GetCommandsForSelected(List<ViewModelBase> selected) {
            return null;
        }

    }
}
