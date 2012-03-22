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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data.Model;
using BioLink.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for LinkedMultimediaItemsControl.xaml
    /// </summary>
    public partial class LinkedMultimediaItemsControl : UserControl, IIdentifiableContent {

        public LinkedMultimediaItemsControl(MultimediaLinkViewModel viewModel) {
            InitializeComponent();
            this.DataContext = viewModel;

            this.MultimediaID = viewModel.MultimediaID;            
            var service = new SupportService(User);

            var mm = service.GetMultimedia(MultimediaID);

            var items = service.ListItemsLinkedToMultimedia(MultimediaID);
            var model = new ObservableCollection<ViewModelBase>();

            foreach (MultimediaLinkedItem item in items) {
                if ( !string.IsNullOrWhiteSpace(item.CategoryName)) {
                    LookupType t;
                    if (Enum.TryParse<LookupType>(item.CategoryName, out t)) {
                        var vm = PluginManager.Instance.GetViewModel(t, item.IntraCatID);
                        if (vm!= null) {
                            model.Add(vm);
                            vm.Tag = t;
                        }
                    }
                }
            }

            lvw.MouseRightButtonUp += new MouseButtonEventHandler(lvw_MouseRightButtonUp);

            ListViewDragHelper.Bind(lvw, (dragged) => {

                if (dragged.Tag is LookupType) {
                    var lookupType = (LookupType) dragged.Tag;
                    var plugin = PluginManager.Instance.GetLookupTypeOwner(lookupType);

                    if (plugin != null) {
                        var data = new DataObject("Pinnable", dragged);
                        var pinnable = new PinnableObject(plugin.Name, lookupType, dragged.ObjectID.Value);
                        data.SetData(PinnableObject.DRAG_FORMAT_NAME, pinnable);
                        data.SetData(DataFormats.Text, dragged.DisplayLabel);
                        return data;
                    }
                }
                return null;
            });

            lvw.ItemsSource = model;
            CollectionView myView = (CollectionView)CollectionViewSource.GetDefaultView(lvw.ItemsSource);

            myView.SortDescriptions.Add(new SortDescription("Tag", ListSortDirection.Ascending));
            myView.SortDescriptions.Add(new SortDescription("DisplayLabel", ListSortDirection.Ascending));

            myView.GroupDescriptions.Add(new LinkedItemGroupDescription());

        }

        void lvw_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var selected = lvw.SelectedItem as ViewModelBase;
            if (selected != null) {
                var list = new List<ViewModelBase>();
                list.Add(selected);
                var commands = PluginManager.Instance.SolicitCommandsForObjects(list);
                if (commands.Count > 0) {
                    var builder = new ContextMenuBuilder(null);
                    foreach (Command loopvar in commands) {
                        Command cmd = loopvar; // include this in the closure scope, loopvar is outside, hence will always point to the last item...
                        if (cmd is CommandSeparator) {
                            builder.Separator();
                        } else {
                            builder.New(cmd.Caption).Handler(() => { cmd.CommandAction(selected); }).End();   
                        }                        
                    }
                    lvw.ContextMenu = builder.ContextMenu;
                }
            }
        }

        public User User { get { return PluginManager.Instance.User; } }

        public int MultimediaID { get; private set; }

        public string ContentIdentifier {
            get { return "ItemsForMultimedia:" + MultimediaID; }
        }

        public void RefreshContent() {
            throw new NotImplementedException();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            var window = this.FindParentWindow();
            if (window != null) {
                window.Close();
            }
        }

    }

    public class LinkedItemGroupDescription : System.ComponentModel.GroupDescription {

        public override object GroupNameFromItem(object item, int level, System.Globalization.CultureInfo culture) {
            var vm = item as ViewModelBase;
            if (vm != null && vm.Tag is LookupType) {
                return ((LookupType) vm.Tag).ToString();
            }

            return "Other";
        }
    }

}
