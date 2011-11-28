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


namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for ImportWizard.xaml
    /// </summary>
    public partial class ImportWizard : Window {

        private WizardPage[] _pages;
        private Object _context;

        static ImportWizard() {
            MoveNext = new RoutedCommand("MoveNextCommand", typeof(ImportWizard));
            MovePrevious = new RoutedCommand("MovePreviousCommand", typeof(ImportWizard));
        }


        public ImportWizard(User user, string title, object context, params WizardPage[] pages) {
            // Command bindings...
            this.CommandBindings.Add(new CommandBinding(MoveNext, ExecutedMoveNext, CanExecuteMoveNext));
            this.CommandBindings.Add(new CommandBinding(MovePrevious, ExecutedMovePrevious, CanExecuteMovePrevious));

            InitializeComponent();

            this.CurrentPageIndex = -1;

            this.Title = title;
            this._context = context;

            _pages = pages;
            if (_pages != null && _pages.Length > 0) {
                
                int idx = 0;
                foreach (WizardPage page in pages) {

                    page.BindUser(user, context);

                    page.RequestMoveNext += new Action<WizardPage>(page_RequestMoveNext);
                    page.RequestMovePrevious += new Action<WizardPage>(page_RequestMovePrevious);
                    page.RequestDisableNavigation += new Action<WizardPage>(page_RequestDisableNavigation);
                    page.RequestEnableNavigation += new Action<WizardPage>(page_RequestEnableNavigation);
                    page.WizardComplete += new Action<WizardPage>(page_WizardComplete);

                    var lbl = new Label();
                    Grid.SetRow(lbl, idx++);
                    Grid.SetColumn(lbl, 1);
                    lbl.Content = page.PageTitle;
                    gridSidebar.Children.Add(lbl);
                }

                SetCurrentPage(0);
            }
        }

        void page_WizardComplete(WizardPage obj) {
            btnCancel.Content = "_Close";
        }

        void page_RequestEnableNavigation(WizardPage obj) {
            gridButtons.IsEnabled = true;
        }

        void page_RequestDisableNavigation(WizardPage obj) {
            gridButtons.IsEnabled = false;
        }

        void page_RequestMovePrevious(WizardPage obj) {
            MovePreviousPage();
        }

        void page_RequestMoveNext(WizardPage obj) {
            MoveNextPage();            
        }

        private void SetCurrentPage(int pageIndex) {
            if (pageIndex >= 0 && pageIndex < _pages.Length) {

                // Deal with the existing page...    
                var todirection = pageIndex >= CurrentPageIndex ? WizardDirection.Next : WizardDirection.Previous;

                if (CurrentPageIndex < 0 || CurrentPage.OnPageExit(todirection)) {

                    CurrentPageIndex = pageIndex;
                    gridContent.Children.Clear();
                    gridContent.Children.Add(_pages[pageIndex]);
                    Grid.SetRow(imgCurrentPage, pageIndex);

                    btnCancel.Content = "_Cancel";

                    CurrentPage.OnPageEnter(todirection == WizardDirection.Previous ? WizardDirection.Next : WizardDirection.Previous);
                }

            } else {
                throw new Exception("Page index out of bounds: " + pageIndex);
            }
        }

        protected void MoveNextPage() {
            SetCurrentPage(CurrentPageIndex + 1);
        }

        protected void MovePreviousPage() {
            SetCurrentPage(CurrentPageIndex - 1);
        }

        #region Properties

        protected int CurrentPageIndex { get; set; }

        public WizardPage CurrentPage {
            get { return CurrentPageIndex >= 0 ? _pages[CurrentPageIndex] : null; }
        }

        public int PageCount {
            get { return _pages == null ? 0 : _pages.Length;}
        }

        public WizardPage NextPage {
            get {
                if (CurrentPageIndex < PageCount - 1) {
                    return _pages[CurrentPageIndex + 1];
                }
                return null;
            }
        }

        public WizardPage PreviousPage {
            get {
                if (CurrentPageIndex > 0) {
                    return _pages[CurrentPageIndex - 1];
                }
                return null;
            }
        }

        #endregion


        #region Commands

        public static RoutedCommand MoveNext { get; private set; }

        private void CanExecuteMoveNext(object target, CanExecuteRoutedEventArgs e) {

            ImportWizard wizard = target as ImportWizard;

            e.CanExecute = false;

            if (target != null) {                
                var nextPage = wizard.NextPage;
                if (nextPage != null) {
                    if (wizard.CurrentPage.CanMoveNext()) {
                        e.CanExecute = true;
                    }
                }
            }
        }

        private void ExecutedMoveNext(object target, ExecutedRoutedEventArgs e) {
            ImportWizard wizard = target as ImportWizard;
            if (wizard != null) {
                wizard.MoveNextPage();
            }
        }

        public static RoutedCommand MovePrevious { get; private set; }

        private void CanExecuteMovePrevious(object target, CanExecuteRoutedEventArgs e) {

            ImportWizard wizard = target as ImportWizard;

            e.CanExecute = false;

            if (target != null) {
                var prevPage = wizard.PreviousPage;
                if (prevPage != null) {
                    if (prevPage.CanMovePrevious()) {
                        e.CanExecute = true;
                    }
                }
            }
        }

        private void ExecutedMovePrevious(object target, ExecutedRoutedEventArgs e) {
            ImportWizard wizard = target as ImportWizard;
            if (target != null) {
                wizard.MovePreviousPage();
            }
        }

        #endregion

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }


    }

    public class WizardPage : UserControl {

        public WizardPage() {
        }

        public void BindUser(User user, Object context) {
            this.User = user;
            this.WizardContext = context;
        }

        public virtual string PageTitle { 
            get { return "Page Title"; } 
        }

        public virtual bool CanMoveNext() {
            return true;
        }

        public virtual bool CanMovePrevious() {
            return true;
        }

        public virtual void OnPageEnter(WizardDirection fromdirection) {
        }

        public virtual bool OnPageExit(WizardDirection todirection) {
            return true;
        }

        protected void RaiseRequestNextPage() {
            if (RequestMoveNext != null) {
                RequestMoveNext(this);
            }
        }

        protected void RaiseRequestPreviousPage() {
            if (RequestMovePrevious != null) {
                RequestMovePrevious(this);
            }
        }

        protected void RaiseRequestDisableNavigation() {
            if (RequestDisableNavigation != null) {
                RequestDisableNavigation(this);
            }
        }

        protected void RaiseRequestEnableNavigation() {
            if (RequestEnableNavigation != null) {
                RequestEnableNavigation(this);
            }
        }

        protected void RaiseWizardComplete() {
            if (WizardComplete != null) {
                WizardComplete(this);
            }
        }

        protected User User { get; private set; }

        protected Object WizardContext { get; set; }

        public event Action<WizardPage> RequestMoveNext;

        public event Action<WizardPage> RequestMovePrevious;

        public event Action<WizardPage> RequestDisableNavigation;

        public event Action<WizardPage> RequestEnableNavigation;

        public event Action<WizardPage> WizardComplete;

    }

    public enum WizardDirection {
        Next, Previous
    }
}
