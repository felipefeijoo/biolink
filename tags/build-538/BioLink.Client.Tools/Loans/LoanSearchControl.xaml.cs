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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for LoanSearchControl.xaml
    /// </summary>
    public partial class LoanSearchControl : DatabaseCommandControl {

        private ObservableCollection<LoanViewModel> _model;
        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public LoanSearchControl(User user, ToolsPlugin plugin) : base( user, "LoanSearch") {
            InitializeComponent();
            this.Plugin = plugin;

            Loaded += new RoutedEventHandler(LoanSearchControl_Loaded);

            txtFind.PreviewKeyDown += new KeyEventHandler(txtFind_PreviewKeyDown);

            lvw.MouseRightButtonUp += new MouseButtonEventHandler(lvw_MouseRightButtonUp);

            lvw.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));

            ListViewDragHelper.Bind(lvw, ListViewDragHelper.CreatePinnableGenerator(ToolsPlugin.TOOLS_PLUGIN_NAME, LookupType.Loan));
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e) {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null) {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding) {
                    if (headerClicked != _lastHeaderClicked) {
                        direction = ListSortDirection.Ascending;
                    } else {
                        if (_lastDirection == ListSortDirection.Ascending) {
                            direction = ListSortDirection.Descending;
                        } else {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    Sort(headerClicked, direction);

                    if (direction == ListSortDirection.Ascending) {
                        headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    } else {
                        headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked) {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }            
            }
        }

        private void Sort(GridViewColumnHeader columnHeader, ListSortDirection direction) {
            String columnName = columnHeader.Content as String;

            String memberName = "";

            if (columnHeader.Content as String == "Loan #") {
                memberName = "LoanNumber";
            } else {
                var dmb = columnHeader.Column.DisplayMemberBinding;
                if (dmb is Binding) {
                    memberName = (dmb as Binding).Path.Path.ToString();
                }
            }

            if (!String.IsNullOrEmpty(memberName)) {
                ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvw.ItemsSource) as ListCollectionView;
                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(memberName, direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }

        void lvw_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var builder = new ContextMenuBuilder(null);

            builder.New("Edit Loan").Handler(() => { EditSelectedLoan(); }).End();
            builder.Separator();
            builder.New("Delete Loan").Handler(() => { DeleteSelectedLoan(); }).End();
            builder.Separator();
            builder.New("Add New Loan").Handler(() => { AddNewLoan(); }).End();

            lvw.ContextMenu = builder.ContextMenu;

        }

        void txtFind_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                e.Handled = true;
                DoFind();
            }
        }

        private void DoFind() {
            
            var searchTerm = txtFind.Text;
            var what = cmbWhat.SelectedItem as Pair<string, string>;
            lblResults.Content = "";

            if (string.IsNullOrWhiteSpace(searchTerm) || what == null) {
                lvw.ItemsSource = new ObservableCollection<LoanViewModel>();
                return;
            }

            var service = new LoanService(User);

            if (!searchTerm.EndsWith("*") && !searchTerm.EndsWith("%")) {
                searchTerm += "*";
            }

            if (!searchTerm.StartsWith("*") && !searchTerm.StartsWith("%")) {
                searchTerm = "*" + searchTerm;
            }


            var list = service.FindLoans(searchTerm, what.Second, chkFindOnlyOpenLoans.IsChecked.ValueOrFalse());
            _model = new ObservableCollection<LoanViewModel>(list.Select((m) => {
                return new LoanViewModel(m);
            }));

            lblResults.Content = String.Format("{0} matching loans found", list.Count);

            lvw.ItemsSource = _model;
        }

        protected Pair<string, string> Pair(string a, string b) {
            return new Pair<string, string>(a, b);
        }

        void LoanSearchControl_Loaded(object sender, RoutedEventArgs e) {

            var options = new Pair<string, string>[] { Pair("Find in all (loan fields)", "A"), Pair("Loan number", "L"), Pair("Permit number", "P"), Pair("Taxon name", "T") };

            cmbWhat.ItemsSource = options;
            cmbWhat.DisplayMemberPath = "First";
            cmbWhat.SelectionChanged += new SelectionChangedEventHandler(cmbWhat_SelectionChanged);

            cmbWhat.SelectedIndex = Config.GetProfile<int>(User, "FindLoans.LastFindWhat", 0);

            txtFind.Focus();

        }

        void cmbWhat_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Config.SetProfile(User, "FindLoans.LastFindWhat", cmbWhat.SelectedIndex);
        }

        protected ToolsPlugin Plugin { get; private set; }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {
            AddNewLoan();
        }

        private void btnProperties_Click(object sender, RoutedEventArgs e) {
            EditSelectedLoan();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelectedLoan();
        }

        private void AddNewLoan() {
            Plugin.AddNewLoan();
        }

        private LoanViewModel GetSelectedLoan() {
            return lvw.SelectedItem as LoanViewModel;
        }

        private void EditSelectedLoan() {
            var selected = GetSelectedLoan();
            if (selected != null) {
                Plugin.EditLoan(selected.LoanID);
            }
        }

        private void DeleteSelectedLoan() {

            var loan = GetSelectedLoan();
            if (loan == null) {
                return;
            }

            loan.IsDeleted = true;
            _model.Remove(loan);
            RegisterUniquePendingChange(new DeleteLoanCommand(loan.Model));
        }

        private void btnFind_Click(object sender, RoutedEventArgs e) {
            DoFind();
        }

        private void lvw_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DependencyObject src = (DependencyObject)(e.OriginalSource);
            while (!(src is Control)) {
                src = VisualTreeHelper.GetParent(src);
            }

            if (src != null && src is ListViewItem) {
                if (lvw.SelectedItem != null) {
                    EditSelectedLoan();
                }
            }

        }

        private void btnExport_Click(object sender, RoutedEventArgs e) {
            var report = new GenericModelReport<LoanViewModel>(User, "Loan report", _model, "LoanNumber", "RequestedBy", "ReceivedBy", "AuthorizedBy", "IsOverdue", "Status", "DateInitiated", "DateDue", "DateClosed", "PermitNumber" );
            PluginManager.Instance.RunReport(Plugin, report);
        }

    }
}
