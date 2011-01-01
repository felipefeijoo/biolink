using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;

namespace BioLinkApplication {

    public class ConnectionProfileViewModel : GenericViewModelBase<ConnectionProfile> {

        public ConnectionProfileViewModel(ConnectionProfile model) : base(model, null) { }

        public String Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public String Server {
            get { return Model.Server; }
            set { SetProperty(() => Model.Server, value); }
        }

        public String Database {
            get { return Model.Database; }
            set { SetProperty(() => Model.Database, value); }
        }

        public Nullable<int> Timeout {
            get { return Model.Timeout; }
            set { SetProperty(() => Model.Timeout, value); }
        }

        public bool IntegratedSecurity {
            get { return Model.IntegratedSecurity; }
            set { SetProperty(() => Model.IntegratedSecurity, value); }
        }

        public string LastUser {
            get { return Model.LastUser; }
            set { SetProperty(() => Model.LastUser, value); }
        }

    }
}
