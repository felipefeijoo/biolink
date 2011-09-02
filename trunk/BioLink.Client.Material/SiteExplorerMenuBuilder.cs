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
using System.Windows.Controls;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.Generic;

namespace BioLink.Client.Material {

    public class SiteExplorerMenuBuilder {

        private static SiteExplorerNodeType NodeType(string elemType) {
            return (SiteExplorerNodeType)Enum.Parse(typeof(SiteExplorerNodeType), elemType);
        }

        private static SiteExplorerNodeType NodeType(SiteExplorerNodeViewModel model) {
            return NodeType(model.ElemType);
        }

        public static ContextMenu BuildForFavorite(SiteFavoriteViewModel fav, MaterialExplorer explorer) {
            if (fav == null) {
                return null;
            }

            ContextMenuBuilder builder = new ContextMenuBuilder(null);

            builder.New("Refresh").Handler(() => { explorer.Refresh(); }).End();

            if (!fav.IsGroup) {
                builder.Separator();

                builder.New("Rename").Handler(() => { fav.IsRenaming = true; }).End();

                // A little bit of a hack to reuse the edit code...simulate a site explorer node, although its not really there...
                SiteExplorerNode model = new SiteExplorerNode();
                model.ElemID = fav.ElemID;
                model.ElemType = fav.ElemType;
                model.Name = fav.Name;
                var node = new SiteExplorerNodeViewModel(model);

                var pinnable = explorer.CreatePinnable(node);
                if (pinnable != null) {
                    builder.Separator();
                    builder.New("_Pin to pin board").Handler(() => { PluginManager.Instance.PinObject(pinnable); });
                }
               
                var mnuReports = CreateReportMenuItems(node, explorer);
                if (mnuReports.HasItems) {
                    builder.Separator();
                    builder.AddMenuItem(mnuReports);
                }



                SiteExplorerNodeType type = (SiteExplorerNodeType)Enum.Parse(typeof(SiteExplorerNodeType), fav.ElemType);
                if (type != SiteExplorerNodeType.SiteGroup) {
                    builder.Separator();
                    builder.New("Edit Details...").Handler(() => {
                        switch (type) {
                            case SiteExplorerNodeType.Region: explorer.EditRegion(node);
                                break;
                            case SiteExplorerNodeType.Site: explorer.EditSite(node);
                                break;
                            case SiteExplorerNodeType.SiteVisit: explorer.EditSiteVisit(node);
                                break;
                            case SiteExplorerNodeType.Trap: explorer.EditTrap(node);
                                break;
                            case SiteExplorerNodeType.Material: explorer.EditMaterial(node);
                                break;
                            default:
                                throw new Exception("[Details] Unhandled site explorer element type: " + node.ElemType);
                        }
                    }).End();
                    var rdeNodeTypes = new List<SiteExplorerNodeType>(new SiteExplorerNodeType[] { SiteExplorerNodeType.Material, SiteExplorerNodeType.Site, SiteExplorerNodeType.SiteVisit });
                    if (node != null && rdeNodeTypes.Contains(node.NodeType)) {
                        builder.New("Open in Rapid Data Entry...").Handler(() => { explorer.EditRDE(node); }).End();
                    } else {
                        builder.New("Rapid Data Entry...").Handler(() => { explorer.EditRDE(node); }).End();
                    }
                }
            }

            return builder.ContextMenu;
        }

