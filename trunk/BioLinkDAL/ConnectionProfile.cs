using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data {

    public class ConnectionProfile {
        public string Name { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public Nullable<int> Timeout { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string LastUser { get; set; }
    }
}
