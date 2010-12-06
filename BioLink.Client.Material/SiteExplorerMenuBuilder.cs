using System;
using System.Windows.Controls;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class SiteExplorerMenuBuilder {

        private static SiteExplorerNodeType NodeType(string elemType) {
            return (SiteExplorerNodeType)Enum.Parse(typeof(SiteExplorerNodeType), elemType);
        }

        private static SiteExplorerNodeType NodeType(SiteExplorerNodeViewModel model) {
            return NodeType(model.ElemType);
        }

        public static ContextMenu Build(SiteExplorerNodeViewModel node, MaterialExplorer explorer) {

            if (node == null) {
                return null;
            }

            ContextMenuBuilder builder = new ContextMenuBuilder(null);

            builder.New("Refresh").Handler(() => { explorer.Refresh(); }).End();

            builder.Separator();

            builder.New("Rename").Handler(() => { node.IsRenaming = true; }).End();

            var addMenu = CreateAddMenu(node, explorer);
            builder.AddMenuItem(addMenu);

            builder.Separator();

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
                    default:
                        throw new Exception("[Delete] Unhandled site explorer element type: " + node.ElemType);
                }
            });

            var pinnable = explorer.CreatePinnable(node);
            if (pinnable != null) {
                builder.Separator();
                builder.New("_Pin to pin board").Handler(() => { PluginManager.Instance.PinObject(pinnable); });
            }



            if (type != SiteExplorerNodeType.SiteGroup) {
                builder.Separator();
                builder.New("Details...").Handler(() => {
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
            }

            return builder.ContextMenu;
        }

        private static MenuItem CreateAddMenu(SiteExplorerNodeViewModel viewModel, MaterialExplorer explorer) {
            var addMenu = new MenuItem();
            addMenu.Header = "Add";

            var builder = new MenuItemBuilder();

            switch (NodeType(viewModel)) {
                case SiteExplorerNodeType.Region:
                    addMenu.Items.Add(builder.New("New Region").Handler(() => { explorer.AddRegion(viewModel); }).MenuItem);
                    addMenu.Items.Add(builder.New("New Site Group").Handler(() => { explorer.AddSiteGroup(viewModel); }).MenuItem);
                    addMenu.Items.Add(builder.New("New Site").Handler(() => { explorer.AddSite(viewModel); }).MenuItem);
                    break;
                case SiteExplorerNodeType.SiteGroup:
                    addMenu.Items.Add(builder.New("New Site Group").Handler(() => { explorer.AddSiteGroup(viewModel); }).MenuItem);
                    addMenu.Items.Add(builder.New("New Site").Handler(() => { explorer.AddSite(viewModel); }).MenuItem);
                    break;
                case SiteExplorerNodeType.Site:
                    addMenu.Items.Add(builder.New("New Site Visit").Handler(() => { explorer.AddSiteVisit(viewModel); }).MenuItem);
                    addMenu.Items.Add(builder.New("New Trap").Handler(() => { explorer.AddTrap(viewModel); }).MenuItem);
                    break;
                default:
                    break;
            }


            return addMenu;
        }
    }

}
