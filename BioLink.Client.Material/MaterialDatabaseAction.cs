﻿/*******************************************************************************
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

    public class RenameMaterialCommand : GenericDatabaseCommand<SiteExplorerNode> {

        public RenameMaterialCommand(SiteExplorerNode model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameMaterial(Model.ElemID, Model.Name);
        }
        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }

    public class InsertMaterialCommand : AbstractSiteExplorerCommand {

        public InsertMaterialCommand(SiteExplorerNode model, SiteExplorerNodeViewModel viewModel, int templateID = 0) : base(model, viewModel) {
            this.TemplateID = templateID;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertMaterial(Model.ParentID, TemplateID);
            UpdateChildrenParentID();
        }

        public int TemplateID { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.INSERT);
        }

    }

    public class DeleteMaterialCommand : DatabaseCommand {

        public DeleteMaterialCommand(int materialId) {
            this.MaterialID = materialId;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteMaterial(MaterialID);
        }

        public int MaterialID { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.INSERT);
        }

    }

    public class UpdateMaterialCommand : GenericDatabaseCommand<BioLink.Data.Model.Material> {

        public UpdateMaterialCommand(BioLink.Data.Model.Material model) : base(model) { }

        protected override void ProcessImpl(User user) {

            if (Preferences.AutoGenerateMaterialNames.Value) {
                Model.MaterialName = NameFormatter.FormatMaterialName(Model);
            }

            var service = new MaterialService(user);
            service.UpdateMaterial(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }

    public class MergeMaterialCommand : GenericDatabaseCommand<SiteExplorerNode> {

        public MergeMaterialCommand(SiteExplorerNode source, SiteExplorerNode dest)
            : base(source) {
            Dest = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.MergeMaterial(Model.ElemID, Dest.ElemID);
        }

        public SiteExplorerNode Dest { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_EXPLORER, PERMISSION_MASK.ALLOW);
        }

    }

    public class MoveMaterialCommand : GenericDatabaseCommand<SiteExplorerNode> {
        public MoveMaterialCommand(SiteExplorerNode model, SiteExplorerNode dest)
            : base(model) {
            this.Destination = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.MoveMaterial(Model.ElemID, Destination.ElemID);
        }

        public SiteExplorerNode Destination { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_EXPLORER, PERMISSION_MASK.ALLOW);
        }

    }

    public class InsertMaterialTemplateCommand : GenericDatabaseCommand<SiteExplorerNode> {
        public InsertMaterialTemplateCommand(SiteExplorerNode model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertMaterialTemplate();
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.INSERT);
        }

    }

    public class InsertRDEMaterialCommand : GenericDatabaseCommand<RDEMaterial> {

        public InsertRDEMaterialCommand(RDEMaterial model, RDESiteVisit owner) : base(model) {
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.SiteVisitID = Owner.SiteVisitID;
            Model.MaterialID = service.InsertMaterial(Model.SiteVisitID);
            service.UpdateMaterialRDE(Model);
        }

        protected RDESiteVisit Owner { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.INSERT);
        }

    }

    public class UpdateRDEMaterialCommand : GenericDatabaseCommand<RDEMaterial> {

        public UpdateRDEMaterialCommand(RDEMaterial model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            if (String.IsNullOrEmpty(Model.MaterialName) || Preferences.AutoGenerateMaterialNames.Value) {
                Model.MaterialName = NameFormatter.FormatMaterialName(Model);
            }

            service.UpdateMaterialRDE(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);        
        }

    }

    public class MoveRDEMaterialCommand : GenericDatabaseCommand<RDEMaterial> {

        public MoveRDEMaterialCommand(RDEMaterial model, RDESiteVisit newParent) : base(model) {
            this.NewParent = newParent;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.MoveMaterial(Model.MaterialID, NewParent.SiteVisitID);
        }

        protected RDESiteVisit NewParent { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }


}
