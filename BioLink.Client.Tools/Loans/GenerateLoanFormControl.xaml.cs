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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using Microsoft.Win32;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for GenerateLoanFormControl.xaml
    /// </summary>
    public partial class GenerateLoanFormControl : Window {

        public GenerateLoanFormControl(User user, ToolsPlugin plugin, int loanId) {
            InitializeComponent();

            User = user;
            Plugin = plugin;
            LoanID = loanId;

            lvw.MouseDoubleClick += lvw_MouseDoubleClick;

            Loaded += GenerateLoanFormControl_Loaded;
        }

        void lvw_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            GenerateSelectedLoanForm();
        }

        void GenerateLoanFormControl_Loaded(object sender, RoutedEventArgs e) {
            var service = new SupportService(User);

            var forms = service.GetMultimediaItems(TraitCategoryType.Biolink.ToString(), SupportService.BIOLINK_INTRA_CAT_ID);
            var model = new ObservableCollection<LoanFormTemplateViewModel>(forms.Select(m => new LoanFormTemplateViewModel(m)));

            lvw.ItemsSource = model;
        }

        protected User User { get; private set; }

        protected ToolsPlugin Plugin { get; private set; }

        protected int LoanID { get; set; }

        private void btnManageForms_Click(object sender, RoutedEventArgs e) {
            Plugin.ShowLoanFormManager();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            GenerateSelectedLoanForm();
        }

        private void GenerateSelectedLoanForm() {
            var selected = lvw.SelectedItem as LoanFormTemplateViewModel;
            if (selected != null) {
                GenerateLoanForm(selected.MultimediaID);
            }
        }

        private void GenerateLoanForm(int mmID) {
            var service = new LoanService(User);
            var loan = service.GetLoan(LoanID);
            var loanMaterial = service.GetLoanMaterial(LoanID);
            // var loanCorrespondence = service.GetLoanCorrespondence(LoanID);

            var supportSevice = new SupportService(User);
            var loanTraits = supportSevice.GetTraits(TraitCategoryType.Loan.ToString(), LoanID);
            
            var originator = loan.OriginatorID != 0 ? service.GetContact(loan.OriginatorID) : null;
            var requestor = loan.RequestorID != 0 ? service.GetContact(loan.RequestorID) : null;
            var receiver = loan.ReceiverID != 0 ? service.GetContact(loan.ReceiverID) : null;

            var template = supportSevice.GetMultimedia(mmID);
            
            var generator = LoanFormGeneratorFactory.GetLoanFormGenerator(template);

            if (generator != null) {
                var outputFile = generator.GenerateLoanForm(template, loan, loanMaterial, loanTraits, originator, requestor, receiver);
                if (outputFile != null) {
                    var filename = ChooseFilename(loan, template);
                    if (!string.IsNullOrWhiteSpace(filename)) {
                        outputFile.CopyTo(filename, true);
                        SystemUtils.ShellExecute(filename);
                        Close();
                    }
                }

            } else {
                ErrorMessage.Show("Unable to generate loan form - unrecognized template extension: {0}", template.FileExtension);
            }

        }

        private string ChooseFilename(Loan loan, Multimedia template) {           
            var dlg = new SaveFileDialog();
            var defaultDir = Config.GetUser(User, "Loans.Forms.DefaultOutputDirectory", dlg.InitialDirectory);
            if (!string.IsNullOrWhiteSpace(defaultDir)) {
                var dirinfo = new DirectoryInfo(defaultDir);
                if (dirinfo.Exists) {
                    dlg.InitialDirectory = dirinfo.FullName;
                }
            }

            var borrowerName = LoanService.FormatName("", loan.RequestorGivenName, loan.RequestorName);
            dlg.FileName = SystemUtils.StripIllegalFilenameChars(string.Format("{0}_{1}_{2:yyyy-MM-dd}.{3}", loan.LoanNumber, borrowerName, DateTime.Now, template.FileExtension));
            if (dlg.ShowDialog() == true) {
                var finfo = new FileInfo(dlg.FileName);
                Config.SetUser(User, "Loans.Forms.DefaultOutputDirectory", finfo.DirectoryName);
                return dlg.FileName;
            }
            return null;
        }


    }

    public class LoanFormTemplateViewModel : MultimediaLinkViewModel {

        public LoanFormTemplateViewModel(MultimediaLink model) : base(model) { }

        public string FileDesc {
            get { return string.Format("{0} {1}", Extension, ByteLengthConverter.FormatBytes(SizeInBytes)); }
        }
    }

}
