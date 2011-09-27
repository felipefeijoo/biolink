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
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using AvalonDock;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for BiolinkHost.xaml
    /// </summary>
    public partial class BiolinkHost : UserControl, IDisposable {

        private static string PREF_DOCK_LAYOUT = "client.dock.layout";

        private PluginManager _pluginManager;
        public User User { get; set; }

        public BiolinkHost() {                
            InitializeComponent();
            dockManager.IsAnimationEnabled = true;
        }

        public void StartUp() {
            PluginLoaderWindow progress = new PluginLoaderWindow();
            progress.Owner = MainWindow.Instance;
            progress.Show();
            LoadPluginsAsync(progress, () => { 
                progress.Hide();
                String layout = Config.GetUser<string>(User, PREF_DOCK_LAYOUT, null);
                if (!String.IsNullOrEmpty(layout)) {
                    StringReader reader = new StringReader(layout);                    
                    dockManager.RestoreLayout(reader);
                }

            });

            txtProfile.Text = String.Format("Connected to {0}\\{1} ({2})", User.ConnectionProfile.Server, User.ConnectionProfile.Database, User.Username);

        }

        private void LoadPluginsAsync(IProgressObserver monitor, Action finished) {

            Debug.Assert(User != null, "User is null!");


            PluginManager.Initialize(this.User, MainWindow.Instance);

            _pluginManager = PluginManager.Instance;

            _pluginManager.RequestShowContent += new PluginManager.ShowDockableContributionDelegate(_pluginManager_RequestShowContent);
            _pluginManager.DocumentContentAdded += new PluginManager.AddDockableContentDelegate(_pluginManager_DocumentContentAdded);
            _pluginManager.DockableContentClosed += new PluginManager.CloseDockableContentDelegate(_pluginManager_DockableContentClosed);
            _pluginManager.ProgressEvent += ProgressObserverAdapter.Adapt(monitor);
            // Debug logging...            
            _pluginManager.ProgressEvent += (message, percent, eventType) => { Logger.Debug("<<{2}>> {0} {1}", message, (percent >= 0 ? "(" + percent + "%)" : ""), eventType); return true; };
            Thread t = new Thread(new ThreadStart(() => {
                this.InvokeIfRequired(() => {
                    this.explorersPane.Width = 0;
                    _pluginManager.LoadPlugins(AddPluginContributions);
                    if (finished != null) {
                        finished();
                    }
                    explorersPane.Width = 180;
                });
            }));
            t.Name = "Plugin Bootstrapper Thread";
            t.TrySetApartmentState(ApartmentState.STA);
            t.Start();
        }

        void _pluginManager_DockableContentClosed(FrameworkElement content) {
            DocumentContent doc = dockManager.Documents.First((a) => { return a.Content == content; });
            if (doc != null) {
                doc.Close();
            }
        }

        void _pluginManager_DocumentContentAdded(IBioLinkPlugin plugin, FrameworkElement content, string title, bool isClosable) {

            var newContent = new BiolinkDocumentContent {
                Title = title, 
                Content = content,
                FloatingWindowSize = new Size(600,500),
                IsCloseable = isClosable
            };

            newContent.Closed += new EventHandler((e,a) => {
                if (content is IDisposable) {
                    (content as IDisposable).Dispose();
                }
            });

            bool addAsFloating = Config.GetUser(User, "client.dock.AddDocumentContentAsFloating", false);

            newContent.Show(dockManager, addAsFloating);

            newContent.BringIntoView();
            newContent.Focus();
            
        }

        void _pluginManager_RequestShowContent(IBioLinkPlugin plugin, string name) {
            String contentName = plugin.Name + "_" + name;
            AvalonDock.DockableContent content = dockManager.DockableContents.FirstOrDefault((candidate) => {
                return candidate.Name.Equals(contentName);
            });

            if (content != null) {
                content.Show();
            }
        }


        private void AddPluginContributions(IBioLinkPlugin plugin) {
            Logger.Debug("Looking for workspace contributions from {0}", plugin.Name);
            List<IWorkspaceContribution> contributions = plugin.GetContributions();
            try {
                
                foreach (IWorkspaceContribution contrib in contributions) {
                    if (contrib is MenuWorkspaceContribution) {
                        AddMenu(contrib as MenuWorkspaceContribution);
                    } else if (contrib is IExplorerWorkspaceContribution) {
                        IExplorerWorkspaceContribution explorer = contrib as IExplorerWorkspaceContribution;
                        DockableContent newContent = new DockableContent();
                        newContent.Title = explorer.Title;
                        newContent.Content = explorer.Content;
                        newContent.Name = plugin.Name + "_" + explorer.Name;

                        this.explorersPane.Items.Add(newContent);

                        JobExecutor.QueueJob(() => {
                            explorer.InitializeContent();
                        });
                    }
                    
                }                
            } finally {                
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

        private void Invoke(DispatcherObject element, Action action) {
            element.Dispatcher.Invoke(action, null);
        }

        private void AddMenu(MenuWorkspaceContribution menuDescriptor) {
            Logger.Debug("Adding menu '{0}'", menuDescriptor.Name);
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
                    if (candidate.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) {
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

        private void Exit_Click(object sender, RoutedEventArgs e) {
            ExitBiolink();
        }

        public void ExitBiolink() {
            MainWindow.Instance.Shutdown();
        }

        public bool RequestShutdown() {
            return _pluginManager.RequestShutdown();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BiolinkHost() {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_pluginManager != null) {
                    _pluginManager.Dispose();
                }

                StringWriter writer = new StringWriter();
                dockManager.SaveLayout(writer);
                Config.SetUser<string>(User, PREF_DOCK_LAYOUT, writer.ToString());
            }
        }

        private void LogOut_Click(object sender, RoutedEventArgs e) {
            MainWindow.Instance.LogOut();
        }

        private void About_Click(object sender, RoutedEventArgs e) {
            ShowAbout();
        }

        private void ShowAbout() {
            var frm = new About();
            frm.Owner = this.FindParentWindow();
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            frm.ShowDialog();
        }
       
    }

    public class BiolinkDocumentContent : DocumentContent {

        public BiolinkDocumentContent() {
        }

    }

}

