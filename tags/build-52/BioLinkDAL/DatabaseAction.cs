using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Data {

    public abstract class DatabaseAction<T> where T : BioLinkService {

        public void Process(T service) {
            ProcessImpl(service);
        }

        protected abstract void ProcessImpl(T service);
    }

}
