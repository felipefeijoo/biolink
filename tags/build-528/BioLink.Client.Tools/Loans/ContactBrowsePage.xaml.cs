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
    /// Interaction logic for ContactBrowsePage.xaml
    /// </summary>
    public partial class ContactBrowsePage : UserControl {

        private ObservableCollection<ContactViewModel> _model = new ObservableCollection<ContactViewModel>();

        #region Designer ctor
        public ContactBrowsePage() {
            InitializeComponent();            
        }
        #endregion

        public ContactBrowsePage(User user) {
            InitializeComponent();
            lvw.MouseRightButtonUp += new MouseButtonEventHandler(lvw_MouseRightButtonUp);

            ListViewDragHelper.Bind(lvw, ListViewDragHelper.CreatePinnableGenerator(ToolsPlugin.TOOLS_PLUGIN_NAME, LookupType.Contact));

            this.User = user;
            lvw.ItemsSource = _model;
        }

        void lvw_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var selected = lvw.SelectedItem as ContactViewModel;
            if (ContextMenuRequested != null) {
                ContextMenuRequested(lvw, selected);
            }
        }

        public void LoadPage(string range) {

            lblPageHeader.Content = string.Format("Contacts - {0}", range);

            string[] bits = range.Split('-');
            _model.Clear();
            if (bits.Length == 2) {
                string from = bits[0];
                string to = bits[1];

                var service = new LoanService(User);
                var list = service.ListContactsRange(from, to);                
                foreach (Contact contact in list) {
                    _model.Add(new ContactViewModel(contact));
                }

            }            
        }

        public event Action<FrameworkElement, ContactViewModel> ContextMenuRequested;

        public void Clear() {
            _model.Clear();
        }

        public User User { get; private set; }

        public ContactViewModel SelectedItem {
            get { return lvw.SelectedItem as ContactViewModel; }
        }

        public ObservableCollection<ContactViewModel> Model { get { return _model; } }
    }
}
