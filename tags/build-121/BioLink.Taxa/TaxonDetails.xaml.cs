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
    public partial class TaxonDetails : DatabaseActionControl {

        #region designer constructor
        public TaxonDetails() {
            InitializeComponent();
        }
        #endregion

        public TaxonDetails(TaxonViewModel taxon, User user) : base(user, "TaxonDetails::" + taxon.TaxaID.Value) {
            InitializeComponent();           

            tabControl.AddTabItem("General", new TaxonNameDetails(taxon.TaxaID, User));

            if (taxon.IsAvailableOrLiteratureName) {
                TaxonRank rank = Service.GetTaxonRank(taxon.ElemType);
                switch (rank.Category.ToLower()) {
                    case "g":
                        tabControl.AddTabItem("Available Name", new GenusAvailableNameControl(taxon, user));
                        break;
                    case "s":
                        tabControl.AddTabItem("Available Name", new SpeciesAvailableNameControl(taxon, user));
                        break;
                    default:
                        tabControl.AddTabItem("Available Name", new AvailableNameControl(taxon, user));
                        break;
                }
            } else {
                tabControl.AddTabItem("Common Names", new CommonNamesControl(taxon, user));
            }

            tabControl.AddTabItem("References", new ReferencesControl(user, TraitCategoryType.Taxon, taxon.TaxaID));

            if ((!taxon.AvailableName.ValueOrFalse() && !taxon.LiteratureName.ValueOrFalse())) {
                tabControl.AddTabItem("Distribution", new DistributionControl(user, taxon));
            }


            tabControl.AddTabItem("Traits", new TraitControl(User, TraitCategoryType.Taxon, taxon.TaxaID));
            var mmc = new MultimediaControl(User, TraitCategoryType.Taxon, taxon.TaxaID);

            tabControl.AddTabItem("Multimedia", mmc, () => {
                if (!mmc.IsPopulated) {
                    mmc.PopulateControl();
                }
            });

            var notes = new NotesControl(User, TraitCategoryType.Taxon, taxon.TaxaID);
            tabControl.AddTabItem("Notes", notes, () => {
                if (!notes.IsPopulated) {
                    notes.PopulateControl();
                }
            });

            tabControl.AddTabItem("Ownership", new OwnershipDetails(taxon.Taxon));

            this.Taxon = taxon;

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

        #endregion

    }



}
