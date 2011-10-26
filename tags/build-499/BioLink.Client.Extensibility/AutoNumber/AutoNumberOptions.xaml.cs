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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for AutoNumberOptions.xaml
    /// </summary>
    public partial class AutoNumberOptions : Window {

        #region Designer Constructor
        public AutoNumberOptions() {
            InitializeComponent();
        }
        #endregion

        public AutoNumberOptions(User user, string autoNumberCategory, string table, string field) {
            InitializeComponent();
            this.User = user;
            this.AutoNumberCategory = autoNumberCategory;
            this.AutoNumberTable = table;
            this.AutoNumberField = field;
            LoadModel();
        }        

        private void btnCategories_Click(object sender, RoutedEventArgs e) {
            ShowManager();
        }

        private void ShowManager() {
            var control = new AutoNumberCategoryManager(User, AutoNumberCategory);
            var form = new ControlHostWindow(User, control, SizeToContent.WidthAndHeight);
            form.Owner = this.FindParentWindow();
            form.Title = "Auto Number Generation Categories";
            form.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            if (form.ShowDialog().ValueOrFalse()) {
                LoadModel();
            }
        }

        private void LoadModel() {
            var service = new SupportService(User);
            var list = service.GetAutoNumbersForCategory(this.AutoNumberCategory);
            var model = list.ConvertAll((n) => {
                return new AutoNumberViewModel(n);
            });
            cmbCategories.ItemsSource = model;
            string lastSelectedName = Config.GetProfile<string>(User, ConfigKey, null);
            if (!string.IsNullOrEmpty(lastSelectedName)) {
                foreach (AutoNumberViewModel vm in model) {
                    if (vm.Name.Equals(lastSelectedName)) {
                        cmbCategories.SelectedItem = vm;
                        break;
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e) {
            if (GenerateNumber()) {
                DialogResult = true;
                this.Close();
            }
        }

        public bool GenerateNumber() {

            var autoNum = cmbCategories.SelectedItem as AutoNumberViewModel;
            if (autoNum != null) {
                var service = new SupportService(User);
                int seed = txtNumber.HasValue ? txtNumber.Number : -1;
                var newAutoNumber = service.GetNextAutoNumber(autoNum.AutoNumberCatID, seed, autoNum.EnsureUnique, AutoNumberTable, AutoNumberField);
                if (newAutoNumber != null) {
                    this.AutoNumber = newAutoNumber.FormattedNumber;
                    return true;
                }                
            }

            return false;                
        }

        private string ConfigKey {
            get { return String.Format("AutoNumber.{0}.LastSelected", AutoNumberCategory); }
        }

        #region Properties

        public string AutoNumber { get; private set; }

        public string AutoNumberCategory { get; private set; }

        public string AutoNumberTable { get; private set; }

        public string AutoNumberField { get; private set; }

        public User User { get; private set; }

        #endregion

        private void cmbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var autoNum = cmbCategories.SelectedItem as AutoNumberViewModel;
            if (autoNum != null) {
                Config.SetProfile(User, ConfigKey, autoNum.Name);
            }
        }

    }

    public class AutoNumberViewModel : GenericViewModelBase<AutoNumber> {

        public AutoNumberViewModel(AutoNumber model) : base(model, null) { }

        public override string DisplayLabel {
            get {
                return GenerateDisplayLabel();
            }
        }

        public int AutoNumberCatID {
            get { return Model.AutoNumberCatID; }
            set { SetProperty(() => Model.AutoNumberCatID, value); }
        }

        public string Category {
            get { return Model.Category; }
            set { SetProperty(() => Model.Category, value); }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string Prefix {
            get { return Model.Prefix; }
            set { SetProperty(() => Model.Prefix, value); }
        }

        public string Postfix {
            get { return Model.Postfix; }
            set { SetProperty(() => Model.Postfix, value); }
        }

        public int NumLeadingZeros {
            get { return Model.NumLeadingZeros; }
            set { SetProperty(() => Model.NumLeadingZeros, value); }
        }

        public int LastNumber {
            get { return Model.LastNumber; }
            set { SetProperty(() => Model.LastNumber, value); }
        }

        public bool Locked {
            get { return Model.Locked; }
            set { SetProperty(() => Model.Locked, value); }
        }

        public bool EnsureUnique {
            get { return Model.EnsureUnique; }
            set { SetProperty(() => Model.EnsureUnique, value); }
        }

        public string Pattern {
            get {
                var sb = new StringBuilder();
                for (int i = 0; i < NumLeadingZeros; ++i) {
                    sb.Append("#");
                }                                
                return string.Format("{0}{1}{2}", Prefix, sb.ToString(), Postfix);
            }
        }

        protected string GenerateDisplayLabel() {
            return string.Format("{0} ({1})", Name, Pattern);
        }

    }

}
