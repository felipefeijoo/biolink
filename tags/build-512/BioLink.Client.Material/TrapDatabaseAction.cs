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
using BioLink.Client.Extensibility;

namespace BioLink.Client.Material {

    public class RenameTrapCommand : GenericDatabaseCommand<SiteExplorerNode> {

        public RenameTrapCommand(SiteExplorerNode model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameTrap(Model.ElemID, Model.Name);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_TRAP, PERMISSION_MASK.UPDATE);
        }

    }

    public class DeleteTrapCommand : DatabaseCommand {

        public DeleteTrapCommand(int trapId) {
            this.TrapID = trapId;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteTrap(TrapID);
        }

        public int TrapID { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_TRAP, PERMISSION_MASK.DELETE);
        }

    }

    public class InsertTrapCommand : AbstractSiteExplorerCommand {

        public InsertTrapCommand(SiteExplorerNode model, SiteExplorerNodeViewModel viewModel) : base(model, viewModel) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertTrap(Model.ParentID, Model.Name);
            UpdateChildrenParentID();
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_TRAP, PERMISSION_MASK.INSERT);
        }

    }

    public class UpdateTrapCommand : GenericDatabaseCommand<Trap> {

        public UpdateTrapCommand(Trap trap)
            : base(trap) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateTrap(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_TRAP, PERMISSION_MASK.UPDATE);
        }

    }

    public class MergeTrapCommand : GenericDatabaseCommand<SiteExplorerNode> {

        public MergeTrapCommand(SiteExplorerNode source, SiteExplorerNode dest)
            : base(source) {
            Dest = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.MergeTrap(Model.ElemID, Dest.ElemID);
        }

        public SiteExplorerNode Dest { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_EXPLORER, PERMISSION_MASK.ALLOW);
        }

    }

    public class MoveTrapCommand : GenericDatabaseCommand<SiteExplorerNode> {

        public MoveTrapCommand(SiteExplorerNode source, SiteExplorerNode dest)
            : base(source) {
            Dest = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.MoveTrap(Model.ElemID, Dest.ElemID);
        }

        public SiteExplorerNode Dest { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_EXPLORER, PERMISSION_MASK.ALLOW);
        }

    }

}
