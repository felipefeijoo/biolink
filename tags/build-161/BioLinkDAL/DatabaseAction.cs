using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Data {

    public abstract class DatabaseAction {

        public void Process(User user) {
            ProcessImpl(user);
        }

        protected abstract void ProcessImpl(User user);
        
    }

    public abstract class GenericDatabaseAction<T> : DatabaseAction {
        
        public GenericDatabaseAction(T model) {
            Model = model;            
        }

        public T Model { get; private set; }

        public override bool Equals(object obj) {
            GenericDatabaseAction<T> other = obj as GenericDatabaseAction<T>;
            if (other != null) {
                return other.Model.Equals(this.Model);
            }
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode() + Model.GetHashCode();
        }

        public override string ToString() {
            return string.Format("{0}: {1}", this.GetType().Name, Model);
        }

    }

}
