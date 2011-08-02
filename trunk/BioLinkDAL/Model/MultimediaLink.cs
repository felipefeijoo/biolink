using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class MultimediaLink : OwnedDataObject {

        public int MultimediaID { get; set; }

        public int MultimediaLinkID { get; set; }

        public string MultimediaType { get; set; }

        public string Name { get; set; }

        public string Caption { get; set; }

        public string Extension { get; set; }

        public int SizeInBytes { get; set; }

        public int Changes { get; set; }

        public int BlobChanges { get; set; }

        public string TempFilename { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.MultimediaLinkID; }
        }
    }

    public class MultimediaType {

        public int ID { get; set; }

        public string Name { get; set; }
    }


}
