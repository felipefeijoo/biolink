using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class FieldDescriptor {

        public string DisplayName { get; set; }
        public string FieldName { get; set; }
        public string TableName { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Format { get; set; }
        public bool UseInRDE { get; set; }
        public string DataType { get; set; }

        public override string ToString() {
            return string.Format("{0} ({1}.{2})", DisplayName, TableName, FieldName);
        }

    }

    public class QueryCriteria {

        public FieldDescriptor Field { get; set; }
        public string Criteria { get; set; }
        public bool Output { get; set; }
        public string Alias { get; set; }
        public string Sort { get; set; }
        public string FormatOption { get; set; }

    }

}
