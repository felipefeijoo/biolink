using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class XMLIOMaterial : Material {

        public Guid? SiteVisitGUID { get; set; }
        public Guid? SiteGUID { get; set; }
        public Guid? TrapGUID { get; set; }
        public Guid? BiotaGUID { get; set; }

    }
}
