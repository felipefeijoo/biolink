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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for JournalManager.xaml
    /// </summary>
    public partial class JournalManager : DatabaseCommandControl, ISelectionHostControl {

        private ObservableCollection<JournalViewModel> _findModel;
        private TabItem _previousPage;
        private JournalBrowsePage _page;

        #region designer ctor
        public JournalManager() {
            InitializeComponent();
        }
        #endregion

        public JournalManager(User user, ToolsPlugin owner) : base(user, "JournalManager") {
            InitializeComponent();
            this.Owner = owner;

            string[] ranges = new string[] { "A-C", "D-F", "G-I", "J-L", "M-O", "P-R", "S-U", "V-X", "Y-Z" };

            _page = new JournalBrowsePage(user);
            _page.LoadPage("A-C");

            foreach (string range in ranges) {
                AddTabPage(range);
            }

            ListBoxDragHelper.Bind(lstResults, CreateDragData);
            ListBoxDragHelper.Bind(_page.lst, CreateDragData);
        }

        private DataObject CreateDragData(ViewModelBase dragged) {
            var selected = dragged as JournalViewModel;
            if (selected != null) {
                var data = new DataObject("Pinnable", selected);

                var pinnable = new PinnableObject(ToolsPlugin.TOOLS_PLUGIN_NAME, LookupType.Journal, selected.JournalID);
                data.SetData(PinnableObject.DRAG_FORMAT_NAME, pinnable);
                data.SetData(DataFormats.Text, selected.DisplayLabel);
                return data;
            }
            return null;
        }

        private void AddTabPage(string range) {

            string[] bits = range.Split('-');
            if (bits.Length == 2) {
                char from = bits[0][0];
                char to = bits[1][0];
                string caption = "";
                for (char ch = from; ch <= to; ch++) {
                    caption += ch;
                }

                TabItem item = new TabItem();
                item.Header = caption.ToUpper();
                item.Tag = range;

                item.RequestBringIntoView += new RequestBringIntoViewEventHandler(item_RequestBringIntoView);
                item.LayoutTransform = new RotateTransform(90);
                tabPages.Items.Add(item);

            } else {
                throw new Exception("Invalid page range!: " + range);
            }

        }

        void item_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {

            if (sender == _previousPage) {
                return;
            }

            // Detach the page from the previous tab
            if (_previousPage != null && _previousPage != sender) {
                _previousPage.Content = null;
            }

            var selected = sender as TabItem;
            if (selected != null) {
                // Load the page with the correct items for the selected tab
                _page.LoadPage(selected.Tag as string);
                // Attach the page to the current tab
                selected.Content = _page;                
            }
            _previousPage = selected;
            
        }

        public ToolsPlugin Owner { get; private set; }

        private void txtFind_TypingPaused(string text) {
            if (string.IsNullOrEmpty(text)) {
                _findModel = null;
            } else {
                var service = new SupportService(User);
                var list = service.FindJournals(text);
                _findModel = new ObservableCollection<JournalViewModel>(list.ConvertAll((model) => {
                    return new JournalViewModel(model);
                }));
            }

            lstResults.ItemsSource = _findModel;
        }

        private void PinSelected() {
            var selected = GetSelected();
            if (selected != null) {
                var pinnable = new PinnableObject(ToolsPlugin.TOOLS_PLUGIN_NAME, LookupType.Journal, selected.JournalID, selected.FullName);
                PluginManager.Instance.PinObject(pinnable);
            }
        }

        private void ShowJournalMenu(ListBox lst) {
            var viewModel = lst.SelectedItem as JournalViewModel;
            if (viewModel != null) {
                ContextMenuBuilder builder = new ContextMenuBuilder(null);
                builder.New("_Edit journal").Handler(() => { EditJournal(viewModel); }).End();
                builder.Separator();
                builder.New("_Pin to pinboard").Handler(() => { PinSelected(); }).End();
                builder.Separator();
                builder.New("_Add new Journal").Handler(() => { AddNew(); }).End();
                builder.New("_Delete Journal").Handler(() => { DeleteJournal(viewModel); }).End();
                lst.ContextMenu = builder.ContextMenu;
            }
        }

        private void DeleteJournal(JournalViewModel model) {

            if (!User.HasPermission(PermissionCategory.SUPPORT_JOURNALS, PERMISSION_MASK.DELETE)) {
                ErrorMessage.Show("You do not have sufficient priviledges to delete journals!");
                return;
            }

            var service = new SupportService(User);
            if (service.OkToDeleteJournal(model.JournalID)) {
                if (this.Question(string.Format("Are you sure you wish to permanently delete the journal '{0}'?", model.FullName), "Delete Journal?")) {
                    if (_findModel.Contains(model)) {
                        _findModel.Remove(model);
                    }

                    if (_page.Model.Contains(model)) {
                        _findModel.Remove(model);
                    }

                    RegisterPendingChange(new DeleteJournalCommand(model.Model));
                }
            } else {
                ErrorMessage.Show("You cannot delete this Journal at this time. This is most probably because this Journal is cited in one or more References.");
            }
        }

        private void EditJournal(JournalViewModel viewModel) {
            if (viewModel != null) {
                Owner.EditJournal(viewModel.JournalID);
            }
        }

        private void AddNew() {

            if (!User.HasPermission(PermissionCategory.SUPPORT_JOURNALS, PERMISSION_MASK.INSERT)) {
                ErrorMessage.Show("You do not have sufficient priviledges to add new journals!");
                return;
            }

            Owner.AddNewJournal();
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {
            AddNew();
        }

        private JournalViewModel GetSelected() {
            JournalViewModel selected = null;
            if (tab.SelectedItem == findTab) {
                selected = lstResults.SelectedItem as JournalViewModel;
            } else {
                selected = _page.SelectedItem;
            }

            return selected;
        }

        private void btnProperties_Click(object sender, RoutedEventArgs e) {
            JournalViewModel selected = GetSelected();
            if (selected != null) {
                EditJournal(selected);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            JournalViewModel selected = GetSelected();
            if (selected != null) {
                DeleteJournal(selected);
            }
        }

        private void lstResults_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var lst = sender as ListBox;
            if (lst != null) {
                ShowJournalMenu(lst);
            }
        }

        public SelectionResult Select() {
            JournalViewModel selected = GetSelected();
            if (selected != null) {
                var res = new JournalSelectionResult(selected.Model);
                return res;
            }
            return null;
        }

    }

    public class JournalSelectionResult : SelectionResult {

        public JournalSelectionResult(Journal data) {
            this.DataObject = data;
            this.Description = data.FullName;
            this.ObjectID = data.JournalID;
        }

    }

}
