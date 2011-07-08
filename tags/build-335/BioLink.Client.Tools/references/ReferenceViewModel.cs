using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;


namespace BioLink.Client.Tools {

    /// <summary>
    /// Although references are not hierarchial the view model is because it is used by the generic favorites control.
    /// </summary>
    public class ReferenceViewModel : GenericViewModelBase<Reference> {

        public ReferenceViewModel(Reference model) : base(model, ()=>model.RefID) { }

        protected override string RelativeImagePath {
            get {
                return @"images\Reference.png";
            }
        }

        public override System.Windows.FrameworkElement TooltipContent {

            get {
                return new ReferenceTooltipContent(RefID, this);
            }

        }

        public override string DisplayLabel {
            get {
                return RefCode;
            }
        }

        public int RefID {
            get { return Model.RefID; }
            set { SetProperty(() => Model.RefID, value); }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); }                
        }

        public string Author {
            get { return Model.Author; }
            set { SetProperty(()=>Model.Author, value); }
        }

        public string Title {
            get { return Model.Title; }
            set { SetProperty(() => Model.Title, value); }
        }

        public string BookTitle {
            get { return Model.BookTitle; }
            set { SetProperty(() => Model.BookTitle, value); }
        }

        public string Editor {
            get { return Model.Editor; }
            set { SetProperty(() => Model.Editor, value); }
        }

        public string RefType {
            get { return Model.RefType; }
            set { SetProperty(() => Model.RefType, value); }
        }

        public string YearOfPub {
            get { return Model.YearOfPub; }
            set { SetProperty(() => Model.YearOfPub, value); }
        }

        public string ActualDate {
            get { return Model.ActualDate; }
            set { SetProperty(() => Model.ActualDate, value); }
        }

        public int? JournalID {
            get { return Model.JournalID; }
            set { SetProperty(() => Model.JournalID, value); }
        }

        public string PartNo {
            get { return Model.PartNo; }
            set { SetProperty(() => Model.PartNo, value); }
        }

        public string Series {
            get { return Model.Series; }
            set { SetProperty(() => Model.Series, value); }
        }

        public string Publisher {
            get { return Model.Publisher; }
            set { SetProperty(() => Model.Publisher, value); }
        }

        public string Place {
            get { return Model.Place; }
            set { SetProperty(() => Model.Place, value); }
        }

        public string Volume {
            get { return Model.Volume; }
            set { SetProperty(() => Model.Volume, value); }
        }

        public string Pages {
            get { return Model.Pages; }
            set { SetProperty(() => Model.Pages, value); }
        }

        public string TotalPages {
            get { return Model.TotalPages; }
            set { SetProperty(() => Model.TotalPages, value); }
        }

        public string Possess {
            get { return Model.Possess; }
            set { SetProperty(() => Model.Possess, value); }
        }

        public string Source {
            get { return Model.Source; }
            set { SetProperty(() => Model.Source, value); }
        }

        public string Edition {
            get { return Model.Edition; }
            set { SetProperty(() => Model.Edition, value); }
        }

        public string ISBN {
            get { return Model.ISBN; }
            set { SetProperty(() => Model.ISBN, value); }
        }

        public string ISSN {
            get { return Model.ISSN; }
            set { SetProperty(() => Model.ISSN, value); }
        }

        public string Abstract {
            get { return Model.Abstract; }
            set { SetProperty(() => Model.Abstract, value); }
        }

        public string FullText {
            get { return Model.FullText; }
            set { SetProperty(() => Model.FullText, value); }
        }

        public string FullRTF {
            get { return Model.FullRTF; }
            set { SetProperty(() => Model.FullRTF, value); }
        }

        public int? StartPage {
            get { return Model.StartPage; }
            set { SetProperty(() => Model.StartPage, value); }
        }

        public int? EndPage {
            get { return Model.EndPage; }
            set { SetProperty(() => Model.EndPage, value); }
        }

        public string JournalName {
            get { return Model.JournalName; }
            set { SetProperty(() => Model.JournalName, value); }
        }
        
    }

    public class ReferenceTooltipContent : TooltipContentBase {

        public ReferenceTooltipContent(int refId, ViewModelBase model) : base(refId, model) {
        }

        protected override void GetDetailText(OwnedDataObject model, TextTableBuilder builder) {
            var refmodel = model as Reference;

            builder.Add("Type", refmodel.RefType);
            builder.Add("Title", RTFUtils.StripMarkup(refmodel.Title));
            builder.Add("Author", refmodel.Author);
            builder.Add("Year", refmodel.YearOfPub);
                        
        }

        protected override OwnedDataObject GetModel() {
            var service = new SupportService(User);
            return service.GetReference(ObjectID);
        }
    }

}
