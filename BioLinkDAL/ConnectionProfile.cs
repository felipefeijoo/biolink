using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.ComponentModel;

namespace BioLink.Data {

    public class ConnectionProfile : ChangeableModelBase {

        private string _name;
        private string _server;
        private string _database;
        private Nullable<int> _timeout;
        private bool _integratedSecurity;
        private string _lastUser;

        public string Name {
            get { return _name; }
            set { SetProperty("Name", ref _name, value); }
        }

        public string Server { 
            get { return _server; }
            set { SetProperty("Server", ref _server, value); }
        }

        public string Database { 
            get { return _database; }
            set { SetProperty("Database", ref _database, value); } 
        }

        public Nullable<int> Timeout {
            get { return _timeout; }
            set { SetProperty("Timeout", ref _timeout, value); }
        }

        public bool IntegratedSecurity {
            get { return _integratedSecurity; }
            set { SetProperty("IntegratedSecurity", ref _integratedSecurity, value); }
        }

        public string LastUser {
            get { return _lastUser; }
            set { SetProperty("LastUser", ref _lastUser, value); }
        }

    }
}
