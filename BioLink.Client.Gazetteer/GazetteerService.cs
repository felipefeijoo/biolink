using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Gazetteer {

    public class GazetteerService : SQLiteServiceBase, IDisposable {

        public string Filename { get; private set; }
        

        public GazetteerService(string file)
            : base(file) {
            Filename = file;
            if (File.Exists(file)) {
                SQLiteConnection connection = new SQLiteConnection(String.Format("Data Source={0}", Filename));
                ValidateGazFile(connection);
            } else {
                throw new Exception("Gazetteer file not found!");
            }
        }

        public List<PlaceName> FindPlaceNames(string find, int maxrows = 1000) {
            List<PlaceName> list = new List<PlaceName>();
            try {
                string sql = "SELECT tPlace as Name, tType as PlaceType, tDivision as Division, tLatitude as LatitudeString, tLongitude as LongitudeString, dblLatitude as Latitude, dblLongitude as Longitude FROM tblGaz WHERE tPlace like @find ORDER BY tDivision, tPlace, tType LIMIT @limit";
                SelectReader(sql, (reader) => {
                    PlaceName place = new PlaceName();
                    MapperBase.ReflectMap(place, reader, null);
                    list.Add(place);
                }, new SQLiteParameter("@find", find + "%"), new SQLiteParameter("@limit", maxrows));
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }

            return list;
        }

        public List<PlaceName> FindPlaceNamesLimited(string find, string limitToDivision, int maxrows = 1000) {
            List<PlaceName> list = new List<PlaceName>();
            try {
                string sql = "SELECT tPlace as Name, tType as PlaceType, tDivision as Division, tLatitude as LatitudeString, tLongitude as LongitudeString, dblLatitude as Latitude, dblLongitude as Longitude FROM tblGaz WHERE tPlace like @find AND tDivision = @division ORDER BY tDivision, tPlace, tType LIMIT @limit";
                SelectReader(sql, (reader) => {
                    PlaceName place = new PlaceName();
                    MapperBase.ReflectMap(place, reader, null);
                    list.Add(place);
                }, new SQLiteParameter("@find", find + "%"), new SQLiteParameter("@limit", maxrows), new SQLiteParameter("@division", limitToDivision));
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }


            return list;
        }

        public List<CodeLabelPair> GetDivisions() {
            List<CodeLabelPair> results = new List<CodeLabelPair>();
            try {                
                SelectReader("SELECT tDatabase, tAbbreviation FROM tblDivisions", (reader) => {
                    string code = reader["tDatabase"] as string;
                    string abbrev = reader["tAbbreviation"] as string;
                    if (String.IsNullOrEmpty(abbrev)) {
                        abbrev = code;
                    }
                    results.Add(new CodeLabelPair(code, abbrev));
                });
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }

            return results;
        }

        private bool ValidateGazFile(SQLiteConnection connection) {
            // TODO: check for the necessary tables and version info...
            return true;
        }

    }

}
