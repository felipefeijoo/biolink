using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using SharpMap.Layers;
using SharpMap.Geometries;
using SharpMap;
using SharpMap.Data;
using SharpMap.Data.Providers;
using System.Data;

namespace BioLink.Client.Maps {

    public class PointsFeaturesReport : ReportBase {

        public PointsFeaturesReport(User user, List<VectorLayer> pointLayers, List<VectorLayer> featureLayers) : base(user) {
            RegisterViewer(new TabularDataViewerSource());
            this.PointLayers = pointLayers;
            this.FeatureLayers = featureLayers;
        }

        public override bool DisplayOptions(User user, System.Windows.Window parentWindow) {
            var frm = new PointFeaturesOptions(PointLayers, FeatureLayers);
            if (frm.ShowDialog() == true) {
                this.SelectedColumns = frm.SelectedColumns;
                this.SelectedFeatureLayer = frm.SelectedFeatureLayer;
                this.SelectedPointLayers = frm.SelectedPointLayers;
                return true;
            }

            return false;
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {

            var matrix = new DataMatrix();

            foreach (string colname in SelectedColumns) {
                matrix.Columns.Add(new MatrixColumn { Name = colname });
            }

            matrix.Columns.Add(new MatrixColumn { Name = "Count" });

            // Open the feature layer data
            SelectedFeatureLayer.DataSource.Open();

            var map = new Dictionary<string, FeatureCountPair>();

            if (progress != null) {
                progress.ProgressMessage("Counting points...", 0);
            }

            int pointCount = 0;
            foreach (VectorLayer pointLayer in SelectedPointLayers) {
                var ds = pointLayer.DataSource;
                ds.Open();
                pointCount += ds.GetFeatureCount();
            }

            if (progress != null) {
                progress.ProgressStart(String.Format("Processing {0} points...", pointCount));
            }

            int processed = 0;
            int notFoundCount = 0;
            foreach (VectorLayer pointLayer in SelectedPointLayers) {
                var ds = pointLayer.DataSource;
                ds.Open();
                for (uint i = 0; i < ds.GetFeatureCount(); ++i) {
                    var row = ds.GetFeature(i);
                    SharpMap.Geometries.Point p = row.Geometry as Point;
                    if (!ProcessPoint(SelectedFeatureLayer.DataSource, pointLayer, p, map)) {
                        notFoundCount++;
                    }
                    processed++;
                    if (progress != null) {
                        double percent = ((double)processed) / ((double)pointCount) * 100;
                        progress.ProgressMessage(String.Format("Processing {0} points...", pointCount), percent);
                    }
                }
            }

            if (progress != null) {
                progress.ProgressMessage("Constructing result set...", 100);
            }

            int countIndex = matrix.IndexOf("Count");

            foreach (FeatureCountPair pair in map.Values) {
                var matrixRow = matrix.AddRow();
                foreach (DataColumn col in pair.First.Table.Columns) {
                    if (SelectedColumns.Contains(col.ColumnName)) {
                        var index = matrix.IndexOf(col.ColumnName);
                        matrixRow[index] = pair.First[col];
                    }
                }
                matrixRow[countIndex] = pair.Second;
            }

            var unmatched = matrix.AddRow();
            unmatched[countIndex] = notFoundCount;

            if (progress != null) {
                progress.ProgressEnd(String.Format("{0} points processed.", pointCount));
            }

            return matrix;            
        }

        private bool ProcessPoint(IProvider featureProvider, VectorLayer pointLayer, Point p, Dictionary<string, FeatureCountPair> map) {
            SharpMap.Data.FeatureDataSet ds = new SharpMap.Data.FeatureDataSet();
            featureProvider.ExecuteIntersectionQuery(p.GetBoundingBox(), ds);
            DataTable tbl = ds.Tables[0] as SharpMap.Data.FeatureDataTable;

            var found = false;

            for (int i = 0; i < tbl.Rows.Count; ++i) {
                var data = tbl.Rows[i] as FeatureDataRow;

                string featurekey = MakeFeatureKey(data);
                if (data.Geometry is MultiPolygon) {
                    MultiPolygon mp = data.Geometry as MultiPolygon;
                    foreach (SharpMap.Geometries.Polygon polygon in mp) {
                        if (MapControl.PointInsidePolygon(p, polygon)) {
                            IncrementFeaturePointCount(map, featurekey, data);
                            found = true;
                        }
                    }
                } else if (data.Geometry is Polygon) {
                    var polygon = data.Geometry as Polygon;
                    if (MapControl.PointInsidePolygon(p, polygon)) {
                        IncrementFeaturePointCount(map, featurekey, data);
                        found = true;
                    }
                } else {
                    // ??? Dunno...
                    IncrementFeaturePointCount(map, featurekey, data);
                    found = true;
                }
            }

            return found;
        }

        private void IncrementFeaturePointCount(Dictionary<string, FeatureCountPair> map, string featurekey, FeatureDataRow data) {
            if (!map.ContainsKey(featurekey)) {
                map[featurekey] = new FeatureCountPair { First = data, Second = 0 };
            }
            map[featurekey].Second++;
        }

        private string MakeFeatureKey(FeatureDataRow data) {
            var b = new StringBuilder("rowkey");
            foreach (DataColumn col in data.Table.Columns) {
                b.Append("_").Append(data[col]);
            }
            return b.ToString();            
        }

        protected List<VectorLayer> PointLayers { get; private set; }

        protected List<VectorLayer> FeatureLayers { get; private set; }

        private List<VectorLayer> SelectedPointLayers { get; set; }

        private VectorLayer SelectedFeatureLayer { get; set; }

        public List<String> SelectedColumns { get; set; }

        public override string Name {
            get { return "Points Features"; }
        }
    }

    public class FeatureCountPair : Pair<FeatureDataRow, int> {
    }
}
