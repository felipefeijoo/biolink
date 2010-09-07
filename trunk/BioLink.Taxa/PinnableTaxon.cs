using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;


namespace BioLink.Client.Taxa {

    public class PinnableTaxon : GenericPinnable<Taxon> {

        public PinnableTaxon() {
        }

        public PinnableTaxon(int taxonId) {
            this.TaxonID = taxonId;
        }        

        public PinnableTaxon(Taxon taxon) {
        }

        public override ViewModelBase CreateViewModel() {
            
            TaxaService service = new TaxaService(PluginManager.Instance.User);
            Taxon t = service.GetTaxon(TaxonID);
            return new TaxonViewModel(null, t, null);            
        }

        public Int32 TaxonID { get; set; }

    }
}
