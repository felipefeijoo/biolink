using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;


namespace BioLink.Client.Extensibility {

    public class ReferencesReport : GenericModelReport<ReferencesReportViewModel> {

        private static String[] columns = { "RefID[hidden]", "RefCode[Ref Code]", "Author", "YearOfPub[Year]", "PlainTitle[Title]", "BookTitle", "Editor", "RefType", "YearOfPub", "ActualDate","PartNo", "Series", "Publisher", "Place", "Volume", "Pages", "TotalPages", "Possess", "Source", "Edition", "ISBN", "ISSN", "StartPage", "EndPage", "JournalID[hidden]", "JournalAbbrev", "JournalAbbrev2", "JournalFullName", "JournalAlias", "JournalNotes" };

        public ReferencesReport(User user, string reportName, IEnumerable<Int32> refIds) : base(user, reportName, null, columns) {
            ModelGenerator = GenerateModel;
            ReferenceIDs = refIds;
        }

        protected List<ReferencesReportViewModel> GenerateModel() {
            var service = new SupportService(User);
            var model = new List<ReferencesReportViewModel>();
            foreach (Int32 refId in ReferenceIDs) {
                var reference = service.GetReference(refId);
                if (reference != null) {
                    model.Add(new ReferencesReportViewModel(reference));
                }
            }
            return model;            
        }

        protected IEnumerable<Int32> ReferenceIDs { get; private set; }

    }

    public class ReferencesReportViewModel : GenericViewModelBase<Reference> {

        public ReferencesReportViewModel(Reference model) : base(model, () => model.RefID) { }

        private Journal _journal;

        public override string DisplayLabel {
            get { return String.Format("{0}, {1} [{2}] ({3})", RTFUtils.StripMarkup(Title), Author, YearOfPub, RefCode); }
        }


        public int RefID {
            get { return Model.RefID; }
        }

        public string RefCode {
            get { return Model.RefCode; }
        }

        public string Author {
            get { return Model.Author; }
        }

        public string YearOfPub {
            get { return Model.YearOfPub; }
        }

        public string Title {
            get { return Model.Title; }
        }

        public string RefType {
            get { return Model.RefType; }
        }

        public string BookTitle {
            get { return RTFUtils.StripMarkup(Model.BookTitle); }
        }

        public string Editor {
            get { return Model.Editor; }
        }

        public string ActualDate {
            get { return Model.ActualDate; }
        }

        public string PartNo {
            get { return Model.PartNo; }
        }

        public string Series {
            get { return Model.Series; }
        }

        public string Publisher {
            get { return Model.Publisher; }
        }

        public string Place {
            get { return Model.Place; }
        }

        public string TotalPages {
            get { return Model.TotalPages; }
        }

        public string Possess {
            get { return Model.Possess; }
        }

        public string Source {
            get { return Model.Source; }
        }

        public string Edition {
            get { return Model.Edition; }
        }

        public string ISBN {
            get { return Model.ISBN; }
        }

        public string ISSN {
            get { return Model.ISSN; }
        }

        public string Abstract {
            get { return Model.Abstract; }
        }

        public string FullText {
            get { return Model.FullText; }
        }

        public string FullRTF {
            get { return Model.FullRTF; }
        }

        public int? StartPage {
            get { return Model.StartPage; }
        }

        public int? EndPage {
            get { return Model.EndPage; }
        }

        public Journal Journal { 
            get {
                if (_journal == null && RefType.Equals("j",StringComparison.CurrentCultureIgnoreCase)) {
                    if (Model.JournalID.HasValue) {
                        var service = new SupportService(PluginManager.Instance.User);
                        _journal = service.GetJournal(Model.JournalID.Value);
                    }
                }
                return _journal;
            }
        }

        public string Pages {
            get { return Model.Pages; }
        }

        public string Volume {
            get { return Model.Volume; }
        }

        public String PlainTitle {
            get { return RTFUtils.StripMarkup(Title); }
        }

        public Int32? JournalID {
            get { return Model.JournalID; }
        }

        public string JournalAbbrevName {
            get { return Journal != null ? Journal.AbbrevName : ""; } 
        }

        public string JournalAbbrevName2 {
            get { return Journal != null ? Journal.AbbrevName2 : ""; } 
        }

        public string JournalAlias {
            get { return Journal != null ? Journal.Alias : ""; } 
        }

        public string JournalFullName {
            get { return Journal != null ? Journal.FullName : ""; } 
        }

        public string JournalNotes {
            get { return Journal != null ? RTFUtils.StripMarkup(Journal.Notes) : ""; } 
        }

    }

}
