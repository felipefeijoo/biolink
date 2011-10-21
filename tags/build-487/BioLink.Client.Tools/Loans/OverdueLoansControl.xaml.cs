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
    /// Interaction logic for OverdueLoansControl.xaml
    /// </summary>
    public partial class OverdueLoansControl : UserControl {

        public OverdueLoansControl(User user, ToolsPlugin plugin) {
            InitializeComponent();
            this.User = user;
            this.Plugin = plugin;
            lvw.MouseDoubleClick += new MouseButtonEventHandler(lvw_MouseDoubleClick);
            lvw.PreviewMouseRightButtonUp += new MouseButtonEventHandler(lvw_PreviewMouseRightButtonUp);
            Loaded += new RoutedEventHandler(OverdueLoansControl_Loaded);
        }

        void lvw_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var builder = new ContextMenuBuilder(null);
            builder.New("Edit loan").Handler(() => { EditSelectedLoan(); }).End();
            lvw.ContextMenu = builder.ContextMenu;
        }

        void lvw_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            EditSelectedLoan();
        }

        private void EditSelectedLoan() {
            var reminder = lvw.SelectedItem as LoanReminderExViewModel;
            if (reminder != null) {
                Plugin.EditLoan(reminder.LoanID);
            }
        }

        void OverdueLoansControl_Loaded(object sender, RoutedEventArgs e) {

            var service = new LoanService(User);
            var list = service.GetRemindersDue(DateTime.Now);
            var model = new ObservableCollection<LoanReminderExViewModel>(list.Select((m) => {
                return new LoanReminderExViewModel(m);
            }));

            lvw.ItemsSource = model;
            lblStatus.Content = string.Format("{0} reminders retreived.", model.Count);
        }

        protected User User { get; private set; }

        protected ToolsPlugin Plugin { get; private set; }
    }

    public class LoanReminderExViewModel : LoanReminderViewModel {

        public LoanReminderExViewModel(LoanReminderEx model) : base(model) { 
            ExModel = model;
        }

        public string LoanNumber {
            get { return ExModel.LoanNumber; }
            set { SetProperty(() => ExModel.LoanNumber, value); }
        }

        public DateTime? DateInitiated { 
            get { return ExModel.DateInitiated;}
            set { SetProperty(() => ExModel.DateInitiated, value); }
        }

        protected LoanReminderEx ExModel { get; private set; }

        

    }
}
