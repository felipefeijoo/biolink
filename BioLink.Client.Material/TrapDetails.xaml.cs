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
    /// Interaction logic for TrapDetail.xaml
    /// </summary>
    public partial class TrapDetails : DatabaseCommandControl {

        #region Designer Constructor

        public TrapDetails() {
            InitializeComponent();
        }

        #endregion

        public TrapDetails(User user, int trapID, bool readOnly) : base(user, "Trap:" + trapID) {
            InitializeComponent();
            var service = new MaterialService(user);
            var model = service.GetTrap(trapID);
            var viewModel = new TrapViewModel(model);

            IsReadOnly = readOnly;

            this.DataContext = viewModel;

            viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);

            txtTrapType.BindUser(User, PickListType.Phrase, "Trap Type", TraitCategoryType.Trap);

            tabTrap.AddTabItem("Traits", new TraitControl(User, TraitCategoryType.Trap, viewModel) { IsReadOnly = readOnly });
            tabTrap.AddTabItem("Notes", new NotesControl(User, TraitCategoryType.Trap, viewModel) { IsReadOnly = readOnly });
            tabTrap.AddTabItem("Ownership", new OwnershipDetails(model));
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            RegisterUniquePendingChange(new UpdateTrapCommand((viewmodel as TrapViewModel).Model));
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(TrapDetails), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (TrapDetails)obj;
            if (control != null) {
                var readOnly = (bool)args.NewValue;
                control.SetReadOnlyRecursive(readOnly);
            }
        }


    }
}
