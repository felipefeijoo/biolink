using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Reflection;

namespace BioLink.Client.Extensibility {

    public class GenericModelReport<T> : ReportBase where T : ViewModelBase {

        private string _reportName;

        public GenericModelReport(User user, string reportName, ICollection<T> model, params String[] columns) : base(user) {
            RegisterViewer(new TabularDataViewerSource());
            _reportName = reportName;
            Model = model;
            Columns = columns;
        }

        public override string Name {
            get { return _reportName; }
        }

        public override Data.DataMatrix ExtractReportData(Utilities.IProgressObserver progress) {
            DataMatrix matrix = new DataMatrix();
            // Cache the property getter
            Dictionary<String, MethodInfo> getters = new Dictionary<String, MethodInfo>();            
            foreach (String colName in Columns) {
                matrix.Columns.Add(new MatrixColumn { Name = colName, IsHidden = false });
                var p = typeof(T).GetProperty(colName);
                if (p != null) {
                    getters[colName] = p.GetGetMethod();
                }
            }

            foreach (T vm in Model) {
                var row = matrix.AddRow();
                int i = 0;
                foreach (String colName in Columns) {
                    Object value = null;
                    if (getters.ContainsKey(colName)) {
                        value = getters[colName].Invoke(vm, null);
                    }
                    row[i++] = value;
                }
            }

            return matrix;
        }

        protected ICollection<T> Model { get; private set; }

        protected String[] Columns { get; private set; }

    }
}
