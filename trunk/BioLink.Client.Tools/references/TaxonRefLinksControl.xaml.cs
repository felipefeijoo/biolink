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
    /// Interaction logic for TaxonRefLinksControl.xaml
    /// </summary>
    public partial class TaxonRefLinksControl : OneToManyControllerEditor {

        public TaxonRefLinksControl(User user, int referenceID) : base(user) {
            InitializeComponent();
            txtRefType.BindUser(User, PickListType.RefLinkType, "", TraitCategoryType.Taxon);
            txtTaxon.BindUser(User, LookupType.Taxon);
            this.ReferenceID = referenceID;
        }

        public override ViewModelBase AddNewItem(out DatabaseCommand addAction) {
            var model = new TaxonRefLink();
            model.RefLinkID = -1;
            model.RefID = ReferenceID;
            model.RefLink = "<New Taxon Link>";
            model.UseInReports = true;
            addAction = new InsertTaxonRefLinkCommand(model);
            return new TaxonRefLinkViewModel(model);
        }

        public override DatabaseCommand PrepareDeleteAction(ViewModelBase viewModel) {
            var link = viewModel as TaxonRefLinkViewModel;
            if (link != null) {
                return new DeleteTaxonRefLinkCommand(link.Model);
            }
            return null;
        }

        public override List<ViewModelBase> LoadModel() {
            var service = new SupportService(User);
            var list = service.GetTaxonRefLinks(ReferenceID);
            return list.ConvertAll((model) => {
                return (ViewModelBase)new TaxonRefLinkViewModel(model);
            });
        }

        public override DatabaseCommand PrepareUpdateAction(ViewModelBase viewModel) {
            var link = viewModel as TaxonRefLinkViewModel;
            if (link != null) {
                return new UpdateTaxonRefLinkCommand(link.Model);
            }
            return null;
        }

        #region Properties

        public int ReferenceID { get; private set;  }

        public override UIElement FirstControl {
            get { return this.txtRefType; }
        }

        #endregion

    }
}
