using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;

namespace BioLink.Taxa {

    public class TaxaPlugin : IBioLinkPlugin {

        public string Name {
            get { return "Taxa"; }
        }



        public List<IWorkspaceContribution> Contributions {
            get {
                List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();

                contrib.Add(new WorkspaceMenuContribution((obj, e) => { Debug.Log("here in Foo!"); },                     
                    "{'Name':'Taxa', 'Header':'_Taxa','InsertAfter':'File'}", 
                    "{'Name':'Foo', 'Header':'_Foo!'}"
                ));

                contrib.Add(new WorkspaceMenuContribution((obj, e) => { Debug.Log("here in Bar!"); },
                    "Taxa",
                    "{'Name':'Bar', 'Header':'_Bar!', 'SeparatorBefore':true}"
                ));

                return contrib;            
            }
        }
    }
}
