using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;

namespace BioLink.Client.Taxa {

    public static class TaxonNameParser {

        public static TaxonName ParseName(TaxonViewModel taxon, string str) {
            

            if (str == null || String.IsNullOrEmpty(str.Trim())) {
                return null;
            }

            str = str.Trim();

            TaxonName name = new TaxonName();
            if (taxon.LiteratureName.ValueOrFalse() || str.IndexOf(' ') < 0) {
                name.Epithet = str.Trim();
                return name;
            }

            str = str.Replace(", ", " ");
            str = str.Replace(",", " ");
            string[] bits = str.Split(' ');
            string lastWord = bits[bits.Length - 1];
            if (lastWord.IsNumeric()) {
                name.Year = lastWord;
                bits[bits.Length - 1] = "";
                if (name.Year.EndsWith(")")) {
                    name.ChangeCombination = true;
                    name.Year = name.Year.Substring(0, name.Year.Length - 1);
                }
                if (bits.Length > 1) {
                    if (taxon.AvailableName.ValueOrFalse() || String.IsNullOrEmpty(taxon.ElemType)) {
                        // For available names assume the last word is the author
                        name.Author = bits[bits.Length - 2];
                        bits[bits.Length - 2] = "";
                    } else {
                        // For valid names assume all other words, other then the first, are part of the author
                        for (int i = 1; i < bits.Length - 1; ++i) {
                            name.Author += bits[i] + " ";
                        }
                        name.Author = name.Author.Trim();
                    }
                    if (name.Author.StartsWith("(")) {
                        name.Author = name.Author.Substring(1);
                    }
                }
            } else {
                // Determine if the last word starts with a capital
                // if so, then it is the author, otherwise its a Genus or other
                char firstLetter = lastWord[0];
                if (firstLetter == '(' && (lastWord.Length > 1)) {
                    firstLetter = lastWord[1];
                }
                if (Char.IsUpper(firstLetter)) {
                    name.Author = lastWord;
                    bits[bits.Length - 1] = "";
                    if (name.Author.StartsWith("(") && name.Author.EndsWith(")")) {
                        name.Author = name.Author.Substring(1, name.Author.Length - 2);
                        name.ChangeCombination = true;
                    }
                }
            }
            
            if (taxon.AvailableName.ValueOrFalse() || String.IsNullOrEmpty(taxon.ElemType)) {
                // If the item is an available name, the rest of the text is the taxon name
                name.Epithet = "";
                foreach (string bit in bits) {
                    name.Epithet = (name.Epithet + " " + bit).Trim();
                }
            } else {
                // This is a valid name, so the epithet should be the first non-blank portion of the name
                name.Epithet = bits[0].Trim();
                if (name.Epithet.StartsWith("(") && name.Epithet.EndsWith(")")) {
                    name.Epithet = name.Epithet.Substring(1, name.Epithet.Length - 2);
                }
            }

            return name;
        }
    }

    public class TaxonName {
        public string Epithet { get; set; }
        public string Author { get; set; }
        public string Year { get; set; }
        public bool ChangeCombination { get; set; }
    }
}
