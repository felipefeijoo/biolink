using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using BioLink.Data;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Gazetteer {

    public class GazetteerService : SQLiteServiceBase {

        public string Filename { get; private set; }
        private SQLiteConnection _connection;

        public GazetteerService(string file) : base(file, true) {
            Filename = file;
            if (File.Exists(file)) {
                _connection = new SQLiteConnection(String.Format("Data Source={0}", Filename));
                ValidateGazFile(_connection);
            } else {
                throw new Exception("Gazetteer file not found!");
            }
        }

        public List<PlaceName> FindPlaceNames(string find) {
            List<PlaceName> list = new List<PlaceName>();
            string sql = "SELECT tPlace as Name, tType as PlaceType, tDivision as Division, tLatitude as LatitudeString, tLongitude as LongitudeString, dblLatitude as Latitude, dblLongitude as Longitude FROM tblGaz WHERE tPlace like @find ORDER BY tDivision, tPlace, tType";
            SelectReader(sql, (reader) => {
                PlaceName place = new PlaceName();
                MapperBase.ReflectMap(place, reader);
                list.Add(place);
            }, new SQLiteParameter("@find", find + "%"));

            return list;
        }

        private bool ValidateGazFile(SQLiteConnection connection) {
            // TODO: check for the necessary tables and version info...
            return true;
        }
    }

    public class PlaceName {
        public string Name { get; set; }
        public string PlaceType { get; set; }
        public string Division { get; set; }
        public string LatitudeString { get; set; }
        public string LongitudeString { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string GazID { get; set; }
    }
}
