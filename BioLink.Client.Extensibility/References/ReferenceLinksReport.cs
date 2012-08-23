using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public abstract class ReferenceLinksReport : ReportBase {

        public ReferenceLinksReport(User user)
            : base(user) {
            RegisterViewer(new ReferenceBibliographyViewerSource());
            RegisterViewer(new TabularDataViewerSource());

            Options = new ReferencesReportOptions();
        }

        public override string Name {
            get { return "References Report"; }
        }

        public override bool DisplayOptions(User user, System.Windows.Window parentWindow) {
            if (Options != null) {
                if (Options.Owner == null) {
                    Options.Owner = parentWindow;
                }
                return Options.ShowDialog() == true;
            }
            return false;
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            var matrix = new DataMatrix();

            var service = new SupportService(User);

            matrix.Columns.Add(new MatrixColumn { Name = IntraCategoryIdColumnName, IsHidden = true });
            matrix.Columns.Add(new MatrixColumn { Name = "RefID", IsHidden = true });
            matrix.Columns.Add(new MatrixColumn { Name = "RefCode" });
            matrix.Columns.Add(new MatrixColumn { Name = "RefType" });
            matrix.Columns.Add(new MatrixColumn { Name = "LinkPage" });
            matrix.Columns.Add(new MatrixColumn { Name = "LinkQualification" });
            matrix.Columns.Add(new MatrixColumn { Name = "LinkQualificationRTF", IsHidden = true });

            matrix.Columns.Add(new MatrixColumn { Name = "Title" });
            matrix.Columns.Add(new MatrixColumn { Name = "Author" });
            matrix.Columns.Add(new MatrixColumn { Name = "BookTitle" });
            matrix.Columns.Add(new MatrixColumn { Name = "Edition" });
            matrix.Columns.Add(new MatrixColumn { Name = "Editor" });
            matrix.Columns.Add(new MatrixColumn { Name = "StartPage" });
            matrix.Columns.Add(new MatrixColumn { Name = "EndPage" });
            matrix.Columns.Add(new MatrixColumn { Name = "ActualDate" });
            matrix.Columns.Add(new MatrixColumn { Name = "ISBN" });
            matrix.Columns.Add(new MatrixColumn { Name = "ISSN" });
            matrix.Columns.Add(new MatrixColumn { Name = "JournalID", IsHidden = true });
            matrix.Columns.Add(new MatrixColumn { Name = "PartNo" });
            matrix.Columns.Add(new MatrixColumn { Name = "Place" });
            matrix.Columns.Add(new MatrixColumn { Name = "Possess" });
            matrix.Columns.Add(new MatrixColumn { Name = "Publisher" });
            matrix.Columns.Add(new MatrixColumn { Name = "RefType" });
            matrix.Columns.Add(new MatrixColumn { Name = "Series" });
            matrix.Columns.Add(new MatrixColumn { Name = "Source" });
            matrix.Columns.Add(new MatrixColumn { Name = "FullText" });
            matrix.Columns.Add(new MatrixColumn { Name = "FullRTF", IsHidden = true });
            matrix.Columns.Add(new MatrixColumn { Name = "JournalAbbrevName" });
            matrix.Columns.Add(new MatrixColumn { Name = "JournalAbbrevName2" });
            matrix.Columns.Add(new MatrixColumn { Name = "JournalAlias" });
            matrix.Columns.Add(new MatrixColumn { Name = "JournalFullName" });
            matrix.Columns.Add(new MatrixColumn { Name = "JournalNotes" });

            var reflinks = SelectReferences(progress);

            progress.ProgressMessage("Preparing view model...");
            foreach (RefLink link in reflinks) {

                if (Options.HonourIncludeInReportsFlag) {
                    if (!link.UseInReport.HasValue || !link.UseInReport.Value) {
                        // skip this one as it hasn't got the use in reports flag set.
                        continue;
                    }
                }

                var reference = service.GetReference(link.RefID);
                if (reference != null) {
                    int i = 0;
                    var row = matrix.AddRow();
                    row[i++] = link.IntraCatID.Value;
                    row[i++] = link.RefID;
                    row[i++] = reference.RefCode;
                    row[i++] = link.RefLinkType;
                    row[i++] = link.RefPage;
                    row[i++] = RTFUtils.StripMarkup(link.RefQual);
                    row[i++] = link.RefQual;


                    row[i++] = RTFUtils.StripMarkup(reference.Title);
                    row[i++] = reference.Author;
                    row[i++] = RTFUtils.StripMarkup(reference.BookTitle);
                    row[i++] = reference.Edition;
                    row[i++] = reference.Editor;
                    row[i++] = reference.StartPage;
                    row[i++] = reference.EndPage;
                    row[i++] = SupportService.FormatDate(reference.ActualDate, "yyyy-MM-dd");
                    row[i++] = reference.ISBN;
                    row[i++] = reference.ISSN;
                    row[i++] = reference.JournalID;
                    row[i++] = reference.PartNo;
                    row[i++] = reference.Place;
                    row[i++] = reference.Possess;
                    row[i++] = reference.Publisher;
                    row[i++] = reference.RefType;
                    row[i++] = reference.Series;
                    row[i++] = reference.Source;
                    row[i++] = reference.FullText;
                    row[i++] = reference.FullRTF;

                    if (reference.JournalID.HasValue && reference.JournalID.Value > 0) {
                        var journal = service.GetJournal(reference.JournalID.Value);
                        row[i++] = journal.AbbrevName;
                        row[i++] = journal.AbbrevName2;
                        row[i++] = journal.Alias;
                        row[i++] = journal.FullName;
                        row[i++] = journal.Notes;
                    }
                }
            }

            progress.ProgressMessage("");

            return matrix;
        }

        public ReferencesReportOptions Options { get; private set; }

        protected abstract List<RefLink> SelectReferences(IProgressObserver progress);

        public abstract String IntraCategoryIdColumnName { get; }

    }
}
