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
            required.AddBiota(Taxon.TaxaID.Value, PERMISSION_MASK.UPDATE);
            required.AddBiota(NewParent.TaxaID.Value, PERMISSION_MASK.UPDATE);
        }

    }

    public class UpdateTaxonCommand : TaxonDatabaseCommand {

        private bool _isNew = false;

        public UpdateTaxonCommand(Taxon taxon) {
            this.Taxon = taxon;
            _isNew = taxon.TaxaID.Value < 0;
        }

        public Taxon Taxon { get; private set; }

        public override void Validate(ValidationMessages messages) {
            var list = new List<string>();

            if (string.IsNullOrEmpty(Taxon.Epithet)) {
                messages.Error("The name must not be blank");
            } else {
                if (Taxon.Epithet.Contains(" ") && !(Taxon.AvailableName.ValueOrFalse() || Taxon.LiteratureName.ValueOrFalse())) {
                    messages.Error("The name must be only one word.");
                }
            }

            if (Taxon.Unverified.ValueOrFalse() || Taxon.LiteratureName.ValueOrFalse()) {
                if (!string.IsNullOrEmpty(Taxon.YearOfPub)) {
                    int year;
                    if (Int32.TryParse(Taxon.YearOfPub, out year)) {
                        if (year < 1700 || year > 4000) {
                            messages.Error("The year must be between the years 1700 and 4000");
                        }
                    }
                }                
            }
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

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
            // don't need biota permission to update a new item...
            if (!_isNew) {
                required.AddBiota(Taxon.TaxaID.Value, PERMISSION_MASK.UPDATE);
            }
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
            required.AddBiota(Source.TaxaID.Value, PERMISSION_MASK.UPDATE);
            required.AddBiota(Target.TaxaID.Value, PERMISSION_MASK.UPDATE);
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
            required.AddBiota(Taxon.TaxaID.Value, PERMISSION_MASK.DELETE);
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
            if (Taxon.Parent != null && Taxon.Parent.ObjectID.HasValue && Taxon.Parent.ObjectID > 0) {
                required.AddBiota(Taxon.Parent.ObjectID.Value, PERMISSION_MASK.INSERT);
            }
        }

    }
    
}
