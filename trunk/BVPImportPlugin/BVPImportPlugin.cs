using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.BVPImport {

    public class BVPImportPlugin : BiolinkPluginBase  {

        public const string BVPImport_PLUGIN_NAME = "BVPImporter";

        public override string Name {
            get { return "ALA Biodiversity Volunteer Portal Tools"; }
        }

        public override List<IWorkspaceContribution> GetContributions() {
            var contrib = new List<IWorkspaceContribution> {
                new MenuWorkspaceContribution(this, "BVPImport", (obj, e) => ShowBVPImport(),
                                                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                                                String.Format("{{'Name':'BVPTools', 'Header':'{0}','InsertAfter':'Import'}}", "ALA Volunteer Portal"),
                                                String.Format("{{'Name':'BVPImport', 'Header':'{0}'}}", "Import data from the BVP")
                )
            };

            return contrib;                        
        }

        private void ShowBVPImport() {
            ShowSingleton("BVP Import Utility", ()=> new BVPImporter(User));
        }

        public override bool RequestShutdown() {
            return true;
        }

        public override List<Command> GetCommandsForSelected(List<ViewModelBase> selected) {
            return null;
        }
    }

}
