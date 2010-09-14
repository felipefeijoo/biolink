using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class MultimediaItem : BiolinkDataObject {

        public int MultimediaID { get; set; }

        public int MultimediaLinkID { get; set; }

        public string MultimediaType { get; set; }

        public string Name { get; set; }

        public string Caption { get; set; }

        public string Extension { get; set; }

        public int SizeInBytes { get; set; }

        public int Changes { get; set; }

        public int BlobChanges { get; set; }

    }

    public class MultimediaType {

        public int ID { get; set; }

        public string Name { get; set; }
    }
}
