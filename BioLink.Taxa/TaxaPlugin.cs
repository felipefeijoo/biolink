using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;

namespace BioLink.Client.Taxa {

    public class TaxaPlugin : IBioLinkPlugin {

        public string Name {
            get { return "Taxa"; }
        }

        public List<IWorkspaceContribution> Contributions {
            get {

                List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();

                contrib.Add(new WorkspaceMenuContribution((obj, e) => { Logger.Debug("here in Foo!"); },                     
                    "{'Name':'Taxa', 'Header':'_Taxa','InsertAfter':'File'}", 
                    "{'Name':'Foo', 'Header':'_Foo!'}"
                ));

                contrib.Add(new WorkspaceMenuContribution((obj, e) => { Logger.Debug("here in Bar!"); },
                    "Taxa",
                    "{'Name':'Bar', 'Header':'_Bar!', 'SeparatorBefore':true}"
                ));

                return contrib;            
            }
        }
    }

}
