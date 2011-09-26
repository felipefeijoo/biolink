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

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for SiteVisitDetails.xaml
    /// </summary>
    public partial class SiteVisitDetails : DatabaseCommandControl {

        #region Designer Constructor
        public SiteVisitDetails() {
            InitializeComponent();
        }
        #endregion

        public SiteVisitDetails(User user, int siteVisitId, bool readOnly) :base(user, "SiteVisit:" + siteVisitId) {
            InitializeComponent();

            this.IsReadOnly = readOnly;

            var service = new MaterialService(user);
            var model = service.GetSiteVisit(siteVisitId);
            var viewModel = new SiteVisitViewModel(model);
            this.DataContext = viewModel;

            txtCollector.BindUser(user);

            viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);

            tab.AddTabItem("Traits", new TraitControl(user, TraitCategoryType.SiteVisit, viewModel) { IsReadOnly = readOnly });
            tab.AddTabItem("Notes", new NotesControl(user, TraitCategoryType.SiteVisit, viewModel) { IsReadOnly = readOnly });
            tab.AddTabItem("Ownership", new OwnershipDetails(model));
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            RegisterUniquePendingChange(new UpdateSiteVisitCommand((viewmodel as SiteVisitViewModel).Model));
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(SiteVisitDetails), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (SiteVisitDetails)obj;
            if (control != null) {
                var readOnly = (bool)args.NewValue;
                control.SetReadOnlyRecursive(readOnly);
            }
        }


    }
}
