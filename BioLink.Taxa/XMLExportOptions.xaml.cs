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
using System.IO;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for XMLExportOptions.xaml
    /// </summary>
    public partial class XMLExportOptions : Window, IProgressObserver {

        private bool _isCancelled;

        public XMLExportOptions(User user, List<int> taxonIds) {
            InitializeComponent();
            TaxonIDs = taxonIds;
            this.User = user;

            var lastFile = Config.GetUser(User, "XMLIOExport.LastExportFile", "");            
            if (!string.IsNullOrEmpty(lastFile)) {
                if (taxonIds.Count == 1) {
                    var service = new TaxaService(user);
                    var taxon = service.GetTaxon(taxonIds[0]);
                    if (taxon != null) {
                        var f = new FileInfo(lastFile);
                        var directory = f.DirectoryName;
                        txtFilename.Text = System.IO.Path.Combine(f.DirectoryName, SystemUtils.StripIllegalFilenameChars(taxon.TaxaFullName) + ".xml");
                    }
                } else {
                    txtFilename.Text = lastFile;
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        protected List<int> TaxonIDs { get; private set; }

        protected User User { get; private set; }

        public string Filename {
            get { return txtFilename.Text; }
        }

        public bool ExportChildTaxa {
            get { return chkChildTaxa.IsChecked.Value; }
        }

        public bool ExportMaterial {
            get { return chkMaterial.IsChecked.Value; }
        }

        public bool ExportMultimedia {
            get { return chkMultimedia.IsChecked.Value; }
        }

        public bool ExportTraits {
            get { return chkTraits.IsChecked.Value; }
        }

        public bool ExportNotes {
            get { return chkNotes.IsChecked.Value; }
        }

        public bool IncludeFullClassification {
            get { return chkClassification.IsChecked.Value; }
        }

        public bool KeepLogFile {
            get { return chkLogFile.IsChecked.Value; }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            StartExport();
        }

        private void StartExport() {

            _isCancelled = false;

            // save the file off in prefs for later...
            Config.SetUser(User, "XMLIOExport.LastExportFile", txtFilename.Text);

            if (string.IsNullOrEmpty(txtFilename.Text)) {
                ErrorMessage.Show("You must select a destination file before you can continue");
                return;
            }

            btnStart.IsEnabled = false;
            btnCancel.IsEnabled = true;

            var options = new XMLIOExportOptions { Filename = this.Filename, ExportChildTaxa = this.ExportChildTaxa, ExportMaterial = this.ExportMaterial, ExportTraits = this.ExportNotes, ExportMultimedia = this.ExportMultimedia, ExportNotes = this.ExportNotes, IncludeFullClassification = this.IncludeFullClassification, KeepLogFile = this.KeepLogFile };

            var service = new XMLIOService(User);
            JobExecutor.QueueJob(() => {
                service.ExportXML(TaxonIDs, options, this, IsCancelled);
                this.InvokeIfRequired(() => {
                    btnStart.IsEnabled = true;
                    btnCancel.IsEnabled = false;
                });
            });

        }

        internal bool IsCancelled() {
            return _isCancelled;
        }

        public void ProgressStart(string message, bool indeterminate = false) {
            progressBar.InvokeIfRequired(() => {
                this.progressBar.Minimum = 0;
                this.progressBar.Maximum = 100;
                this.progressBar.Value = 0;
                lblProgress.Content = message;
                progressBar.IsIndeterminate = indeterminate;
            });
        }

        public void ProgressMessage(string message, double? percentComplete = null) {

            progressBar.InvokeIfRequired(() => {
                lblProgress.Content = message;
                if (percentComplete.HasValue) {
                    progressBar.IsIndeterminate = false;
                    this.progressBar.Value = percentComplete.Value;
                }
            });
        }

        public void ProgressEnd(string message) {
            progressBar.InvokeIfRequired(() => {
                this.progressBar.Value = 0;
                lblProgress.Content = message;
                progressBar.IsIndeterminate = false;
            });

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            _isCancelled = true;
        }
    }

}
