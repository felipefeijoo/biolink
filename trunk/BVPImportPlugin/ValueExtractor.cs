using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GenericParsing;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace BioLink.Client.BVPImport {

    public abstract class ValueExtractor {
        
        public abstract String ExtractValue(BVPImportColumnDefinition coldef, GenericParserAdapter row, Dictionary<String, List<String>> extraData);

        protected String Default(BVPImportColumnDefinition columnDef, GenericParserAdapter row, Dictionary<String, List<String>> extraData) {
            var value = row[columnDef.SourceColumnName];
            return value;
        }
    }

    public class PassThroughValueExtractor : ValueExtractor {

        public override string ExtractValue(BVPImportColumnDefinition columnDef, GenericParserAdapter row, Dictionary<string, List<string>> extraData) {
            return Default(columnDef, row, extraData);            
        }
    }

    public class RegexCaptureValueExtractor : ValueExtractor {

        public String[] Patterns { get; private set; }
        public List<Regex> Regexes { get; private set; }

        public RegexCaptureValueExtractor(params String[] patterns) {
            this.Regexes = new List<Regex>();
            this.Patterns = patterns;
            foreach (String pattern in patterns) {
                Regexes.Add(new Regex(pattern));
            }
        }

        public override string ExtractValue(BVPImportColumnDefinition columnDef, GenericParserAdapter row, Dictionary<string, List<string>> extraData) {
            var value = Default(columnDef, row, extraData);
            if (!String.IsNullOrEmpty(value)) {
                value = value.Trim();
                foreach (Regex regex in Regexes) {
                    var matcher = regex.Match(value);
                    if (matcher.Success) {
                        if (matcher.Groups.Count > 1) {
                            return matcher.Groups[1].Value;
                        }
                    }
                }
            }
            return value;
        }
    }

    public class ConcatExtraDataFieldsValueExtractor : ValueExtractor {

        protected String Delimiter { get; private set; }
        public Func<String, String> ValueFormatter { get; set; }

        public ConcatExtraDataFieldsValueExtractor(String delimiter = ", ") {
            this.Delimiter = delimiter;
        }

        public override string ExtractValue(BVPImportColumnDefinition coldef, GenericParserAdapter row, Dictionary<string, List<string>> extraData) {            
            if (extraData != null && extraData.ContainsKey(coldef.SourceColumnName)) {
                var values = extraData[coldef.SourceColumnName];
                if (ValueFormatter != null) {
                    var formattedValues = values.Select((value) => ValueFormatter(value));
                    return String.Join(Delimiter, formattedValues);
                } else {
                    return String.Join(Delimiter, values);
                }
            }
            return "";
        }
    }

    public class ValueMappingValueExtractor : ValueExtractor {

        private Dictionary<String, String> _valueMap = new Dictionary<string, string>();

        public ValueMappingValueExtractor(Dictionary<String, String> valueMappings) {
            foreach (String key in valueMappings.Keys) {
                _valueMap[key.ToLower()] = valueMappings[key];
            }
        }

        public override string ExtractValue(BVPImportColumnDefinition coldef, GenericParserAdapter row, Dictionary<string, List<string>> extraData) {
            var value = Default(coldef, row, extraData);
            if (!String.IsNullOrEmpty(value) && _valueMap.ContainsKey(value.ToLower())) {
                value = _valueMap[value.ToLower()];
            }
            return value;
        }

    }

    public class ANICIdentifiedByValueExtractor : ValueExtractor {

        public override string ExtractValue(BVPImportColumnDefinition coldef, GenericParserAdapter row, Dictionary<string, List<string>> extraData) {
            var value = row[coldef.SourceColumnName];
            return ANICUtils.ConvertNameFormat(value);
        }
    }

    public class ANICCollectorNameFormattingValueExtractor : ConcatExtraDataFieldsValueExtractor {

        public ANICCollectorNameFormattingValueExtractor() : base(" & ") {
            this.ValueFormatter = FormatName;
        }

        protected String FormatName(String value) {
            return ANICUtils.ConvertNameFormat(value);
        }
    }

    public class ANICStateStripValueExtractor : ValueExtractor {

        private Regex _initals = new Regex(@"^(.*?)(?:[\s\r\n,.;]+)(?:W[.]*A[.]*|N[.]*T[.]*|Q[.]*[L][.]*D[.]*|N[.]*S[.]*W[.]*|S[.]*A[.]*|T[.]*A[.]*S[.]*|V[.]*I[.]*C[.]*|W[.]*A[.]*|A[.]*C[.]*T[.]*)\s*$", RegexOptions.IgnoreCase);
        private Regex _fullword = new Regex(@"^(.*)(?:[\s\r\n,.;]+)(?:queensland|northern territory|new south wales|victoria|south australia|australian capital territory|western australia|tasmania)[.]*\s*$", RegexOptions.IgnoreCase);

        public override string ExtractValue(BVPImportColumnDefinition coldef, GenericParserAdapter row, Dictionary<string, List<string>> extraData) {
            var value = Default(coldef, row, extraData);
            
            value = value.Replace("\\n", " ");            

            var m = _initals.Match(value);
            if (m.Success) {
                value = m.Groups[1].Value;
            } else {
                m = _fullword.Match(value);
                if (m.Success) {
                    value = m.Groups[1].Value;
                }
            }
            return value;
        }
    }

    public class ANICLatLongValueExtractor : ValueExtractor {

        public override string ExtractValue(BVPImportColumnDefinition coldef, GenericParserAdapter row, Dictionary<string, List<string>> extraData) {
            var value = row[coldef.SourceColumnName];
            if (String.IsNullOrEmpty(value)) {
                String alternate = null;
                if (coldef.SourceColumnName.Equals("decimalLatitude")) {
                    alternate = row["verbatimLatitude"];
                } else if (coldef.SourceColumnName.Equals("decimalLongitude")) {
                    alternate = row["verbatimLongitude"];
                }
                if (!String.IsNullOrEmpty(alternate)) {
                    double? coord = GeoUtils.ParseCoordinate(alternate);
                    if (coord.HasValue) {
                        value = Math.Round(coord.Value, 2).ToString();
                    }
                }
            }
            return value;
        }
    }

    public abstract class DateValueExtractor : ValueExtractor {

        protected static Regex _BVPDateRegexFull = new Regex(@"^(\d\d\d\d)[-.](\d\d)[-.](\d\d)$");
        protected static Regex _BVPDateRegexYearMonth = new Regex(@"^(\d\d\d\d)[-.](\d\d)$");
        protected static Regex _BVPDateRegexYear = new Regex(@"^(\d\d\d\d)$");

        public override string ExtractValue(BVPImportColumnDefinition coldef, GenericParserAdapter row, Dictionary<string, List<string>> extraData) {
            var value = Default(coldef, row, extraData);
            var dates = value.Split('/');
            return ExtractDate(dates, value);
        }

        protected abstract String ExtractDate(String[] candidates, string @default);
    }

    public class StartDateValueExtractor : DateValueExtractor {

        protected override string ExtractDate(String[] candidates, String @default) {
            if (candidates.Length > 0) {
                var bldate = DateUtils.DateStrToBLDate(candidates[0]);
                if (!String.IsNullOrEmpty(bldate)) {
                    return bldate;
                }
            }
            return @default;
        }

    }

    public class EndDateValueExtractor : DateValueExtractor {

        protected override string ExtractDate(String[] candidates, String @default) {
            if (candidates.Length > 1) {
                var bldate = DateUtils.DateStrToBLDate(candidates[1]);
                if (!String.IsNullOrEmpty(bldate)) {
                    return bldate;
                }
            }
            return null;
        }

    }

    public class AssociatedMediaValueExtractor : ValueExtractor {

        public override string ExtractValue(BVPImportColumnDefinition coldef, GenericParserAdapter row, Dictionary<string, List<string>> extraData) {
            if (extraData != null && extraData.ContainsKey(coldef.SourceColumnName)) {
                var mediaList = extraData[coldef.SourceColumnName];
                if (mediaList != null && mediaList.Count > 0) {
                    return mediaList[0];
                }
            }
            return null;
        }
    }

}
