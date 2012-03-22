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
            if (SuccessAction != null) {
                SuccessAction();
            }
        }

        public void CheckPermissions(User user) {
            var requiredPermissions = new PermissionBuilder();
            BindPermissions(requiredPermissions);
            foreach (RequiredPermission required in requiredPermissions.Permissions) {
                if (required is BasicRequiredPermission) {
                    var basic = required as BasicRequiredPermission;
                    user.CheckPermission(basic.Category, basic.Mask, "You do not have permission to perform this action.");
                } else if (required is BiotaRequiredPermission) {
                    var taxonperm = required as BiotaRequiredPermission;
                    if (!user.HasBiotaPermission(taxonperm.TaxonID, taxonperm.Mask)) {
                        throw new NoPermissionException("You do not have permission to perform this action.");
                    }
                }
            }
        }

        protected abstract void BindPermissions(PermissionBuilder required);

        protected abstract void ProcessImpl(User user);

        public Action SuccessAction { get; set; }

        public virtual void Validate(ValidationMessages messages) {
        }
        
    }

    public enum ValidationType {
        Error, Warning
    }

    public class ValidationMessage {
       
        public ValidationMessage(ValidationType type, string message) {            
            ValidationType = type;
            Message = message;
        }

        public ValidationType ValidationType { get; set; }
        public string Message { get; set; }
    }

    public class ValidationMessages {

        public ValidationMessages() {
            Messages = new List<ValidationMessage>();
        }

        public void Warn(string message) {
            Messages.Add(new ValidationMessage(ValidationType.Warning, message));
        }

        public void Error(string message) {
            Messages.Add(new ValidationMessage(ValidationType.Error, message));
        }

        public List<ValidationMessage> Messages { get; private set; }
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

    public abstract class RequiredPermission {

        protected RequiredPermission(PERMISSION_MASK mask) {
            this.Mask = mask;
        }

        public PERMISSION_MASK Mask { get; private set; }
    }

    public class BasicRequiredPermission : RequiredPermission {

        public BasicRequiredPermission(PermissionCategory category, PERMISSION_MASK mask) : base(mask) {
            this.Category = category;
        }

        public PermissionCategory Category { get; private set; }        
    }

    public class BiotaRequiredPermission : RequiredPermission {
        public BiotaRequiredPermission(int taxonId, PERMISSION_MASK mask) : base(mask) {
            this.TaxonID = taxonId;
        }

        public int TaxonID { get; private set; }
    }

    public class PermissionBuilder {

        private List<RequiredPermission> _required = new List<RequiredPermission>();

        public PermissionBuilder Add(PermissionCategory category, PERMISSION_MASK mask) {
            _required.Add(new BasicRequiredPermission(category, mask));
            return this;
        }

        public PermissionBuilder AddBiota(int taxonId, PERMISSION_MASK mask) {
            _required.Add(new BiotaRequiredPermission(taxonId, mask));
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
