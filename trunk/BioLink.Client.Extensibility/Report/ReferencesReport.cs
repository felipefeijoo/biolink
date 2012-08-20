using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public abstract class ReferencesReport : ReportBase {

        public ReferencesReport(User user) : base(user) {
            RegisterViewer(new TabularDataViewerSource());
            RegisterViewer(new  ReferenceBibliographyViewerSource());
            Options = new ReferenceReportOptions { IncludeQualification = true, HonourIncludeInReportsFlag = true };
        }

        public override string Name {
            get { return "References Report"; }
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
            matrix.Columns.Add(new MatrixColumn { Name = "FullRTF" });
            matrix.Columns.Add(new MatrixColumn { Name = "JournalAbbrevName" });
            matrix.Columns.Add(new MatrixColumn { Name = "JournalAbbrevName2" });
            matrix.Columns.Add(new MatrixColumn { Name = "JournalAlias" });
            matrix.Columns.Add(new MatrixColumn { Name = "JournalFullName" });
            matrix.Columns.Add(new MatrixColumn { Name = "JournalNotes" });
            


            var reflinks = SelectReferences();
            foreach (RefLink link in reflinks) {

                if (Options.HonourIncludeInReportsFlag) {
                    if (!link.UseInReport.HasValue || !link.UseInReport.Value) {
                        // skip this one as it hasn't got the use in reports flag set.
                        continue;
                    }
                }

                var reference = service.GetReference(link.RefID);
                int i = 0;
                var row = matrix.AddRow();
                row[i++] = link.IntraCatID.Value;
                row[i++] = link.RefID;
                row[i++] = link.RefCode;
                row[i++] = link.RefLinkType;
                row[i++] = link.RefPage;
                row[i++] = link.RefQual;

                
                row[i++] = reference.Title;
                row[i++] = reference.Author;
                row[i++] = reference.BookTitle;
                row[i++] = reference.Edition;
                row[i++] = reference.Editor;
                row[i++] = reference.StartPage;
                row[i++] = reference.EndPage;
                row[i++] = reference.ActualDate;
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

            return matrix;
        }

        public ReferenceReportOptions Options { get; private set; }

        protected abstract List<RefLink> SelectReferences();

        public abstract String IntraCategoryIdColumnName { get; }        

    }

    public class TaxonReferencesReport : ReferencesReport {

        public TaxonReferencesReport(User user, Taxon taxon) : base(user) {
            Taxon = taxon;
        }

        public override string Name {
            get { return string.Format("References for {0}", Taxon.TaxaFullName); }
        }

        protected override List<RefLink> SelectReferences() {
            var service = new SupportService(User);
            var reflinks = service.GetReferenceLinks(TraitCategoryType.Taxon.ToString(), Taxon.TaxaID.Value);
            return reflinks;
        }

        public Taxon Taxon { get; private set; }

        public override string IntraCategoryIdColumnName {
            get { return "intTaxonID"; }
        }

    }

    public class ReferenceBibliographyViewerSource : IReportViewerSource {

        public string Name {
            get { return "Bibliography"; }
        }

        public System.Windows.FrameworkElement ConstructView(IBioLinkReport report, DataMatrix reportData, IProgressObserver progress) {

            var options = (report as ReferencesReport).Options;

            var viewer = new RTFReportViewer {ReportName = report.Name};
            var rtf = new RTFReportBuilder();
            rtf.AppendFullHeader();
            rtf.ReportHeading("References");

            var idx = 1;
            var colIndex = reportData.IndexOf("RefID");

            var refIds = new List<Int32>();
            for (var i = 0; i < reportData.Rows.Count; ++i ) {
                refIds.Add(i);
            }

            int sortColumnIdx = reportData.IndexOf(options.SortColumn);
            int refTypeIndex = reportData.IndexOf("RefType");
            refIds.Sort((idx1, idx2) => {
                            // If grouping, first check the ref type
                            if (options.GroupByReferenceType) {
                                var refType1 = reportData.Rows[idx1][refTypeIndex] as String;
                                var refType2 = reportData.Rows[idx2][refTypeIndex] as String;
                                if (!refType1.Equals(refType2)) {
                                    return String.CompareOrdinal(refType1, refType2);
                                }
                            }

                            // then by the nominated sort column
                            var objVal1 = reportData.Rows[idx1][sortColumnIdx];
                            var objVal2 = reportData.Rows[idx2][sortColumnIdx];
                            var val1 = objVal1 == null ? "" : objVal1.ToString();
                            var val2 = objVal2 == null ? "" : objVal2.ToString();
                            if (options.SortAscending) {
                                return String.CompareOrdinal((String)val1, (String)val2);
                            } else {
                                return String.CompareOrdinal((String)val2, (String)val1);
                            }
                        });


            var lastRefType = "";
            foreach (var rowIdx in refIds) {
                var row = reportData.Rows[rowIdx];

                if (options.GroupByReferenceType) {
                    var refType = row["RefType"] as String;
                    if (!String.Equals(refType, lastRefType, StringComparison.CurrentCultureIgnoreCase)) {
                        rtf.Par();
                        rtf.Append(@" \pard\fs24\b\f1 ");
                        rtf.Append(refType);
                        rtf.Append(@" \b0");
                        rtf.Par();
                        lastRefType = refType;
                    }
                }

                rtf.Par();
                rtf.Append(@" \pard\fs20\f1 [");
                switch (options.BibliographyIndexStyle) {
                    case BibliographyIndexStyle.Number:
                        rtf.Append(idx);
                        break;
                    case BibliographyIndexStyle.RefCode:
                        rtf.Append(row["RefCode"]);
                        break;
                }
                idx++;
                rtf.Append("] ");
                rtf.Append(row["FullRTF"]);
                if (options.IncludeQualification) {
                    var qual = row["LinkQualification"] as string;
                    if (!String.IsNullOrEmpty(qual)) {
                        rtf.Append(" (");                        
                        rtf.Append(RTFUtils.StripSpecficKeywords(qual, true, "par").Trim());
                        rtf.Append(")");
                    }
                }
                rtf.Par();
            }

            Console.WriteLine(rtf.RTF);

            viewer.rtf.Rtf = rtf.RTF;

            return viewer;
        }
    }

    public class ReferenceReportOptions {

        public ReferenceReportOptions() {
            BibliographyTitle = "References";
            HonourIncludeInReportsFlag = true;
            IncludeQualification = true;
            SortColumn = "RefCode";
            SortAscending = true;
            BibliographyIndexStyle = BibliographyIndexStyle.RefCode;
            GroupByReferenceType = true;
        }

        public string BibliographyTitle { get; set; }
        public bool HonourIncludeInReportsFlag { get; set; }
        public bool IncludeQualification { get; set; }
        public string SortColumn { get; set; }
        public bool SortAscending { get; set; }
        public BibliographyIndexStyle BibliographyIndexStyle { get; set; }
        public bool GroupByReferenceType { get; set; }
    }

    public enum BibliographyIndexStyle {
        Number,
        RefCode
    }

}
