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
            matrix.Columns.Add(new MatrixColumn { Name = "LinkQualificationRTF", IsHidden=true });

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
                    row[i++] = link.RefCode;
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

    public class TaxonReferencesReport : ReferencesReport {

        public TaxonReferencesReport(User user, Taxon taxon) : base(user) {
            Taxon = taxon;
        }

        public override string Name {
            get { return string.Format("References for '{0}'", Taxon.TaxaFullName); }
        }

        protected override List<RefLink> SelectReferences(IProgressObserver progress) {
            var service = new SupportService(User);
            var taxaService = new TaxaService(User);
            if (progress != null) {
                progress.ProgressMessage("Retrieving Reference links...");
            }
            var reflinks = service.GetReferenceLinks(TraitCategoryType.Taxon.ToString(), Taxon.TaxaID.Value);
            if (Options.IncludeChildReferences == true) {
                var children = taxaService.GetExpandFullTree(Taxon.TaxaID.Value);

                var elementCount = 0;
                int total = children.Count;

                if (progress != null) {
                    progress.ProgressStart("Extracting references for children...");
                }

                foreach (Taxon child in children) {

                    if (progress != null) {
                        double percent = (((double)elementCount) / ((double)total)) * 100.0;
                        progress.ProgressMessage(string.Format("Processing {0}", child.TaxaFullName), percent);
                    }
                    elementCount++;

                    var links = service.GetReferenceLinks(TraitCategoryType.Taxon.ToString(), child.TaxaID.Value);
                    foreach (RefLink link in links) {
                        reflinks.Add(link);
                    }
                }

                if (progress != null) {
                    progress.ProgressEnd("");
                }


            }

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
            rtf.ReportHeading(options.BibliographyTitle);

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
                                    return String.Compare(refType1, refType2, true);
                                }
                            }

                            // then by the nominated sort column
                            var objVal1 = reportData.Rows[idx1][sortColumnIdx];
                            var objVal2 = reportData.Rows[idx2][sortColumnIdx];
                            var val1 = RTFUtils.StripMarkup(objVal1 == null ? "" : objVal1.ToString());
                            var val2 = RTFUtils.StripMarkup(objVal2 == null ? "" : objVal2.ToString());

                            if (options.SortAscending) {
                                return String.Compare((String)val1, (String)val2, true);
                            } else {
                                return String.Compare((String)val2, (String)val1, true);
                            }
                        });


            var lastRefType = "";
            String[] allowedKeywords = { "b", "i", "sub", "super", "strike", "ul", "ulnone", "nosupersub" };
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
                rtf.Append(@" \pard\fs20\f1 ");
                if (options.BibliographyIndexStyle != BibliographyIndexStyle.None) {
                    rtf.Append("[");
                    switch (options.BibliographyIndexStyle) {
                        case BibliographyIndexStyle.Number:
                            rtf.Append(idx);
                            break;
                        case BibliographyIndexStyle.RefCode:
                            rtf.Append(row["RefCode"]);
                            break;
                    }
                    rtf.Append("] ");
                }
                idx++;

                var fullRTF = RTFUtils.filter(row["FullRTF"] as string, true, false, allowedKeywords);
                rtf.Append(fullRTF);

                var bits = new List<String>();                
                if (!String.IsNullOrWhiteSpace(row["LinkPage"] as String)) {
                    bits.Add(String.Format("Page {0}", row["LinkPage"] as String));
                }
                
                if (options.IncludeQualification) {
                    var qual = row["LinkQualificationRTF"] as string;
                    if (!String.IsNullOrEmpty(qual)  ) {
                        bits.Add(RTFUtils.filter(qual, true, true, allowedKeywords).Trim());
                    }
                }

                if (bits.Count > 0) {
                    rtf.Append(" (").Append(bits.Join("; ").Trim()).Append(")");
                }

                rtf.Par();
            }

            Console.WriteLine(rtf.RTF);

            viewer.rtf.Rtf = rtf.RTF;

            return viewer;
        }
    }

}
