using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Data {

    /// <summary>
    /// Base class for a Command pattern. Each command represents a action against the database, and contains enough state to perform the command independently.
    /// Commands are aggregated by command container controls
    /// </summary>
    public abstract class DatabaseCommand {

        public DatabaseCommand() {
        }

        public void Process(User user) {
            CheckPermissions(user);
            ProcessImpl(user);
        }

        public void CheckPermissions(User user) {
            var required = new PermissionBuilder();
            BindPermissions(required);
            foreach (RequiredPermission mask in required.Permissions) {
                user.CheckPermission(mask.Category, mask.Mask, "You do not have permission to perform this action.");
            }            
        }

        protected abstract void BindPermissions(PermissionBuilder required);

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

    public class RequiredPermission {

        public RequiredPermission(PermissionCategory category, PERMISSION_MASK mask) {
            this.Category = category;
            this.Mask = mask;
        }

        public PermissionCategory Category { get; private set; }
        public PERMISSION_MASK Mask { get; private set; }
    }

    public class PermissionBuilder {

        private List<RequiredPermission> _required = new List<RequiredPermission>();

        public PermissionBuilder Add(PermissionCategory category, PERMISSION_MASK mask) {
            _required.Add(new RequiredPermission(category, mask));
            return this;
        }

        /// <summary>
        /// Placeholder to help find commands that have no required permissions...
        /// </summary>
        /// <returns></returns>
        public PermissionBuilder None() {
            _required.Clear();
            return this;
        }

        public IEnumerable<RequiredPermission> Permissions { get { return _required; } }
    }


}
