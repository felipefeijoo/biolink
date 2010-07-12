using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms.Integration;
using BioLink.Client.Extensibility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for BiolinkHost.xaml
    /// </summary>
    public partial class BiolinkHost : UserControl {

        private DebugControl _debug;
        private PluginManager _pluginManager;

        public BiolinkHost() {
            InitializeComponent();
        }

        public void StartUp() {
            

            _debug = new DebugControl();
            Debug.RegisterObserver(_debug);
            Debug.Log("Started!");            

            AvalonDock.DocumentContent debugContent = new AvalonDock.DocumentContent();
            debugContent.Title = "Debug";
            debugContent.Content = _debug;
            documentPane.Items.Add(debugContent);

            LoadPlugins();

            AvalonDock.DockableContent newContent = new AvalonDock.DockableContent();
            newContent.Title = "Site Explorer";
            this.explorersPane.Items.Add(newContent);

            webBrowser.Navigate("http://google.com.au");
            
        }

        private void LoadPlugins() {
            _pluginManager = new PluginManager();
            _pluginManager.LoadPlugins((message, percent, eventType) => { Debug.Log("<<{2}>> {0} {1}", message, (percent >= 0 ? "(" + percent + "%)" : ""), eventType); return true; }, AddPluginContributions);
        }

        void menuItem_Click(object sender, RoutedEventArgs e) {
            
        }

        private void AddPluginContributions(IBioLinkPlugin plugin) {
            Debug.Log("Looking for workspace contributions from {0}", plugin.Name);
            List<IWorkspaceContribution> contributions = plugin.Contributions;
            foreach (IWorkspaceContribution contrib in contributions) {
                if (contrib is WorkspaceMenuContribution) {
                    AddMenu(contrib as WorkspaceMenuContribution);
                    
                }
            }
        }

        private delegate int IndexAdjuster(int index, ItemCollection collection);

        private int IndexOfItemFromName(ItemCollection collection, string name, IndexAdjuster adjuster = null) {

            foreach (object item in collection) {                
                if (item is FrameworkElement) {
                    FrameworkElement element = item as FrameworkElement;                                        
                    if (element.Name.Equals(name)) {
                        if (adjuster != null) {
                            return adjuster(collection.IndexOf(element), collection);
                        } else {
                            return collection.IndexOf(element);
                        }
                    }
                }
            }

            return -1;
        }

        private int AdjustForPrecedingSeparator(int index, ItemCollection collection) {
            if (index > 0) {
                if (collection[index - 1] is Separator) {
                    return index - 1;
                }
            }
            return index;
        }

        private void AddMenu(WorkspaceMenuContribution menuDescriptor) {
            Debug.Log("Adding menu '{0}'", menuDescriptor.Name);
            ItemCollection parentCollection = menuBar.Items;
            MenuItem child = null;
            foreach (MenuItemDescriptor pathElement in menuDescriptor.Path) {
                child = FindMenuItem(parentCollection, pathElement.Name);
                if (child == null) {
                    child = new MenuItem();
                    child.Header = pathElement.Header;
                    child.Name = pathElement.Name;
                    int index = -1;

                    if (pathElement.InsertBefore != null) {
                        index = IndexOfItemFromName(parentCollection, pathElement.InsertBefore, AdjustForPrecedingSeparator);
                    } else if (pathElement.InsertAfter != null) {
                        index = IndexOfItemFromName(parentCollection, pathElement.InsertAfter);
                        if (index >= 0) {
                            index++;
                        }                        
                    }

                    if (index < 0) {
                        index = parentCollection.Add(child);
                    } else {
                        parentCollection.Insert(index, child);
                    }
                    
                    if (pathElement.SeparatorBefore) {
                        parentCollection.Insert(index, new Separator());
                    }

                    if (pathElement.SeparatorAfter) {
                        parentCollection.Insert(index + 1, new Separator());
                    }
                }
                parentCollection = child.Items;
            }
            child.Click += new RoutedEventHandler(menuDescriptor.Action);
        }

        private MenuItem FindMenuItem(ItemCollection parentCollection, string pathElement) {
            string name = (pathElement.StartsWith("_") ? pathElement.Substring(1) : pathElement);                        

            foreach (Object obj in parentCollection) {
                if (obj is MenuItem) {
                    MenuItem candidate = obj as MenuItem;
                    string candidateName = candidate.Header as string;
                    if (candidateName.StartsWith("_")) {
                        candidateName = candidateName.Substring(1);
                    }
                    if (candidateName.Equals(name, StringComparison.CurrentCultureIgnoreCase)) {
                        return candidate;
                    }
                }
            }
            return null;
        }

        private MenuItem createMenuItem(string caption) {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = caption;

            return menuItem;
        }
    }
}

