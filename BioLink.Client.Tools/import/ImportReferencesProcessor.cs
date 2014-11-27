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

namespace BioLink.Client.Tools {

    public class ImportReferencesProcessor : ImportProcessor {

        protected override void InitImportImpl() {
        }

        protected override void RollbackRow(int rowId) {
        } 

        protected override void ImportRowImpl(int rowId, System.Data.Common.DbConnection connection) {

            var service = new ImportService(User);

            string strJAbbrev = Get("References.Journal Abbreviated Name");
            string strJName = Get("References.Journal name");
            string strBookTitle = Get("References.Book title");
            string strPublisher = Get("References.Book Publisher");
            string strPlacePublished = Get("References.Book Place of publication");

            string strRefType = null;

            if (!string.IsNullOrWhiteSpace(strJAbbrev) || !string.IsNullOrWhiteSpace(strJName)) {
                if (!string.IsNullOrWhiteSpace(strBookTitle)) {
                    strRefType = "JS";
                } else {
                    strRefType = "J";
                }
            } else if (!string.IsNullOrWhiteSpace(strPublisher) || !string.IsNullOrWhiteSpace(strPlacePublished)) {
                if (!string.IsNullOrWhiteSpace(strBookTitle)) {
                    strRefType = "BS";
                } else {
                    strRefType = "B";
                }

            } else {
                strRefType = "M";
            }

            int? startPage;
            int? endPage;

            ParserStartEndPages(Get("References.Total pages"), out startPage, out endPage);


            var r = new ReferenceImport {
                RefCode = Get("References.ReferenceCode"), 
                Author = Get("References.Author(s)"), 
                Title = Get("References.Article title"), 
                BookTitle = strBookTitle,
                Editor = Get("References.Editor"),
                RefType = strRefType,
                YearOfPub = Get("References.Year of publication"),
                ActualDate = Get("References.Actual publication date"),
                JournalAbbrevName = strJAbbrev, 
                JournalFullName = strJName,
                PartNo = Get("References.Part Number"),
                Series = Get("References.Series Number"),
                Publisher = strPublisher, 
                Place = strPlacePublished,
                Volume = Get("References.Volume"),
                Pages = Get("References.Pages"), 
                TotalPages = Get("References.Total pages"),
                Possess = Get("References.Possession"),
                Source = Get("References.Source"),
                Edition = Get("References.Edition"),
                ISBN = Get("References.ISBN"),
                ISSN = Get("References.ISSN"),
                Abstract = Get("References.Qualification"), 
                DateQual = Get("References.Qualification-date"),
                StartPage = startPage, 
                EndPage = endPage
            };

            r.RefID = Service.ImportReference(r);
        }

        private void ParserStartEndPages(string pages, out int? startPage, out int? endPage) {
           
            startPage = null;
            endPage = null;
            if (string.IsNullOrWhiteSpace(pages)) {
                return;
            }

            StringBuilder b = new StringBuilder();
            foreach (char ch in pages) {
                if (Char.IsDigit(ch)) {
                    b.Append(ch);
                } else if (ch == '-' || ch == 150) { // em dash?
                    b.Append(" ");
                }
            }

            string[] bits = b.ToString().Split(' ');
            if (bits.Length == 1) {
                TryParse(bits[0], endPage);
            } else if (bits.Length > 1) {
                TryParse(bits[0], startPage);
                TryParse(bits[1], endPage);
            }

        }

        public bool TryParse(string str, int? val) {
            int i;
            if (Int32.TryParse(str, out i)) {
                val = i;
                return true;
            } else {
                val = null;
            }
            return false;
        }

    }
}
