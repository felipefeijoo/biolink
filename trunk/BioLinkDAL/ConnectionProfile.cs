using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.ComponentModel;

namespace BioLink.Data {

    [Notify]
    public class ConnectionProfile {

        public string Name { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public Nullable<int> Timeout { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string LastUser { get; set; }

    }
}
