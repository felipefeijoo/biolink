using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.Resources;
using System.Windows;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using BioLink.Data;

namespace BioLink.Client.Taxa {

    public class TaxaPlugin : BiolinkPluginBase {

        public TaxaPlugin(User user) : base(user) {
            Debug.Assert(user != null, "User is null!");
        }

        public override string Name {
            get { return "Taxa"; }
        }

        public override List<IWorkspaceContribution> Contributions {
            get {

                List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();

                contrib.Add(new MenuWorkspaceContribution(this, (obj, e) => { Logger.Debug("here in Foo!"); },                     
                    String.Format("{{'Name':'Taxa', 'Header':'{0}','InsertAfter':'File'}}", _R("Taxa.Menu.Taxa")), 
                    "{'Name':'Foo', 'Header':'_Foo!'}"
                ));

                contrib.Add(new MenuWorkspaceContribution(this, (obj, e) => { Logger.Debug("here in Bar!"); },
                    "Taxa",
                    "{'Name':'Bar', 'Header':'_Bar!', 'SeparatorBefore':true}"
                ));

                contrib.Add(new ExplorerWorkspaceContribution<TaxonExplorer>(this, new TaxonExplorer(), _R("TaxonExplorer.Title"), 
                    (explorer) => {
                        // Load the model on the background thread
                        ObservableCollection<Taxon> model = new ObservableCollection<Taxon>(new TaxaService(User).GetTopLevelTaxa());
                        // and set it on the the components own thread...
                        explorer.InvokeIfRequired(() => {
                            explorer.SetModel(model);
                        });
                    }
                    
                    ));

                return contrib;            
            }
        }
    }

}
