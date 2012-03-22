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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for LoanSearchControl.xaml
    /// </summary>
    public partial class LoanSearchControl : DatabaseCommandControl {

        private ObservableCollection<LoanViewModel> _model;

        public LoanSearchControl(User user, ToolsPlugin plugin) : base( user, "LoanSearch") {
            InitializeComponent();
            this.Plugin = plugin;

            Loaded += new RoutedEventHandler(LoanSearchControl_Loaded);

            txtFind.PreviewKeyDown += new KeyEventHandler(txtFind_PreviewKeyDown);

            lvw.MouseRightButtonUp += new MouseButtonEventHandler(lvw_MouseRightButtonUp);

            ListViewDragHelper.Bind(lvw, ListViewDragHelper.CreatePinnableGenerator(ToolsPlugin.TOOLS_PLUGIN_NAME, LookupType.Loan));
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

            if (string.IsNullOrWhiteSpace(searchTerm) || what == null) {
                lvw.ItemsSource = new ObservableCollection<LoanViewModel>();
                return;
            }

            var service = new LoanService(User);

            if (!searchTerm.EndsWith("*") && !searchTerm.EndsWith("%")) {
                searchTerm += "*";
            }

            var list = service.FindLoans(searchTerm, what.Second, chkFindOnlyOpenLoans.IsChecked.ValueOrFalse());
            _model = new ObservableCollection<LoanViewModel>(list.Select((m) => {
                return new LoanViewModel(m);
            }));

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



    }
}
