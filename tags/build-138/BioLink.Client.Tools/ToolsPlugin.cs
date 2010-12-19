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
        private ControlHostWindow _journalManager;

        public const string TOOLS_PLUGIN_NAME = "Tools";

        public override string Name {
            get { return TOOLS_PLUGIN_NAME; }
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

            contrib.Add(new MenuWorkspaceContribution(this, "JournalManager", (obj, e) => { ShowJournalManager(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                String.Format("{{'Name':'JournalManager', 'Header':'{0}'}}", _R("Tools.Menu.JournalManager"))
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
            _phraseManager.Focus();
        }

        private void ShowReferenceManager() {
            if (_refManager == null) {
                _refManager = PluginManager.Instance.AddNonDockableContent(this, new ReferenceManager(User, this), "Reference Manager", SizeToContent.Manual,true,(window)=> {
                    window.btnOk.IsDefault = false;
                });
                _refManager.Closed += new EventHandler((sender, e) => {
                    _refManager = null;
                });
            }

            _refManager.Show();
            _refManager.Focus();
        }

        private void ShowJournalManager() {
            if (_journalManager == null) {
                _journalManager = PluginManager.Instance.AddNonDockableContent(this, new JournalManager(User, this), "Journal Manager", SizeToContent.Manual, true, (window) => {
                    window.btnOk.IsDefault = false;
                });
                _journalManager.Closed += new EventHandler((sender, e) => {
                    _journalManager = null;
                });
            }

            _journalManager.Show();
            _journalManager.Focus();

        }


        public override List<Command> GetCommandsForObject(ViewModelBase obj) {
            if (obj is ReferenceViewModel) {
                var list = new List<Command>();
                list.Add(new Command("Edit", (vm) => {
                    var r = vm as ReferenceViewModel;
                    EditReference(r.RefID);
                }));

                return list;
            }
            return null;
        }

        public override bool CanSelect(Type t) {
            return typeof(ReferenceSearchResult).IsAssignableFrom(t);
        }

        public override void Select(Type t, Action<SelectionResult> success) {
            if (typeof(ReferenceSearchResult).IsAssignableFrom(t)) {
                ShowReferenceManager();
                _refManager.BindSelectCallback(success);
            } else {
                throw new Exception("Unhandled Selection Type: " + t.Name);
            }

        }

        public override bool CanEditObjectType(LookupType type) {
            switch (type) {
                case LookupType.Reference:
                    return true;
            }
            return false;
        }

        public override ViewModelBase CreatePinnableViewModel(PinnableObject pinnable) {

            var service = new SupportService(User);
            switch (pinnable.LookupType) {
                case LookupType.Reference:
                    var model = service.GetReference(pinnable.ObjectID);
                    return new ReferenceViewModel(model);
            }

            return null;
        }

        public void EditReference(int refID) {
            var control = new ReferenceDetail(User, refID);
            PluginManager.Instance.AddNonDockableContent(this, control, "Reference Detail", SizeToContent.Manual);
        }

        public void AddNewReference() {
            var control = new ReferenceDetail(User, -1);
            PluginManager.Instance.AddNonDockableContent(this, control, "Reference Detail", SizeToContent.Manual);
        }

        public override void EditObject(LookupType type, int objectID) {
            switch (type) {
                case LookupType.Reference:
                    EditReference(objectID);
                    break;
            }
        }
        
    }
}
