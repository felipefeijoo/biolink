using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq; 
using System.Text;
using System.Text.RegularExpressions;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace BioLink.Client.Tools {

    public class OpenXMLLoanFormGenerator : AbstractLoanFormGenerator {

        public const string WORDMLNS = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        private enum MergeStatus { None, HaveOld, HaveNew, HaveBegin, HaveName, HaveSeperator, HaveData };

        protected override string NewLineSequence {
            get { return "\\par " ; }
        }

        public override FileInfo GenerateLoanForm(Multimedia template, Loan loan, List<LoanMaterial> material, List<Trait> traits, Contact originator, Contact requestor, Contact receiver) {
            
            var tempFile = TempFileManager.NewTempFilename(".docx");
            var bytes = SupportService.GetMultimediaBytes(template.MultimediaID);

            Func<String, String> fieldDataFunc = (String fieldName) => {
                return base.SubstitutePlaceHolder(NormaliseFieldName(fieldName), loan, material, traits, originator, requestor, receiver);
            };
                                
            Merge(fieldDataFunc, bytes, tempFile);
            return new FileInfo(tempFile);
        }

        private String NormaliseFieldName(String str) {
            if (str.Contains("\\*")) {
                str = str.Substring(0, str.IndexOf("\\*"));
            }
            return str.Trim();
        }

        public XNamespace XMLNS { get { return XNamespace.Get(WORDMLNS); } }

        public XElement MergeReplace(XElement newBody, Func<String, String> fieldValueFunc) {
            // Get all Mail Merge Fields 
            IList<XElement> mailMergeFields = (from el in newBody.Descendants()
                 where (el.Name == (XMLNS + "r") || el.Attribute(XMLNS + "instr") != null)
                 select el).ToList();

            MergeStatus status = MergeStatus.None;
            XElement status0 = null, status1 = null, status2 = null, status3 = null, status4 = null;
            string fieldName = "";
            // Replace all merge fields with Data 
            foreach (XElement field in mailMergeFields) {
                if ((status == MergeStatus.None) &&
                    (field.Element(XMLNS + "fldChar") != null) &&
                    (field.Element(XMLNS + "fldChar").Attribute(XMLNS + "fldCharType") != null) &&
                    (field.Element(XMLNS + "fldChar").Attribute(XMLNS + "fldCharType").Value == "begin")) {
                    status = MergeStatus.HaveBegin;
                    status0 = field;
                } else if ((status == MergeStatus.None) &&
                      (field.Attribute(XMLNS + "instr") != null)) {
                    status = MergeStatus.HaveOld;
                    status0 = field;
                    fieldName = GetFieldData(field.Attribute(XMLNS + "instr").Value.Replace("MERGEFIELD", string.Empty).Trim());
                } else if ((status == MergeStatus.HaveBegin) &&
                      (field.Element(XMLNS + "instrText") != null) &&
                      (field.Element(XMLNS + "instrText").Value != null) &&
                      (field.Element(XMLNS + "instrText").Value.Trim().Length > 10) &&
                      (field.Element(XMLNS + "instrText").Value.Trim().Substring(0, 10) == "MERGEFIELD")) {
                    status = MergeStatus.HaveName;
                    status1 = field;
                    fieldName = GetFieldData(field.Element(XMLNS + "instrText").Value.Replace("MERGEFIELD", string.Empty).Trim());
                } else if ((status == MergeStatus.HaveName) &&
                      (field.Element(XMLNS + "fldChar") != null) &&
                      (field.Element(XMLNS + "fldChar").Value != null) &&
                      (field.Element(XMLNS + "fldChar").Attribute(XMLNS + "fldCharType").Value != "") &&
                      (field.Element(XMLNS + "fldChar").Attribute(XMLNS + "fldCharType").Value.Trim() == "separate")) {
                    status = MergeStatus.HaveSeperator;
                    status2 = field;
                } else if ((status == MergeStatus.HaveSeperator) &&
                      (field.Element(XMLNS + "rPr") != null) &&
                      (field.Element(XMLNS + "rPr").NextNode != null) &&
                      (((field.Element(XMLNS + "rPr").NextNode as XElement).Value != null))) {
                    status = MergeStatus.HaveData;
                    status3 = field;
                } else if ((status == MergeStatus.HaveData) &&
                      (field.Element(XMLNS + "fldChar") == null) &&
                      (field.Element(XMLNS + "rPr") != null) &&
                      (status3 != null)) {
                    status3.Element(XMLNS + "rPr").Add(field.Element(XMLNS + "rPr").Nodes());
                    field.Remove();
                } else if ((status == MergeStatus.HaveData) &&
                      (field.Element(XMLNS + "fldChar") != null) &&
                      (field.Element(XMLNS + "fldChar").Attribute(XMLNS + "fldCharType") != null) &&
                      (field.Element(XMLNS + "fldChar").Attribute(XMLNS + "fldCharType").Value == "end")) {
                    status = MergeStatus.HaveNew;
                    status4 = field;
                } else {
                    status = MergeStatus.None;
                    status0 = null;
                    status1 = null;
                    status2 = null;
                    status3 = null;
                    status4 = null;
                    fieldName = "";
                }

                if (status == MergeStatus.HaveOld || status == MergeStatus.HaveNew) {
                    var value = fieldValueFunc(fieldName);
                    if (value != null) {
                        XElement newElement = null, newElement2 = null;
                        if (status == MergeStatus.HaveOld) {
                            newElement = field.Descendants(XMLNS + "r").First();
                            if (value.Contains(NewLineSequence)) {
                                var bits = value.Split(new String[] { NewLineSequence }, StringSplitOptions.None);
                                foreach (String bit in bits) {
                                    newElement.AddBeforeSelf(new XElement(XMLNS + "t", bit));
                                    newElement.AddBeforeSelf(new XElement(XMLNS + "br", bit));
                                }                                                                
                            } else {
                                newElement.Descendants(XMLNS + "t").First().Value = value;
                            }
                        } else {
                            newElement = new XElement(XMLNS + "fldSimple");
                            newElement.SetAttributeValue(XMLNS + "instr", "MERGEFIELD " + fieldName);
                            if (value.Contains(NewLineSequence)) {
                                var bits = value.Split(new String[] { NewLineSequence }, StringSplitOptions.None);
                                foreach (String bit in bits) {
                                    newElement.Add(new XElement(XMLNS + "t", bit));
                                    newElement.Add(new XElement(XMLNS + "br", bit));
                                }
                            } else {
                                newElement2 = new XElement(XMLNS + "t", value);
                                newElement.Add(newElement2);
                            }
                        }
                        if (status == MergeStatus.HaveOld) {
                            status0.ReplaceWith(newElement);
                        } else {
                            status3.Element(XMLNS + "rPr").NextNode.ReplaceWith(newElement);
                            status0.Remove();
                            status1.Remove();
                            status2.Remove();
                            status4.Remove();
                        }
                    }
                    status = MergeStatus.None;
                }
            }
            return newBody;
        }

        private string GetFieldData(string text) {
            string newText = text;
            if (newText.Length > 0 && newText.Substring(0, 1) == "\"") {
                newText = newText.Substring(1);
                if (newText.Length > 1 && newText.Substring(newText.Length - 1, 1) == "\"") {
                    newText = newText.Substring(0, newText.Length - 1);
                }
            }

            return newText;
        }

        private void Merge(Func<String, String> fieldDataFunc, byte[] sourceBytes, String targetFileName) {
            // Open Template            
            using (MemoryStream _workingMemoryStream = new MemoryStream()) {
                // Load into memory 
                _workingMemoryStream.Write(sourceBytes, 0, sourceBytes.Length);

                using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(_workingMemoryStream, true)) {
                    XElement newBody = XElement.Parse(wordDocument.MainDocumentPart.Document.Body.OuterXml);
                    newBody = MergeReplace(newBody, fieldDataFunc);
                    wordDocument.MainDocumentPart.Document.Body = new Body(newBody.ToString());
                    wordDocument.MainDocumentPart.Document.Save();

                    foreach (HeaderPart x in wordDocument.MainDocumentPart.HeaderParts) {
                        newBody = XElement.Parse(x.Header.OuterXml);
                        newBody = MergeReplace(newBody, fieldDataFunc);
                        x.Header = new Header(newBody.ToString());
                        x.Header.Save();
                    }

                    foreach (FooterPart x in wordDocument.MainDocumentPart.FooterParts) {
                        newBody = XElement.Parse(x.Footer.OuterXml);
                        newBody = MergeReplace(newBody, fieldDataFunc);
                        x.Footer = new Footer(newBody.ToString());
                        x.Footer.Save();
                    }

                    XElement settings = null;
                    DocumentSettingsPart settingsPart = null;
                    if (wordDocument.MainDocumentPart.GetPartsOfType<DocumentSettingsPart>().Count() > 0) {
                        settingsPart = wordDocument.MainDocumentPart.GetPartsOfType<DocumentSettingsPart>().First();
                        if (settingsPart != null) {
                            if (settingsPart.GetPartsOfType<MailMergeRecipientDataPart>().Count() > 0) {
                                MailMergeRecipientDataPart mmrPart = settingsPart.GetPartsOfType<MailMergeRecipientDataPart>().First();
                                settingsPart.DeletePart(mmrPart);
                                // Delete refrence to Mail Merge Data sources 
                                settings = XElement.Parse(settingsPart.RootElement.OuterXml);
                            }
                        }
                    }

                    if (settings != null) {
                        IList<XElement> mailMergeElements =
                           (from el in settings.Descendants()
                            where el.Name == (XMLNS + "mailMerge")
                            select el).ToList();

                        foreach (XElement field in mailMergeElements) {
                            field.Remove();
                        }
                    }

                    if (settingsPart != null && settings != null) {
                        settingsPart.RootElement.InnerXml = settings.ToString();
                        settingsPart.RootElement.Save();
                    }

                    // Save in output directory 
                    // Create a new document based on updated template 
                    using (FileStream fileStream = new FileStream(targetFileName, FileMode.Create)) {
                        _workingMemoryStream.WriteTo(fileStream);
                    } 
                }
            }
        }

    }

}
