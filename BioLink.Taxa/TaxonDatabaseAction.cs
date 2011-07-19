using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;

namespace BioLink.Client.Taxa {

    public abstract class TaxonDatabaseCommand : DatabaseCommand {
    }

    public class MoveTaxonDatabaseCommand : TaxonDatabaseCommand {

        public MoveTaxonDatabaseCommand(TaxonViewModel taxon, TaxonViewModel newParent) {
            this.Taxon = taxon;
            this.NewParent = newParent;
        }

        public TaxonViewModel Taxon { get; private set; }
        public TaxonViewModel NewParent { get; private set; }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.MoveTaxon(Taxon.TaxaID.Value, NewParent.TaxaID.Value);
        }

        public override string ToString() {
            return String.Format("Move: {0} to {1}", Taxon, NewParent);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_EXPLORER, PERMISSION_MASK.ALLOW);
        }

    }

    public class UpdateTaxonCommand : TaxonDatabaseCommand {

        public UpdateTaxonCommand(Taxon taxon) {
            this.Taxon = taxon;
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
        }

        public Taxon Taxon { get; private set; }

        public override List<string> Validate() {
            var list = new List<string>();

            if (string.IsNullOrEmpty(Taxon.Epithet)) {
                list.Add("The name must not be blank");
            } else {
                if (Taxon.Epithet.Contains(" ") && !(Taxon.AvailableName.ValueOrFalse() || Taxon.LiteratureName.ValueOrFalse())) {
                    list.Add("The name must be only one word.");
                }
            }

            if (Taxon.Unverified.ValueOrFalse() || Taxon.LiteratureName.ValueOrFalse()) {
                if (!string.IsNullOrEmpty(Taxon.YearOfPub)) {
                    int year;
                    if (Int32.TryParse(Taxon.YearOfPub, out year)) {
                        if (year < 1700 || year > 4000) {
                            list.Add("The year must be between the years 1700 and 4000");
                        }
                    }
                }                
            }

            return list;
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.UpdateTaxon(Taxon);
        }

        public override string ToString() {
            return String.Format("Update: {0}", Taxon);
        }

        public override bool Equals(object obj) {
            UpdateTaxonCommand other = obj as UpdateTaxonCommand;
            if (other != null) {
                return other.Taxon.TaxaID == this.Taxon.TaxaID;
            }
            return false;
        }

        public override int GetHashCode() {
            return Taxon.GetHashCode();
        }

    }

    public class MergeTaxonDatabaseCommand : TaxonDatabaseCommand {

        public MergeTaxonDatabaseCommand(TaxonViewModel source, TaxonViewModel target, bool createNewIDRecord) {
            this.Source = source;
            this.Target = target;
            this.CreateNewIDRecord = createNewIDRecord;
        }

        public TaxonViewModel Source { get; private set; }
        public TaxonViewModel Target { get; private set; }
        public bool CreateNewIDRecord { get; private set; }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.MergeTaxon(Source.TaxaID.Value, Target.TaxaID.Value, CreateNewIDRecord);
            service.DeleteTaxon(Source.TaxaID.Value);
        }

        public override string ToString() {
            return String.Format("Merging: {0} with {1}", Source, Target);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_EXPLORER, PERMISSION_MASK.ALLOW);
        }

    }

    public class DeleteTaxonDatabaseCommand : TaxonDatabaseCommand {
        
        public DeleteTaxonDatabaseCommand(TaxonViewModel taxon) {
            this.Taxon = taxon;
        }

        public TaxonViewModel Taxon { get; private set; }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.DeleteTaxon(Taxon.TaxaID.Value);
        }

        public override string ToString() {
            return String.Format("Deleting: {0} ", Taxon);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.DELETE);
        }

    }

    public class InsertTaxonDatabaseCommand : TaxonDatabaseCommand {

        public InsertTaxonDatabaseCommand(TaxonViewModel taxon) {
            this.Taxon = taxon;
        }

        public TaxonViewModel Taxon { get; private set; }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.InsertTaxon(Taxon.Taxon);
            // The service will have updated the new taxon with its database identity.
            // If this taxon has any children we can update their identity too.
            foreach (HierarchicalViewModelBase child in Taxon.Children) {
                TaxonViewModel tvm = child as TaxonViewModel;
                tvm.TaxaParentID = Taxon.Taxon.TaxaID;
            }
        }

        public override string ToString() {
            return String.Format("Inserting: {0}", Taxon);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.INSERT);
        }

    }

    
}
