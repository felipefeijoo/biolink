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

        public const string BVPImport_PLUGIN_NAME = "BVPTools";

        public override string Name {
            get { return "ALA Biodiversity Volunteer Portal Tools"; }
        }

        public override List<IWorkspaceContribution> GetContributions() {
            var contrib = new List<IWorkspaceContribution>();
            return contrib;                        
        }

        public override bool RequestShutdown() {
            return true;
        }

        public override List<Command> GetCommandsForSelected(List<ViewModelBase> selected) {
            return null;
        }
    }

}
