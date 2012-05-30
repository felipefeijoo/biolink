/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using System.Windows;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using System.Windows.Controls;

namespace BioLink.Client.Tools {

    public class ToolsPlugin : BiolinkPluginBase {

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

            contrib.Add(new MenuWorkspaceContribution(this, "QueryTool", (obj, e) => { ShowQueryTool(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                String.Format("{{'Name':'QueryTool', 'Header':'{0}'}}", "Query Tool")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "Import", (obj, e) => { ShowImport(); },
                String.Format("{{'Name':'Tools', 'Header':'_Tools','InsertAfter':'UserManager'}}"),
                String.Format("{{'Name':'Import', 'Header':'Import'}}"), String.Format("{{'Name':'Import', 'Header':'_Taxa and Material records...'}}")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "Import", (obj, e) => { ShowImportReferences(); },
                String.Format("{{'Name':'Tools', 'Header':'_Tools','InsertAfter':'UserManager'}}"),
                String.Format("{{'Name':'Import', 'Header':'Import'}}"), String.Format("{{'Name':'ImportReferences', 'Header':'_References'}}")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "Modelling", (obj, e) => { ShowModelling(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                String.Format("{{'Name':'Modelling', 'Header':'{0}'}}", "_Modelling")
            ));

            // Reports...

            contrib.Add(new MenuWorkspaceContribution(this, "UserStatsReport", (obj, e) => { ShowUserStatsReport(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Reports', 'Header':'_Reports'}",
                "{'Name':'UserStatsReport', 'Header' : 'Data Entry _Statistics by User Report'}"                
            ));

            // Loans..
            contrib.Add(new MenuWorkspaceContribution(this, "LoanContacts", (obj, e) => { ShowLoanContacts(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Loans', 'Header':'_Loans'}",
                "{'Name':'LoanContacts', 'Header' : '_Contacts'}"
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "FindLoans", (obj, e) => { ShowFindLoans(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Loans', 'Header':'_Loans'}",
                "{'Name':'FindLoans', 'Header' : '_Find Loan'}"
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "Reminders", (obj, e) => { ShowLoanReminders(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Loans', 'Header':'_Loans'}",
                "{'Name':'Reminders', 'Header' : 'Reminders'}"
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "AddNewLoan", (obj, e) => { AddNewLoan(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Loans', 'Header':'_Loans'}",
                "{'Name':'AddNewLoan', 'Header' : '_Add new Loan'}"
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "ManageLoanForms", (obj, e) => { ShowLoanFormManager(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Loans', 'Header':'_Loans'}",
                "{'Name':'ManageLoanForms', 'Header' : '_Manage Loan Forms'}"
            ));

            // Label Manager

            contrib.Add(new MenuWorkspaceContribution(this, "LabelManager", (obj, e) => { ShowLabelManager(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'LabelManager', 'Header':'Label Manager'}"
            ));

            // Settings...

            contrib.Add(new MenuWorkspaceContribution(this, "Phrases", (obj, e) => { ShowPhraseManager(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Settings', 'Header':'_Settings'}",
                String.Format("{{'Name':'Phrases', 'Header':'{0}'}}", _R("Tools.Menu.Phrases"))
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "ReferenceManager", (obj, e) => { ShowReferenceManager(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Settings', 'Header':'_Settings'}",
                String.Format("{{'Name':'ReferenceManager', 'Header':'{0}'}}", _R("Tools.Menu.ReferenceManager"))
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "JournalManager", (obj, e) => { ShowJournalManager(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Settings', 'Header':'_Settings'}",
                String.Format("{{'Name':'JournalManager', 'Header':'{0}'}}", _R("Tools.Menu.JournalManager"))
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "UserManager", (obj, e) => { ShowUserManager(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Settings', 'Header':'_Settings'}",
                String.Format("{{'Name':'UserManager', 'Header':'{0}'}}", "Users and Groups...")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "MultimediaManager", (obj, e) => { ShowMultimediaManagerWindow(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Settings', 'Header':'_Settings'}",
                String.Format("{{'Name':'MultimediaManager', 'Header':'{0}'}}", "Multimedia Manager...")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "Administration", (obj, e) => { ShowAdminWindow(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Settings', 'Header':'_Settings'}",
                String.Format("{{'Name':'Administration', 'Header':'{0}'}}", "Administration...")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "HTMLManager", (obj, e) => { ShowHTMLManager(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Settings', 'Header':'_Settings'}",
                String.Format("{{'Name':'HTMLManager', 'Header':'{0}'}}", "Manage HTML (Welcome page)...")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "Preferences", (obj, e) => { ShowUserPreferences(); },
                String.Format("{{'Name':'Tools', 'Header':'{0}','InsertAfter':'View'}}", _R("Tools.Menu.Tools")),
                "{'Name':'Settings', 'Header':'_Settings'}",
                String.Format("{{'Name':'UserPreferences', 'Header':'{0}'}}", "Preferences...")
            ));

            return contrib;
        }

        public override bool RequestShutdown() {
            return true;
        }

        public override void Dispose() {
            base.Dispose();

            if (_importWizard != null) {
                _importWizard.Close();
                _importWizard = null;
            }

            if (_importReferencesWizard != null) {
                _importReferencesWizard.Close();
                _importWizard = null;
            }

        }

        private void ShowUserPreferences() {
            var frm = new AdvancedPreferences();
            frm.Owner = PluginManager.Instance.ParentWindow;
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            frm.ShowDialog();
        }

        public ControlHostWindow ShowLoanContacts() {
            return ShowSingleton("Contacts", () => new LoanContactsControl(User, this));
        }

        public ControlHostWindow ShowFindLoans() {
            return ShowSingleton("Find Loans", () => new LoanSearchControl(User, this));
        }

        public ControlHostWindow ShowLoanReminders() {
            return ShowSingleton("Reminders", () => new OverdueLoansControl(User, this));
        }

        public ControlHostWindow ShowLoanFormManager() {
            return ShowSingleton("Loan Forms", () => new SpecializedMultimediaManager(User, this, TraitCategoryType.Biolink, SupportService.BIOLINK_INTRA_CAT_ID));
        }

        public ControlHostWindow ShowHTMLManager() {
            return ShowSingleton("Welcome screen HTML", () => new SpecializedMultimediaManager(User, this, TraitCategoryType.Biolink, SupportService.BIOLINK_HTML_INTRA_CAT_ID));
        }

        public ControlHostWindow ShowLabelManager() {
            return ShowSingleton("Label Manager", () => new OneToManyControl(new LabelManagerControl(this, User)) { Margin = new Thickness(6) });
        }

        public void ShowPhraseManager() {
            ShowSingleton("Phrases", () => new PhraseManager(User));
        }

        public void ShowLoansForContact(int contactId) {
            var service = new LoanService(User);            
            var contact = service.GetContact(contactId);            

            if (contact != null) {
                var vm = new ContactViewModel(contact);
                var control = new LoansForContact(User, this, contactId);
                PluginManager.Instance.AddNonDockableContent(this, control, "Loans involving " + vm.FullName, SizeToContent.Manual);
            }

        }

        public ControlHostWindow ShowReferenceManager() {
            return ShowSingleton("Reference Manager", () => new ReferenceManager(User, this), SizeToContent.Manual, true,(window)=> {
                window.btnOk.IsDefault = false;
            });
        }

        public ControlHostWindow ShowJournalManager() {
            return ShowSingleton("Journal Manager", () => new JournalManager(User, this));
        }

        public void ShowQueryTool() {
            ShowSingleton("Query Tool", () => new QueryTool(User, this));
        }

        public void ShowModelling() {
            ShowSingleton("Predicted Distribution Modelling", () => new ModellingTool(User, this));
        }

        public void ShowUserManager() {
            ShowSingleton("Users and Groups", () => new UserManager(User, this));
        }

        public void ShowAdminWindow() {
            ShowSingleton("Administration", () => new AdministrationControl(User));
        }

        public void ShowMultimediaManagerWindow() {
            ShowSingleton("Multimedia Manager", () => new MultimediaManager(this, User));
        }

        public void ShowImport() {

            User.CheckPermission(PermissionCategory.IMPORT_MATERIAL, PERMISSION_MASK.ALLOW, "You do not have sufficient privileges to import into BioLink!");

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

        public void ShowImportReferences() {

            User.CheckPermission(PermissionCategory.IMPORT_REFERENCES, PERMISSION_MASK.ALLOW, "You do not have sufficient privileges to import references into BioLink!");

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

        public void ShowUserStatsReport() {
            PluginManager.Instance.RunReport(this, new UserStatsReport(User));
        }

        public override List<Command> GetCommandsForSelected(List<ViewModelBase> selected) {

            if (selected == null || selected.Count == 0) {
                return null;
            }

            var obj = selected[0];
            var list = new List<Command>();

            if (obj is ReferenceViewModel) {
                
                list.Add(new Command("Edit details...", (vm) => {
                    var r = vm as ReferenceViewModel;
                    EditReference(r.RefID);
                }));
            }

            if (obj is JournalViewModel) {
                list.Add(new Command("Edit details...", (vm) => {
                    var j = vm as JournalViewModel;
                    EditJournal(j.JournalID);
                }));
            }

            if (obj is ContactViewModel) {
                var c = obj as ContactViewModel;

                list.Add(new Command("Edit details...", (vm) => {
                    EditContact(c.ContactID);
                }));

                list.Add(new Command("Show Loans ...", (vm) => {
                    ShowLoansForContact(c.ContactID);
                }));

            }

            if (obj is LoanViewModel) {
                var loan = obj as LoanViewModel;
                list.Add(new Command("Edit details...", (vm) => {
                    EditLoan(loan.LoanID);
                }));
            }

            return list;
        }

        public override bool CanSelect<T>() {
            var t = typeof(T);
            return typeof(ReferenceSearchResult).IsAssignableFrom(t) || typeof(Journal).IsAssignableFrom(t) || typeof(Contact).IsAssignableFrom(t);
        }

        public override void Select<T>(LookupOptions options, Action<SelectionResult> success, SelectOptions selectOptions) {
            var t = typeof(T);
            if (typeof(ReferenceSearchResult).IsAssignableFrom(t)) {
                var frm = ShowReferenceManager();
                frm.BindSelectCallback(success);
            } else if (typeof(Journal).IsAssignableFrom(t)) {
                var frm = ShowJournalManager();
                frm.BindSelectCallback(success);
            } else if (typeof(Contact).IsAssignableFrom(t)) {
                var frm = ShowLoanContacts();
                frm.BindSelectCallback(success);
            } else {
                throw new Exception("Unhandled Selection Type: " + t.Name);
            }

        }

        public override bool CanEditObjectType(LookupType type) {
            switch (type) {
                case LookupType.Reference:
                case LookupType.Journal:
                case LookupType.Contact:
                case LookupType.Loan:
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
                case LookupType.Contact:
                    var loanService = new LoanService(User);
                    var cmodel = loanService.GetContact(pinnable.ObjectID);
                    if (cmodel != null) {
                        return new ContactViewModel(cmodel);
                    }
                    break;
                case LookupType.Loan:
                    loanService = new LoanService(User);
                    var lmodel = loanService.GetLoan(pinnable.ObjectID);
                    if (lmodel != null) {
                        return new LoanViewModel(lmodel);
                    }
                    break;
            }

            return null;
        }

        public void EditReference(int refID) {

            var readOnly = !User.HasPermission(PermissionCategory.SUPPORT_REFS, PERMISSION_MASK.UPDATE);

            var control = new ReferenceDetail(User, refID, readOnly);
            PluginManager.Instance.AddNonDockableContent(this, control, string.Format("Reference Detail [{0}]", refID), SizeToContent.Manual);
        }

        public void AddNewReference() {

            if (!User.HasPermission(PermissionCategory.SUPPORT_REFS, PERMISSION_MASK.INSERT)) {
                ErrorMessage.Show("You do not have sufficient priviledges to create new references!");
                return;
            }

            var control = new ReferenceDetail(User, -1, false);
            PluginManager.Instance.AddNonDockableContent(this, control, "Reference Detail", SizeToContent.Manual);
        }

        public void EditJournal(int journalID) {
            var control = new JournalDetails(User, journalID);
            PluginManager.Instance.AddNonDockableContent(this, control, string.Format("Journal Detail [{0}]", journalID), SizeToContent.Manual);
        }

        public void EditContact(int contactID) {
            var ctl = new ContactDetails(User, contactID);
            PluginManager.Instance.AddNonDockableContent(this, ctl, "Contact details", SizeToContent.Manual);
        }

        public void AddNewJournal() {

            if (!User.HasPermission(PermissionCategory.SUPPORT_JOURNALS, PERMISSION_MASK.INSERT)) {
                ErrorMessage.Show("You do not have sufficient priviledges to create new journals!");
                return;
            }

            var control = new JournalDetails(User, -1);
            PluginManager.Instance.AddNonDockableContent(this, control, "Journal Detail", SizeToContent.Manual);
        }

        public void EditLoan(int loanId) {
            var control = new LoanDetails(User, this, loanId);
            PluginManager.Instance.AddNonDockableContent(this, control, string.Format("Loan Detail [{0}]", loanId), SizeToContent.Manual);
        }

        public override void EditObject(LookupType type, int objectID) {
            switch (type) {
                case LookupType.Reference:
                    EditReference(objectID);
                    break;
                case LookupType.Journal:
                    EditJournal(objectID);
                    break;
                case LookupType.Contact:
                    EditContact(objectID);
                    break;
            }
        }

        public override T GetAdaptorForPinnable<T>(PinnableObject pinnable) {
            if (pinnable.LookupType == LookupType.Material) {
                if (typeof(T) == typeof(ILabelSetItemTarget)) {
                    return (T) (object) (new AddToLabelSetAdapter(this, User));
                }
            }

            return base.GetAdaptorForPinnable<T>(pinnable);

        }


        public void AddNewLoan() {
            var control = new LoanDetails(User, this, -1);
            PluginManager.Instance.AddNonDockableContent(this, control, "Loan Detail <New Loan>", SizeToContent.Manual);
        }


        internal void AddItemToLabelSet(PinnableObject item, string labelSetName) {
            var host = ShowLabelManager();
            if (host != null) {
                var otm = host.Control as OneToManyControl;
                if (otm != null) {
                    var existing = otm.FindItem((vm) => {
                        var set = vm as LabelSetViewModel;
                        return set.Name.Equals(labelSetName, StringComparison.CurrentCultureIgnoreCase);
                    });
                    LabelManagerControl control = null;
                    if (existing != null) {
                        otm.SetSelectedItem(existing);
                        control = otm.DetailControl as LabelManagerControl;
                    } else {
                        var set = otm.AddNew() as LabelSetViewModel;
                        if (set != null) {
                            set.Name = labelSetName;
                            otm.SetSelectedItem(set);
                            control = otm.DetailControl as LabelManagerControl;
                        }
                    }

                    if (control != null) {
                        control.AddItemFromPinnable(item);
                    }
                }
            }
        }
    }

    class AddToLabelSetAdapter : ILabelSetItemTarget {

        public AddToLabelSetAdapter(ToolsPlugin plugin, User user) {
            this.Plugin = plugin;
            this.User = user;
        }

        public void AddItemToLabelSet(PinnableObject item) {
            if (item.LookupType == LookupType.Material) {
                var frm = new AddToLabelSet(User, item.ObjectID);
                frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (frm.ShowDialog().ValueOrFalse()) {
                    // Do it!
                    Plugin.AddItemToLabelSet(item, frm.LabelSetName);
                }
            } else {
                ErrorMessage.Show("Unhandled item type for this operation: " + item.LookupType.ToString());
                return;
            }
        }

        public void AddMaterialToSet(PinnableObject item) {
        }

        protected User User { get; private set; }
        protected ToolsPlugin Plugin { get; private set; }
    }



}
