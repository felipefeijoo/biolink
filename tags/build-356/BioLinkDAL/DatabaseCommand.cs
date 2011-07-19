using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Data {

    /// <summary>
    /// Base class for a Command pattern. Each command represents a action against the database, and contains enough state to perform the command independently.
    /// Commands are aggregated by command container controls
    /// </summary>
    public abstract class DatabaseCommand {

        public void Process(User user) {
            ProcessImpl(user);
        }

        protected abstract void ProcessImpl(User user);

        public virtual List<string> Validate() {
            return null;
        }
        
    }

    public abstract class GenericDatabaseCommand<T> : DatabaseCommand where T: BioLinkDataObject {
        
        public GenericDatabaseCommand(T model) {
            Model = model;            
        }

        public T Model { get; private set; }

        public override bool Equals(object obj) {

            if (obj.GetType() == this.GetType()) {
                var other = obj as GenericDatabaseCommand<T>;
                return other.Model == Model;
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
