using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Material {

    public class MaterialPlugin : BiolinkPluginBase {

        private MaterialExplorer _explorer;

        public MaterialPlugin() {            
        }

        public override string Name {
            get { return "Material"; }
        }

        public override List<IWorkspaceContribution> GetContributions() {
            
            List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();

            contrib.Add(new MenuWorkspaceContribution(this, "ShowExplorer", (obj, e) => { PluginManager.EnsureVisible(this, "MaterialExplorer"); },
                String.Format("{{'Name':'View', 'Header':'{0}','InsertAfter':'File'}}", _R("Material.Menu.View")),
                String.Format("{{'Name':'ShowMaterialExplorer', 'Header':'{0}'}}", _R("Material.Menu.ShowExplorer"))
            ));


            _explorer = new MaterialExplorer();
            contrib.Add(new ExplorerWorkspaceContribution<MaterialExplorer>(this, "MaterialExplorer", _explorer, _R("MaterialExplorer.Title"), (explorer) => {
                // initializer
            }));

            return contrib;
        }

        public override bool RequestShutdown() {
            return true;
        }


        public override List<Command> GetCommandsForObject(ViewModelBase obj) {
            return null;
        }
    }
}
