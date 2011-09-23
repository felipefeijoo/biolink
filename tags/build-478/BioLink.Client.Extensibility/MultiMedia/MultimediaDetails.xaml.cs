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
using BioLink.Data.Model;
using BioLink.Data;


namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for MultimediaDetails.xaml
    /// </summary>
    public partial class MultimediaDetails : DatabaseCommandControl {

        #region Designer Constructor
        public MultimediaDetails() {
            InitializeComponent();
        }
        #endregion

        public MultimediaDetails(Multimedia multimedia, User user) : base(user, "Multimedia::" + multimedia.MultimediaID) {
            InitializeComponent();
            this.Multimedia = new MultimediaViewModel(multimedia);
            AddTabItem("Traits", new TraitControl(User, TraitCategoryType.Multimedia, Multimedia));
            AddTabItem("Ownership", new OwnershipDetails(multimedia));
            txtArtist.BindUser(user, "tblMultimedia", "vchrArtist");
            txtOwner.BindUser(user, "tblMultimedia", "vchrOwner");
            txtIDNumber.BindUser(user, "MultimediaID", "tblMultimedia", "vchrNumber");
            this.DataContext = this.Multimedia;

            Multimedia.DataChanged += new DataChangedHandler(Multimedia_DataChanged);
        }

        void Multimedia_DataChanged(ChangeableModelBase viewmodel) {
            RegisterUniquePendingChange(new UpdateMultimediaCommand(Multimedia.Model));
        }

        private TabItem AddTabItem(string title, UIElement content, Action bringIntoViewAction = null) {
            TabItem tabItem = new TabItem();
            tabItem.Header = title;
            tabItem.Content = content;
            tab.Items.Add(tabItem);
            if (bringIntoViewAction != null) {
                tabItem.RequestBringIntoView += new RequestBringIntoViewEventHandler((s, e) => {
                    bringIntoViewAction();
                });
            }

            return tabItem;
        }

        #region Properties

        public MultimediaViewModel Multimedia { get; private set; }

        #endregion

    }


}
