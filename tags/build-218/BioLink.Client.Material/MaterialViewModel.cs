using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class MaterialViewModel : GenericViewModelBase<BioLink.Data.Model.Material> {

        public MaterialViewModel(BioLink.Data.Model.Material model) : base(model, ()=>model.MaterialID) { }

        public int MaterialID {
            get { return Model.MaterialID; }
            set { SetProperty(() => Model.MaterialID, value); }
        }

        public bool IsTemplate {
            get { return Model.IsTemplate; }
            set { SetProperty(() => Model.IsTemplate, value); }
        }

        public string MaterialName {
            get { return Model.MaterialName; }
            set { SetProperty(() => Model.MaterialName, value); }
        }

        public int SiteVisitID {
            get { return Model.SiteVisitID; }
            set { SetProperty(() => Model.SiteVisitID, value); }
        }

        public string AccessionNumber {
            get { return Model.AccessionNumber; }
            set { SetProperty(() => Model.AccessionNumber, value); }
        }

        public string RegistrationNumber {
            get { return Model.RegistrationNumber; }
            set { SetProperty(() => Model.RegistrationNumber, value); }
        }

        public string CollectorNumber {
            get { return Model.CollectorNumber; }
            set { SetProperty(() => Model.CollectorNumber, value); }
        }

        public int BiotaID {
            get { return Model.BiotaID; }
            set { SetProperty(() => Model.BiotaID, value); }
        }

        public string IdentifiedBy {
            get { return Model.IdentifiedBy; }
            set { SetProperty(() => Model.IdentifiedBy, value); }
        }

        public DateTime? IdentificationDate {
            get { return Model.IdentificationDate; }
            set { SetProperty(() => Model.IdentificationDate, value); }
        }

        public int IdentificationReferenceID {
            get { return Model.IdentificationReferenceID; }
            set { SetProperty(() => Model.IdentificationReferenceID, value); }
        }

        public string IdentificationRefPage {
            get { return Model.IdentificationRefPage; }
            set { SetProperty(() => Model.IdentificationRefPage, value); }
        }

        public string IdentificationMethod {
            get { return Model.IdentificationMethod; }
            set { SetProperty(() => Model.IdentificationMethod, value); }
        }

        public string IdentificationAccuracy {
            get { return Model.IdentificationAccuracy; }
            set { SetProperty(() => Model.IdentificationAccuracy, value); }
        }

        public string IdentificationNameQualification {
            get { return Model.IdentificationNameQualification; }
            set { SetProperty(() => Model.IdentificationNameQualification, value); }
        }

        public string IdentificationNotes {
            get { return Model.IdentificationNotes; }
            set { SetProperty(() => Model.IdentificationNotes, value); }
        }

        public string Institution {
            get { return Model.Institution; }
            set { SetProperty(() => Model.Institution, value); }
        }

        public string CollectionMethod {
            get { return Model.CollectionMethod; }
            set { SetProperty(() => Model.CollectionMethod, value); }
        }

        public string Abundance {
            get { return Model.Abundance; }
            set { SetProperty(() => Model.Abundance, value); }
        }

        public string MacroHabitat {
            get { return Model.MacroHabitat; }
            set { SetProperty(() => Model.MacroHabitat, value); }
        }

        public string MicroHabitat {
            get { return Model.MicroHabitat; }
            set { SetProperty(() => Model.MicroHabitat, value); }
        }

        public string Source {
            get { return Model.Source; }
            set { SetProperty(() => Model.Source, value); }
        }

        public int AssociateOf {
            get { return Model.AssociateOf; }
            set { SetProperty(() => Model.AssociateOf, value); }
        }

        public string SpecialLabel {
            get { return Model.SpecialLabel; }
            set { SetProperty(() => Model.SpecialLabel, value); }
        }

        public string OriginalLabel {
            get { return Model.OriginalLabel; }
            set { SetProperty(() => Model.OriginalLabel, value); }
        }

        public int TrapID {
            get { return Model.TrapID; }
            set { SetProperty(() => Model.TrapID, value); }
        }

        public string TaxaDesc {
            get { return Model.TaxaDesc; }
            set { SetProperty(() => Model.TaxaDesc, value); }
        }

        public int SiteID {
            get { return Model.SiteID; }
            set { SetProperty(() => Model.SiteID, value); }
        }

        public string SiteName {
            get { return Model.SiteName; }
            set { SetProperty(() => Model.SiteName, value); }
        }

        public string SiteVisitName {
            get { return Model.SiteVisitName; }
            set { SetProperty(() => Model.SiteVisitName, value); }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); }
        }

        public string TrapName {
            get { return Model.TrapName; }
            set { SetProperty(() => Model.TrapName, value); }
        }

        public string LoadID {
            get { return Model.LoadID; }
            set { SetProperty(() => Model.LoadID, value); }
        }

        public string TypeData {
            get { return Model.TypeData; }
            set { SetProperty(() => Model.TypeData, value); }
        }

    }
}
