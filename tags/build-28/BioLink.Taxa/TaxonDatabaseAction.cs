using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Taxa {

    public abstract class TaxonDatabaseAction : DatabaseAction<TaxaService> {
    }

    public class MoveTaxonDatabaseAction : TaxonDatabaseAction {

        public MoveTaxonDatabaseAction(int taxonId, int newParentId) {
            this.TaxonId = taxonId;
            this.NewParentId = newParentId;
        }

        public int TaxonId { get; private set; }
        public int NewParentId { get; private set; }

        protected override void ProcessImpl(TaxaService service) {
            service.MoveTaxon(TaxonId, NewParentId);
        }
    }

    public class UpdateTaxonDatabaseAction : TaxonDatabaseAction {

        public UpdateTaxonDatabaseAction(Taxon taxon) {
            this.Taxon = taxon;
        }

        public Taxon Taxon { get; private set; }

        protected override void ProcessImpl(TaxaService service) {
            service.UpdateTaxon(Taxon);
        }

    }

    public class MergeTaxonDatabaseAction : TaxonDatabaseAction {

        public MergeTaxonDatabaseAction(int sourceId, int targetId, bool createNewIDRecord) {
            this.SourceId = sourceId;
            this.TargetId = targetId;
            this.CreateNewIDRecord = createNewIDRecord;
        }

        public int SourceId { get; private set; }
        public int TargetId { get; private set; }
        public bool CreateNewIDRecord { get; private set; }

        protected override void ProcessImpl(TaxaService service) {
            service.MergeTaxon(SourceId, TargetId, CreateNewIDRecord);
            service.DeleteTaxon(SourceId);
        }
    }

    public class DeleteTaxonDatabaseAction : TaxonDatabaseAction {
        
        public DeleteTaxonDatabaseAction(TaxonViewModel taxon) {
            this.Taxon = taxon;
        }

        public TaxonViewModel Taxon { get; private set; }

        protected override void ProcessImpl(TaxaService service) {
            service.DeleteTaxon(Taxon.TaxaID.Value);
        }

    }

    public class InsertTaxonDatabaseAction : TaxonDatabaseAction {

        private TaxonViewModel _taxon;

        public InsertTaxonDatabaseAction(TaxonViewModel taxon) {
            _taxon = taxon;
        }

        protected override void ProcessImpl(TaxaService service) {                           
            service.InsertTaxon(_taxon.Taxon);
            // The service will have updated the new taxon with its database identity.
            // If this taxon has any children we can update their identity too.
            foreach (HierarchicalViewModelBase child in _taxon.Children) {
                TaxonViewModel tvm = child as TaxonViewModel;
                tvm.TaxaParentID = _taxon.Taxon.TaxaID;
            }
        }

    }

    
}
