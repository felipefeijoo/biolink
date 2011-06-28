using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.IO;
using System.Text.RegularExpressions;

namespace BioLink.Client.Tools {

    public class GridLayer {

        private static Int32 GRD_MAGIC = 0xCAFA;

        private double[,] _data = null;

        public GridLayer(int width, int height) {
            Width = width;
            Height = height;
            _data = new double[Width, Height];
            this.Name = "model";
        }

        public GridLayer(string filename) {
            var f = new FileInfo(filename);
            if (f.Exists) {
                if (f.Extension.Equals(".grd", StringComparison.CurrentCultureIgnoreCase)) {
                    LoadFromGRDFile(filename);
                } else if (f.Extension.Equals(".asc", StringComparison.CurrentCultureIgnoreCase)) {
                    LoadFromASCIIFile(filename);
                } else {
                    throw new Exception("Unrecognized file extension!");
                }
                this.Name = filename;
            } else {
                throw new Exception("Could not open file: " + filename);
            }
        }

        public void SaveToASCIIFile(string filename) {
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write)) {
                using (var writer = new StreamWriter(fs, Encoding.ASCII)) {
                    writer.Write("ncols {0}\n", Width);
                    writer.Write("nrows {0}\n", Height);
                    writer.Write("xllcorner {0:#.######}\n", Longitude0);
                    writer.Write("yllcorner {0:#.######}\n", Latitude0);
                    writer.Write("cellsize {0:#.######}\n", DeltaLatitude);
                    writer.Write("NODATA_value {0:#.######}\n", NoValueMarker);

                    for (int lngRow = (Height - 1); lngRow >= 0; --lngRow) {
                        for (int lngCol = 0; lngCol < Width; ++lngCol) {
                            writer.Write("{0:#.######} ", _data[lngCol, lngRow]);
                        }
                        writer.Write("\n");
                    }
                }
            }
        }

        public void LoadFromASCIIFile(string filename) {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
                using (var reader = new StreamReader(fs)) {
                    Width = reader.ReadRegexInt(@"^ncols\s+(\d+)\s*$");
                    Height = reader.ReadRegexInt(@"^nrows\s+(\d+)\s*$");
                    Longitude0 = reader.ReadRegexDouble(@"^xllcorner\s+([-\d.]+)\s*$");
                    Latitude0 = reader.ReadRegexDouble(@"^yllcorner\s+([-\d.]+)\s*$");
                    DeltaLatitude = reader.ReadRegexDouble(@"^cellsize\s+([-\d.]+)\s*$");
                    NoValueMarker = reader.ReadRegexDouble(@"^NODATA_value\s+([-\d.]+)\s*$");
                    DeltaLongitude = DeltaLatitude;

                    _data = new double[Width, Height];
                    for (int y = Height - 1; y >= 0; --y) {
                        for (int x = 0; x < Width; ++x) {
                             _data[x,y] = reader.ReadDouble();
                        }
                    }
                }
            }
        }


        public void SaveToGRDFile(string filename) {
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write)) {
                using (var writer = new BinaryWriter(fs, Encoding.ASCII)) {                    
                    writer.Write((Int32) GRD_MAGIC);
                    writer.Write((Int32)0x38); // header size
                    writer.Write(Width);
                    writer.Write(Height);
                    writer.Write(Longitude0);
                    writer.Write(Latitude0);
                    writer.Write(DeltaLongitude);
                    writer.Write(DeltaLatitude);
                    writer.Write((float) NoValueMarker);
                    writer.Write((Int32) Flags);
                    for (int row = 0; row < Height; row++) {
                        for (int col = 0; col < Width; col++) {
                            writer.Write((float) _data[col, row]);
                        }
                    }
                }
            }
        }

        protected void LoadFromGRDFile(string filename) {

            double[,] data = null;

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
                // Buffer the file in a memory stream for performance....
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
                using (var stream = new MemoryStream(buffer)) {
                    using (BinaryReader reader = new BinaryReader(stream)) {
                        Int32 magic = reader.ReadInt32();
                        if (magic != GRD_MAGIC) {
                            throw new Exception("Bad magic number in GRD file!");
                        }
                        Int32 headsize = reader.ReadInt32();
                        Width = reader.ReadInt32();
                        Height = reader.ReadInt32();
                        Longitude0 = reader.ReadDouble();
                        Latitude0 = reader.ReadDouble();
                        DeltaLongitude = reader.ReadDouble();
                        DeltaLatitude = reader.ReadDouble();
                        NoValueMarker = reader.ReadSingle();
                        Flags = reader.ReadInt32();

                        // Allocate an array for the data...
                        data = new double[Width, Height];
                        for (int row = 0; row < Height; row++) {
                            for (int col = 0; col < Width; col++) {
                                data[col, row] = reader.ReadSingle();
                            }
                        }
                    }
                }
            }

            _data = data;
        }

        

        public double GetValueAt(double latitude, double longitude, double novalue = 0.0) {
	        int lngX, lngY;
	        double fudge = (double) ( DeltaLatitude / 2.0 );

	        lngX = (int) ((longitude - ( Longitude0 - fudge) ) / DeltaLongitude);
	        lngY = (int) ((latitude - Latitude0) / DeltaLatitude);

            if ((lngX >= Width) || (lngY >= Height)) {
                return novalue;
            }

	        if ((lngX < 0) || (lngY < 0)) return novalue;

            double val = _data[lngX, lngY];
	        return (val  == NoValueMarker ? novalue : val);
        }

        public double GetCellValue(int x, int y) {
            return _data[x, y];
        }

        public void SetCellValue(int x, int y, double value) {
            _data[x, y] = value;
        }

        public EnvironmentalLayerRange GetRange(double? percentile = null) {
            double? min = null;
            double? max = null;
            
            TraverseCells((x, y, value) => {
                if (value != NoValueMarker) {
                    if (!min.HasValue) {
                        min = value;
                        max = value;
                    } else {
                        if (value < min) {
                            min = value;
                        } else if (value > max) {
                            max = value;
                        }
                    }
                }
            });
            // Now work out the percentile....
            if (max.HasValue && min.HasValue) {
                double val = 0;
                if (percentile.HasValue) {
                    val = (max.Value - min.Value) * percentile.Value;
                }
                var range = new EnvironmentalLayerRange { Max = max.Value - val, Min = min.Value - val, Percentile = percentile };
                return range;
            }

            return null;
        }

        public EnvironmentalLayerRange GetRangeForPoints(IEnumerable<ModelPoint> points, double percentile) {

            var values = new PointValueList();

            foreach (ModelPoint p in points) {
                var value = GetValueAt(p.Y, p.X, NoValueMarker);
                if (value == NoValueMarker) {
                } else {                    
                    values.AddValue(value);
                }
            }

            return new EnvironmentalLayerRange { Max = values.GetUpper(percentile), Min = values.GetLower(percentile), Percentile = percentile };
        }

        public void SetAllCells(double value) {
            TraverseCells((x, y, current) => {
                _data[x, y] = value;
            });
        }

        public void TraverseCells(Action<int, int, double> action) {
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    action(x, y, _data[x, y]);
                }
            }
        }

        public bool MatchesResolution(GridLayer other) {

            if (this.Width != other.Width) return false;
            if (Height != other.Height) return false;
            if (Latitude0 != other.Latitude0) return false;
            if (Longitude0 != other.Longitude0) return false;
            if (DeltaLatitude != other.DeltaLatitude) return false;
            if (DeltaLongitude != other.DeltaLongitude) return false;

            return true;
        }


        #region Properties

        public string Name { get; protected set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public double Latitude0 { get; set; }
        public double Longitude0 { get; set; }
        public double DeltaLatitude { get; set; }
        public double DeltaLongitude { get; set; }
        public double NoValueMarker { get; set; }
        public Int32 Flags { get; set; }

        #endregion
    }

    class PointValueList {
        
        private LLNode _first;

        public PointValueList() {
            Count = 0;
            _first = null;
        }

        public bool AddValue(double value) {

            var newNode = new LLNode(value);
            if (_first == null) {
                _first = newNode;
                Count = 1;
                return true;
            }

            if (value < _first.Value) {
                newNode.Next = _first;
                _first = newNode;
                Count++;
                return true;
            }

            var temp = _first;
            var prev = temp;
            while (temp != null && value > temp.Value) {
                prev = temp;
                temp = temp.Next;
            }
            newNode.Next = prev.Next;
            prev.Next = newNode;

            Count++;

            return true;
        }

        public double GetLower(double percentile) {
            int index = (int) Math.Round((double)(Count + 1) * percentile, MidpointRounding.AwayFromZero);
            var p = _first;
            int i = 0;

            while (p != null && (i++ < index - 1)) {
                p = p.Next;
            }

            if (p != null) {
                return p.Value;
            }

            return 0;            
        }

        public double GetUpper(double percentile) {
            int index = (int) Math.Round(((double) (Count + 1) * (1-percentile)), MidpointRounding.AwayFromZero) - 1;
            var p = _first;
            int i = 0;
            double last = 0;

            while (p != null && (i++ < index)) {
                last = p.Value;
                p = p.Next;
            }

            if (p != null) {
                return p.Value;
            } 

            return last;
        }

        public int Count { get; private set; }

    }

    class LLNode {

        public LLNode() {
            Next = null;
            Value = 0;
        }

        public LLNode(double value) {
            Next = null;
            Value = value;
        }

        public LLNode Next { get; set; }
        public double Value { get; set; }
    }

    public class EnvironmentalLayerRange {
        public double Max { get; set; }
        public double Min { get; set; }        
        public double? Percentile { get; set; }

        public double Range {
            get { return Max - Min; }
        }
    }
}
