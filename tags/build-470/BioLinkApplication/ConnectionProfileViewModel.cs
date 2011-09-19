/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
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
