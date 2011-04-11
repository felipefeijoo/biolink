using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.IO;

namespace BioLink.Client.Tools {

    public interface IEnvironmentalLayer {
        double GetValueAt(double latitude, double longitude, double @default=0.0);
        EnvironmentalLayerRange GetRange(double percentile);
        EnvironmentalLayerRange GetRangeForPoints(IEnumerable<MapPoint> points, double percentile);
        string Name { get; }
    }

    public abstract class GridLayer : IEnvironmentalLayer {

        private double[,] _data = null;

        protected GridLayer(string filename) {
            _data = LoadImpl(filename);
            this.Name = filename;
        }

        protected abstract double[,] LoadImpl(string filename);

        public double GetValueAt(double latitude, double longitude, double novalue = 0.0) {
	        int lngX, lngY;
	        double fudge = (double) ( DeltaLatitude / 2.0 );

	        lngX = (int) ((longitude - ( Longitude0 - fudge) ) / DeltaLongitude);
	        lngY = (int) ((latitude - ( Latitude0) ) / DeltaLatitude);

            if ((lngX >= Width) || (lngY >= Height)) {
                return novalue;
            }

	        if ((lngX < 0) || (lngY < 0)) return novalue;

            double val = _data[lngY, lngX];
	        return (val  == NoValueMarker ? novalue : val);
        }

        public EnvironmentalLayerRange GetRange(double percentile) {
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
                var val = (max.Value - min.Value) * percentile;

                var range = new EnvironmentalLayerRange { Max = max.Value - val, Min = min.Value - val, Percentile = percentile };
                return range;
            }

            return null;
        }

        public EnvironmentalLayerRange GetRangeForPoints(IEnumerable<MapPoint> points, double percentile) {

            foreach (MapPoint p in points) {
                var value = GetValueAt(p.Latitude, p.Longitude, NoValueMarker);
            }

            return null;
        }

        public void SetAllCells(double value) {
            TraverseCells((x, y, current) => {
                _data[x, y] = value;
            });
        }

        protected void TraverseCells(Action<int, int, double> action) {
            for (int row = 0; row < Height; row++) {
                for (int col = 0; col < Width; col++) {
                    action(row, col, _data[row, col]);                    
                }
            }
        }


        #region Properties

        public string Name { get; protected set; }

        protected int Width { get; set; }
        protected int Height { get; set; }
        protected double Latitude0 { get; set; }
        protected double Longitude0 { get; set; }
        protected double DeltaLatitude { get; set; }
        protected double DeltaLongitude { get; set; }
        protected double NoValueMarker { get; set; }
        protected Int32 Flags { get; set; }

        #endregion

    }

    public class GRDGridLayer : GridLayer {

        private static Int32 GRD_MAGIC = 0xCAFA;

        public GRDGridLayer(string filename) : base(filename) { }

        protected override double[,] LoadImpl(string filename) {

            double[,] data = null;

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
                // Buffer the file in a memory stream for performance....
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int) fs.Length);
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

            return data;
        }
    }

    public class EnvironmentalLayerRange {
        public double Max { get; set; }
        public double Min { get; set; }        
        public double Percentile { get; set; }

        public double Range {
            get { return Max - Min; }
        }
    }
}