        public static ContextMenu Build(SiteExplorerNodeViewModel node, MaterialExplorer explorer) {

            if (node == null) {
                return null;
            }

            ContextMenuBuilder builder = new ContextMenuBuilder(null);

            builder.New("Refresh").Handler(() => { explorer.Refresh(); }).End();

            builder.Separator();

            builder.New("Rename").Handler(() => { node.IsRenaming = true; }).End();

            if (!node.IsTemplate) {
                var addMenu = CreateAddMenu(node, explorer);
                builder.AddMenuItem(addMenu);

                builder.Separator();
            }

            var type = NodeType(node);

            builder.New("Delete").Handler(() => {
                switch (type) {
                    case SiteExplorerNodeType.Region: explorer.DeleteRegion(node);
                        break;
                    case SiteExplorerNodeType.SiteGroup: explorer.DeleteSiteGroup(node);
                        break;
                    case SiteExplorerNodeType.Site: explorer.DeleteSite(node);
                        break;
                    case SiteExplorerNodeType.SiteVisit: explorer.DeleteSiteVisit(node);
                        break;
                    case SiteExplorerNodeType.Trap: explorer.DeleteTrap(node);
                        break;
                    case SiteExplorerNodeType.Material: explorer.DeleteMaterial(node);
                        break;
                    default:
                        throw new Exception("[Delete] Unhandled site explorer element type: " + node.ElemType);
                }
            });

            if (!node.IsTemplate) {
                var pinnable = explorer.CreatePinnable(node);
                if (pinnable != null) {
                    builder.Separator();
                    builder.New("_Pin to pin board").Handler(() => { PluginManager.Instance.PinObject(pinnable); });
                }

                builder.Separator();
                builder.AddMenuItem(CreateFavoriteMenuItems(explorer, node));

                var mnuReports = CreateReportMenuItems(node, explorer);
                if (mnuReports.HasItems) {
                    builder.Separator();
                    builder.AddMenuItem(mnuReports);
                }

                builder.Separator();
                builder.AddMenuItem(CreateTemplateItems(explorer));
            }


            if (type != SiteExplorerNodeType.SiteGroup) {
                builder.Separator();
                builder.New("Edit Details...").Handler(() => {
                    switch (type) {
                        case SiteExplorerNodeType.Region: explorer.EditRegion(node);
                            break;
                        case SiteExplorerNodeType.Site: explorer.EditSite(node);
                            break;
                        case SiteExplorerNodeType.SiteVisit: explorer.EditSiteVisit(node);
                            break;
                        case SiteExplorerNodeType.Trap: explorer.EditTrap(node);
                            break;
                        case SiteExplorerNodeType.Material: explorer.EditMaterial(node);
                            break;
                        default:
                            throw new Exception("[Details] Unhandled site explorer element type: " + node.ElemType);
                    }
                }).End();

                var rdeNodeTypes = new List<SiteExplorerNodeType>(new SiteExplorerNodeType[] { SiteExplorerNodeType.Material, SiteExplorerNodeType.Site, SiteExplorerNodeType.SiteVisit });
                if (node != null && rdeNodeTypes.Contains(node.NodeType)) {
                    builder.New("Open in Rapid Data Entry...").Handler(() => { explorer.EditRDE(node); }).Enabled(!node.IsTemplate).End();
                } else {
                    builder.New("Rapid Data Entry...").Handler(() => { explorer.EditRDE(node); }).End();
                }
                
            }

            return builder.ContextMenu;
        }

        public static MenuItem CreateTemplateItems(MaterialExplorer explorer) {
            MenuItemBuilder builder = new MenuItemBuilder();
            MenuItem tmp = builder.New("Create Template").MenuItem;
            tmp.Items.Add(builder.New("_Site").Handler(() => { explorer.AddSiteTemplate(); }).MenuItem);
            tmp.Items.Add(builder.New("Site _Visit").Handler(() => { explorer.AddSiteVisitTemplate(); }).MenuItem);
            tmp.Items.Add(builder.New("_Material").Handler(() => { explorer.AddMaterialTemplate(); }).MenuItem);

            return tmp;
        }

        private static MenuItem CreateFavoriteMenuItems(MaterialExplorer explorer, SiteExplorerNodeViewModel node) {
            MenuItemBuilder builder = new MenuItemBuilder();
            MenuItem add = builder.New("Add to favorites").MenuItem;
            add.Items.Add(builder.New("User specific").Handler(() => { explorer.Favorites.AddToFavorites(node, false); }).MenuItem);
            add.Items.Add(builder.New("Global").Handler(() => { explorer.Favorites.AddToFavorites(node, true); }).MenuItem);
            return add;
        }


        private static MenuItem CreateReportMenuItems(SiteExplorerNodeViewModel node, MaterialExplorer explorer) {
            var builder = new MenuItemBuilder();

            MenuItem reports = builder.New("Reports").MenuItem;
            var list = explorer.Owner.GetReportsForNode(node);
            foreach (IBioLinkReport report in list) {
                IBioLinkReport reportToExecute = report;
                reports.Items.Add(builder.New(report.Name).Handler(() => { PluginManager.Instance.RunReport(explorer.Owner, reportToExecute); }).MenuItem);
            }

            return reports;
        }


