using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using System.Windows;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Tools {

    public class ToolsPlugin : BiolinkPluginBase {

        private ControlHostWindow _phraseManager;
        private ControlHostWindow _refManager;

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

            contrib.Add(new MenuWorkspaceContribution(this, "ReferenceManager", (obj, e) => { ShowReferenceManager(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                String.Format("{{'Name':'ReferenceManager', 'Header':'{0}'}}", _R("Tools.Menu.ReferenceManager"))
            ));

            return contrib;
        }

        public override bool RequestShutdown() {
            return true;
        }

        public override void Dispose() {
            base.Dispose();
            if (_phraseManager != null) {
                _phraseManager.Close();
                _phraseManager = null;
            }
        }

        private void ShowPhraseManager() {
            if (_phraseManager == null) {
                _phraseManager = PluginManager.Instance.AddNonDockableContent(this, new PhraseManager(User), "Phrases", SizeToContent.Manual);
                _phraseManager.Closed += new EventHandler((sender, e) => {                    
                    _phraseManager = null;
                });
            }

            _phraseManager.Show();

        }

        private void ShowReferenceManager() {
            if (_refManager == null) {
                _refManager = PluginManager.Instance.AddNonDockableContent(this, new ReferenceManager(User), "Reference Manager", SizeToContent.Manual);
            }

            _refManager.Closed +=new EventHandler((sender, e) => {
                _refManager = null;
            });

            _refManager.Show();
            _refManager.Focus();
        }


        public override List<Command> GetCommandsForObject(ViewModelBase obj) {
            return null;
        }

        public override bool CanSelect(Type t) {
            return typeof(ReferenceSearchResult).IsAssignableFrom(t);
        }

        public override void Select(Type t, Action<SelectionResult> success) {
            if (typeof(ReferenceSearchResult).IsAssignableFrom(t)) {
                ShowReferenceManager();                
            } else {
                throw new Exception("Unhandled Selection Type: " + t.Name);
            }

        }
    }
}
