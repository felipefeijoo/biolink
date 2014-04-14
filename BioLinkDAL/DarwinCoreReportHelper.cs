using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.Reflection;
using System.IO;


namespace BioLink.Data {

    public class DarwinCoreReportHelper {

        public DataMatrix RunDwcQuery(BioLinkService service, String whereClause) {

            Assembly assembly = Assembly.GetExecutingAssembly();
            TextReader inputStream = new StreamReader(assembly.GetManifestResourceStream("BioLink.Data.scripts.DarwinCoreReportTemplate.txt"));
            string sql = inputStream.ReadToEnd();

            // Now we need do substitutions
            var values = new Dictionary<String, String>();
            values["InstitutionCode"] = "";
            values["CollectionCode"] = "";
            values["where"] = whereClause;

            sql = StringUtils.SubstitutePlaceholders(sql, values);

            var formatters = new Dictionary<String, ColumnDataFormatter>();
            formatters["occurrenceRemarks"] = (value, reader) => RTFUtils.StripMarkup(value as String);
            formatters["dateIdentified"] = (value, reader) => {
                var d = value as DateTime?;
                if (d != null) {
                    return d.Value.ToString("yyyy-MM-dd");
                }

                return value;
            };

            formatters["eventDate"] = (value, reader) => {
                var rangeStr = value as String;
                if (rangeStr != null) {
                    var bits = rangeStr.Split('/');
                    var sb = new StringBuilder();
                    Int32 date1 = 0, date2 = 0;
                    if (bits.Length > 1) {
                        Int32.TryParse(bits[0], out date1);
                        Int32.TryParse(bits[1], out date2);
                    } else {
                        if (bits.Length == 1) {
                            Int32.TryParse(bits[0], out date1);
                        }
                    }

                    if (date1 > 0) {
                        sb.Append(DateUtils.FormatBLDate("yyyy-MM-dd", date1));
                        if (date2 > 0) {
                            sb.Append("/").Append(DateUtils.FormatBLDate("yyyy-MM-dd", date2));
                        }
                    } else if (date2 > 0) {
                        sb.Append(DateUtils.FormatBLDate("yyyy-MM-dd", date2));
                    }
                    return sb.ToString();
                }
                return value;
            };

            return service.SQLReaderDataMatrix(sql, formatters, null);
        }

    }
}
