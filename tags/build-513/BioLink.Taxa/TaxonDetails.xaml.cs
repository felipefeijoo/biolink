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
using System.Reflection;
using BioLink.Data;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for TaxonDetails.xaml
    /// </summary>
    public partial class TaxonDetails : DatabaseCommandControl {

        private Action<TaxonViewModel> _committedAction;

        #region designer constructor
        public TaxonDetails() {
            InitializeComponent();
        }
        #endregion

        public TaxonDetails(TaxaPlugin plugin, TaxonViewModel taxon, User user, Action<TaxonViewModel> committedAction, bool readOnly) : base(user, "TaxonDetails::" + taxon.TaxaID.Value) {
            InitializeComponent();

            this.Plugin = plugin;
            _committedAction = committedAction;

            tabControl.AddTabItem("General", new TaxonNameDetails(taxon.TaxaID, User, committedAction) { IsReadOnly = readOnly });

            if (taxon.IsAvailableOrLiteratureName) {
                TaxonRank rank = Service.GetTaxonRank(taxon.Taxon);
                switch (rank.Category.ToLower()) {
                    case "g":
                        tabControl.AddTabItem("Available Name", new GenusAvailableNameControl(taxon, user) { IsReadOnly = readOnly });
                        break;
                    case "s":
                        tabControl.AddTabItem("Available Name", new SpeciesAvailableNameControl(taxon, user) { IsReadOnly = readOnly });
                        break;
                    default:
                        tabControl.AddTabItem("Available Name", new AvailableNameControl(taxon, user) { IsReadOnly = readOnly });
                        break;
                }
            } else {
                tabControl.AddTabItem("Common Names", new CommonNamesControl(taxon, user) { IsReadOnly = readOnly });
            }

            tabControl.AddTabItem("References", new ReferencesControl(user, TraitCategoryType.Taxon, taxon.TaxaID) { IsReadOnly = readOnly });

            if ((!taxon.AvailableName.ValueOrFalse() && !taxon.LiteratureName.ValueOrFalse())) {
                tabControl.AddTabItem("Distribution", new DistributionControl(Plugin, user, taxon) { IsReadOnly = readOnly });
            }

            tabControl.AddTabItem("Multimedia", new MultimediaControl(User, TraitCategoryType.Taxon, taxon) { IsReadOnly = readOnly });
            tabControl.AddTabItem("Associates", new OneToManyControl(new AssociatesOneToManyController(User, TraitCategoryType.Taxon, taxon)) { IsReadOnly = readOnly });
            tabControl.AddTabItem("Traits", new TraitControl(User, TraitCategoryType.Taxon, taxon) { IsReadOnly = readOnly });
            tabControl.AddTabItem("Notes", new NotesControl(User, TraitCategoryType.Taxon, taxon) { IsReadOnly = readOnly });
            tabControl.AddTabItem("Ownership", new OwnershipDetails(taxon.Taxon));

            this.Taxon = taxon;

            this.ChangesCommitted += new PendingChangesCommittedHandler(TaxonDetails_ChangesCommitted);

        }

        void TaxonDetails_ChangesCommitted(object sender) {
            if (_committedAction != null) {
                _committedAction(Taxon);
            }
        }

        public override void Dispose() {
            foreach (TabItem item in tabControl.Items) {
                if (item.Content is IDisposable) {
                    (item.Content as IDisposable).Dispose();
                }
            }

            base.Dispose();
        }

        #region properties

        public TaxonViewModel Taxon { get; private set; }

        public TaxaService Service { get { return new TaxaService(User); } }

        public TaxaPlugin Plugin { get; private set; }

        #endregion

    }



}
