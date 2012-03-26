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
using System.IO;
using System.Collections.ObjectModel;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for XMLIOImportOptions.xaml
    /// </summary>
    public partial class XMLIOImportOptions : Window, IXMLImportProgressObserver {

        public XMLIOImportOptions() {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            StartImport();
        }

        private void StartImport() {
            string filename = txtFilename.Text;
                
            if (string.IsNullOrWhiteSpace(filename)) {
                ErrorMessage.Show("You must supply the name of an XML file to import from");
                return;
            }

            if (!File.Exists(filename)) {
                ErrorMessage.Show("The specified file does not exist!");
                return;
            }



            JobExecutor.QueueJob(() => {
                this.InvokeIfRequired(() => {
                    btnStart.IsEnabled = false;
                    btnCancel.IsEnabled = false;
                    btnStop.IsEnabled = true;
                });

                try {

                    var importer = new XMLIOImporter(PluginManager.Instance.User, filename, this);
                    importer.Import();

                } finally {
                    this.InvokeIfRequired(() => {
                        btnStart.IsEnabled = true;
                        btnCancel.IsEnabled = true;
                        btnStop.IsEnabled = false;
                    });
                }
            });


        }

        public void ProgressMessage(string message) {
            lblMessage.InvokeIfRequired(() => {
                lblMessage.Content = message;
            });
        }

        public void ImportStarted(string message, List<XMLImportProgressItem> items) {

            lvw.InvokeIfRequired(() => {
                lvw.ItemsSource = new ObservableCollection<ProgressItemViewModel>(items.Select((m) => {
                    return new ProgressItemViewModel(m);
                }));
            });

        }

        public void ImportCompleted() {
            lblMessage.InvokeIfRequired(() => {
                lblMessage.Content = "Import complete.";
            });

        }

        public void ProgressTick(string itemName, int countCompleted) {
            var model = lvw.ItemsSource as ObservableCollection<ProgressItemViewModel>;
            if (model != null) {
                var item = model.FirstOrDefault((vm) => {
                    return vm.Name.Equals(itemName);
                });
                if (item != null) {
                    item.Completed = countCompleted;
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }

    class ProgressItemViewModel : ViewModelBase {

        private string _Name;
        private int _Total;
        private int _Completed;

        public ProgressItemViewModel(XMLImportProgressItem model) {
            _Name = model.Name;
            _Total = model.Total;
            _Completed = model.Completed;
            
        }

        public string Name {
            get { return _Name; }
            set { SetProperty("Name", ref _Name, value); }
        }

        public int Total {
            get { return _Total; }
            set { SetProperty("Total", ref _Total, value); }
        }

        public int Completed {
            get { return _Completed; }
            set { 
                SetProperty("Completed", ref _Completed, value);
                RaisePropertyChanged("PercentStr");
            }
        }

        public string PercentStr {            
            get {     
                int percent = (Total == 0 ? 0 :  (int) (((double)Completed / (double)Total) * 100.0));
                return string.Format("{0}%", percent); 
            }
        }

        public override int? ObjectID {
            get { return null; }
        }

    }
}
