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
        private ControlHostWindow _queryTool;
        private ControlHostWindow _userManager;
        private ImportWizard _importWizard;
        private ImportWizard _importReferencesWizard;

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

            contrib.Add(new MenuWorkspaceContribution(this, "QueryTool", (obj, e) => { ShowQueryTool(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                String.Format("{{'Name':'QueryTool', 'Header':'{0}'}}", "Query Tool")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "UserManager", (obj, e) => { ShowUserManager(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                String.Format("{{'Name':'UserManager', 'Header':'{0}'}}", "Users and Groups...")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "Import", (obj, e) => { ShowImport(); },
                String.Format("{{'Name':'Tools', 'Header':'_Tools','InsertAfter':'UserManager'}}"),
                String.Format("{{'Name':'Import', 'Header':'Import'}}"), String.Format("{{'Name':'Import', 'Header':'_Taxa and Material records...'}}")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "Import", (obj, e) => { ShowImportReferences(); },
                String.Format("{{'Name':'Tools', 'Header':'_Tools','InsertAfter':'UserManager'}}"),
                String.Format("{{'Name':'Import', 'Header':'Import'}}"), String.Format("{{'Name':'ImportReferences', 'Header':'_References'}}")
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

            if (_importWizard != null) {
                _importWizard.Close();
                _importWizard = null;
            }

            if (_importReferencesWizard != null) {
                _importReferencesWizard.Close();
                _importWizard = null;
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

        private void ShowQueryTool() {
            if (_queryTool == null) {
                _queryTool = PluginManager.Instance.AddNonDockableContent(this, new QueryTool(User, this), "Query Tool", SizeToContent.Manual, true, (window) => {
                    window.btnOk.IsDefault = false;
                });
                _queryTool.Closed += new EventHandler((sender, e) => {
                    _queryTool = null;
                });
            }

            _queryTool.Show();
            _queryTool.Focus();
        }

        private void ShowUserManager() {
            if (_userManager == null) {
                _userManager = PluginManager.Instance.AddNonDockableContent(this, new UserManager(User, this), "Users and Groups", SizeToContent.Manual, true, (window) => {
                    window.btnOk.IsDefault = false;
                });
                _userManager.Closed += new EventHandler((sender, e) => {
                    _userManager = null;
                });
            }

            _userManager.Show();
            _userManager.Focus();
        }

        private void ShowImport() {
            if (_importWizard == null) {
                var context = new ImportWizardContext();

                Func<List<FieldDescriptor>> fieldSource = () => {
                    var service = new ImportService(User);
                    return service.GetImportFields();
                };

                Func<ImportProcessor> importProcessorFactory = () => {
                    return new MaterialImportProcessor();
                };

                _importWizard = new ImportWizard(User, "Import Data", context, new ImportFilterSelection(), new ImportMappingPage(fieldSource), new ImportPage(importProcessorFactory));

                _importWizard.Closed += new EventHandler((sender, e) => {
                    _importWizard = null;
                });
            }

            _importWizard.Show();
            _importWizard.Focus();
        }

        private void ShowImportReferences() {
            if (_importReferencesWizard == null) {
                var context = new ImportWizardContext();

                Func<List<FieldDescriptor>> fieldSource = () => {
                    var service = new ImportService(User);
                    return service.GetReferenceImportFields();
                };

                Func<ImportProcessor> importProcessorFactory = () => {
                    return new ImportReferencesProcessor();
                };

                _importReferencesWizard = new ImportWizard(User, "Import References", context, new ImportFilterSelection(), new ImportMappingPage(fieldSource), new ImportPage(importProcessorFactory));

                _importReferencesWizard.Closed += new EventHandler((sender, e) => {
                    _importReferencesWizard = null;
                });
            }

            _importReferencesWizard.Show();
            _importReferencesWizard.Focus();
        }


        public override List<Command> GetCommandsForSelected(List<ViewModelBase> selected) {

            if (selected == null || selected.Count == 0) {
                return null;
            }

            var obj = selected[0];

            if (obj is ReferenceViewModel) {
                var list = new List<Command>();
                list.Add(new Command("Edit", (vm) => {
                    var r = vm as ReferenceViewModel;
                    EditReference(r.RefID);
                }));

                return list;
            }

            if (obj is JournalViewModel) {
                var list = new List<Command>();
                list.Add(new Command("Edit", (vm) => {
                    var j = vm as JournalViewModel;
                    EditJournal(j.JournalID);
                }));

                return list;
            }
            return null;
        }

        public override bool CanSelect(Type t) {
            return typeof(ReferenceSearchResult).IsAssignableFrom(t) || typeof(Journal).IsAssignableFrom(t);
        }

        public override void Select(Type t, Action<SelectionResult> success) {
            if (typeof(ReferenceSearchResult).IsAssignableFrom(t)) {
                ShowReferenceManager();
                _refManager.BindSelectCallback(success);
            } else if (typeof(Journal).IsAssignableFrom(t)) {
                ShowJournalManager();
                _journalManager.BindSelectCallback(success);
            } else {
                throw new Exception("Unhandled Selection Type: " + t.Name);
            }

        }

        public override bool CanEditObjectType(LookupType type) {
            switch (type) {
                case LookupType.Reference:
                case LookupType.Journal:
                    return true;
            }
            return false;
        }

        public override ViewModelBase CreatePinnableViewModel(PinnableObject pinnable) {

            var service = new SupportService(User);
            switch (pinnable.LookupType) {
                case LookupType.Reference:
                    var refmodel = service.GetReference(pinnable.ObjectID);
                    if (refmodel != null) {
                        return new ReferenceViewModel(refmodel);
                    }
                    break;
                case LookupType.Journal:
                    var jmodel = service.GetJournal(pinnable.ObjectID);
                    if (jmodel != null) {
                        return new JournalViewModel(jmodel);
                    }
                    break;
            }

            return null;
        }

        public void EditReference(int refID) {
            var control = new ReferenceDetail(User, refID);
            PluginManager.Instance.AddNonDockableContent(this, control, string.Format("Reference Detail [{0}]", refID), SizeToContent.Manual);
        }

        public void AddNewReference() {
            var control = new ReferenceDetail(User, -1);
            PluginManager.Instance.AddNonDockableContent(this, control, "Reference Detail", SizeToContent.Manual);
        }

        public void EditJournal(int journalID) {
            var control = new JournalDetails(User, journalID);
            PluginManager.Instance.AddNonDockableContent(this, control, string.Format("Journal Detail [{0}]", journalID), SizeToContent.Manual);
        }

        public void AddNewJournal() {
            var control = new JournalDetails(User, -1);
            PluginManager.Instance.AddNonDockableContent(this, control, "Journal Detail", SizeToContent.Manual);
        }

        public override void EditObject(LookupType type, int objectID) {
            switch (type) {
                case LookupType.Reference:
                    EditReference(objectID);
                    break;
                case LookupType.Journal:
                    EditJournal(objectID);
                    break;
            }
        }
        
    }
}