        private static MenuItem CreateAddMenu(SiteExplorerNodeViewModel viewModel, MaterialExplorer explorer) {
            var addMenu = new MenuItem();
            addMenu.Header = "Add";

            var user = PluginManager.Instance.User;
            var service = new MaterialService(user);                        

            var builder = new MenuItemBuilder();

            switch (NodeType(viewModel)) {
                case SiteExplorerNodeType.Region:
                    addMenu.Items.Add(builder.New("New Region").Handler(() => { explorer.AddRegion(viewModel); }).MenuItem);
                    addMenu.Items.Add(builder.New("New Site Group").Handler(() => { explorer.AddSiteGroup(viewModel); }).MenuItem);
                    var addSite = builder.New("_Site").MenuItem;
                    addSite.Items.Add(builder.New("_Blank").Handler(() => { explorer.AddSite(viewModel); }).MenuItem);
                    var lastSiteTemplateId = Config.GetUser(user, "SiteExplorer.LastSiteTemplate", -1);
                    if (lastSiteTemplateId > 0) {
                        var siteTemplate = service.GetSite(lastSiteTemplateId);
                        if (siteTemplate != null) {
                            addSite.Items.Add(builder.New("As '" + siteTemplate.SiteName + "'").Handler(() => {
                                explorer.AddSite(viewModel, lastSiteTemplateId);
                            }).MenuItem);
                        }
                    }

                    addSite.Items.Add(builder.New("From _Template").Handler(() => {
                        int? templateId = explorer.ChooseTemplate(SiteExplorerNodeType.Site);
                        if (templateId.HasValue) {
                            explorer.AddSite(viewModel, templateId.Value);
                            Config.SetUser(PluginManager.Instance.User, "SiteExplorer.LastSiteTemplate", templateId.Value);
                        }                        
                    }).MenuItem);
                    addMenu.Items.Add(addSite);
                    break;
                case SiteExplorerNodeType.SiteGroup:
                    addMenu.Items.Add(builder.New("New Site Group").Handler(() => { explorer.AddSiteGroup(viewModel); }).MenuItem);
                    addSite = builder.New("_Site").MenuItem;
                    addSite.Items.Add(builder.New("_Blank").Handler(() => { explorer.AddSite(viewModel); }).MenuItem);

                    lastSiteTemplateId = Config.GetUser(user, "SiteExplorer.LastSiteTemplate", -1);
                    if (lastSiteTemplateId > 0) {
                        var siteTemplate = service.GetSite(lastSiteTemplateId);
                        if (siteTemplate != null) {
                            addSite.Items.Add(builder.New("As '" + siteTemplate.SiteName + "'").Handler(() => {
                                explorer.AddSite(viewModel, lastSiteTemplateId);
                            }).MenuItem);
                        }
                    }

                    addSite.Items.Add(builder.New("From _Template").Handler(() => {
                        int? templateId = explorer.ChooseTemplate(SiteExplorerNodeType.Site);
                        if (templateId.HasValue) {
                            explorer.AddSite(viewModel, templateId.Value);
                            Config.SetUser(PluginManager.Instance.User, "SiteExplorer.LastSiteTemplate", templateId.Value);
                        }                        
                    }).MenuItem);
                    addMenu.Items.Add(addSite);
                    break;
                case SiteExplorerNodeType.Site:
                    addMenu.Items.Add(builder.New("New Trap").Handler(() => { explorer.AddTrap(viewModel); }).MenuItem);

                    var addVisit = builder.New("Site _Visit").MenuItem;
                    addVisit.Items.Add(builder.New("_Blank").Handler(() => { explorer.AddSiteVisit(viewModel); }).MenuItem);

                    var lastSiteVisitTemplateId = Config.GetUser(user, "SiteExplorer.LastSiteVisitTemplate", -1);
                    if (lastSiteVisitTemplateId > 0) {
                        var siteVisitTemplate = service.GetSiteVisit(lastSiteVisitTemplateId);
                        if (siteVisitTemplate != null) {
                            addVisit.Items.Add(builder.New("As '" + siteVisitTemplate.SiteVisitName + "'").Handler(() => {
                                explorer.AddSiteVisit(viewModel, lastSiteVisitTemplateId);
                            }).MenuItem);
                        }
                    }

                    addVisit.Items.Add(builder.New("From _Template").Handler(() => {
                        int? templateId = explorer.ChooseTemplate(SiteExplorerNodeType.SiteVisit);
                        if (templateId.HasValue) {                            
                            explorer.AddSiteVisit(viewModel, templateId.Value);
                            Config.SetUser(user, "SiteExplorer.LastSiteVisitTemplate", templateId.Value);
                        }                        
                    }).MenuItem);
                    addMenu.Items.Add(addVisit);
                    
                    break;
                case SiteExplorerNodeType.SiteVisit:
                    var addMaterial = builder.New("_Material").MenuItem;
                    addMaterial.Items.Add(builder.New("_Blank").Handler(() => { explorer.AddMaterial(viewModel); }).MenuItem);

                    var lastMaterialTemplateId = Config.GetUser(user, "SiteExplorer.LastMaterialTemplate", -1);
                    if (lastMaterialTemplateId > 0) {
                        var materialTemplate = service.GetMaterial(lastMaterialTemplateId);
                        if (materialTemplate != null) {
                            addMaterial.Items.Add(builder.New("As '" + materialTemplate.MaterialName + "'").Handler(() => {
                                explorer.AddMaterial(viewModel, lastMaterialTemplateId);
                            }).MenuItem);
                        }
                    }

                    addMaterial.Items.Add(builder.New("From _Template").Handler(() => {
                        int? templateId = explorer.ChooseTemplate(SiteExplorerNodeType.Material);
                        if (templateId.HasValue) {
                            explorer.AddMaterial(viewModel, templateId.Value);
                            Config.SetUser(user, "SiteExplorer.LastMaterialTemplate", templateId);
                        }                        
                    }).MenuItem);
                    addMenu.Items.Add(addMaterial);
                    break;
                default:
                    break;
            }


            return addMenu;
        }
    }

}
