using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class StorageLocation : GUIDObject {

        public int BiotaLocationID { get; set; }
        public int BiotaID { get; set; }
        public int BiotaStorageID { get; set; }
        public string Notes { get; set; }
        [MappingInfo("StorageLocation")]
        public string Location { get; set; }
        public string StoragePath { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => BiotaLocationID; }
        }

    }
}
