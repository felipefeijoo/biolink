using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using System.Windows;
using BioLink.Data;

namespace BioLink.Client.Tools {

    public class ToolsPlugin : BiolinkPluginBase {

        private ControlHostWindow _phraseManager;

        public override string Name {
            get { return "Tools"; }
        }

        public override void InitializePlugin(User user, PluginManager pluginManager, Window parentWindow) {
            base.InitializePlugin(user, pluginManager, parentWindow);
        }

        public override List<IWorkspaceContribution> GetContributions() {
            List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();
            contrib.Add(new MenuWorkspaceContribution(this, "Phrases", (obj, e) => { ShowPhraseManager(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                String.Format("{{'Name':'Phrases', 'Header':'{0}'}}", _R("Tools.Menu.Phrases"))
            ));
            return contrib;
        }

        public override bool RequestShutdown() {
            return true;
        }

        private void ShowPhraseManager() {
            if (_phraseManager == null) {

                _phraseManager = PluginManager.Instance.AddNonDockableContent(this, new PhraseManager(User), "Phrases", true, (window) => {

                });

                _phraseManager.Closed += new EventHandler((sender, e) => {
                    _phraseManager = null;
                });
            }

            _phraseManager.Show();

        }

    }
}
