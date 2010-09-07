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
            List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();

            contrib.Add(new MenuWorkspaceContribution(this, "ShowPinBoardMenu", (obj, e) => { PluginManager.EnsureVisible(this, "PinBoard"); },
                String.Format("{{'Name':'View', 'Header':'{0}','InsertAfter':'File'}}", "_View"),
                String.Format("{{'Name':'ShowPinBoard', 'Header':'{0}'}}", "Show _Pin board")
            ));

            _pinboard = new ExplorerWorkspaceContribution<PinBoard>(this, "PinBoard", new PinBoard(this), "Pin board",
                (pb) => {
                    pb.InitializePinBoard();
                });

            contrib.Add(_pinboard);


            return contrib;
            
        }

        public override bool RequestShutdown() {
            _pinboard.ContentControl.PersistPinnedItems();
            return true;
        }

        internal void PinObject(IPinnable pinnable) {
            this._pinboard.ContentControl.Pin(pinnable);
        }
    }
}
