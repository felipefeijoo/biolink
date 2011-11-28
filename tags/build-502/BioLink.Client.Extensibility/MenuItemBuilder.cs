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
using System.Windows.Input;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class MenuItemBuilder {

        private MenuItem _menuItem;
        private MessageFormatterFunc _formatter;

        public MenuItemBuilder(MessageFormatterFunc formatter = null) {
            _formatter = formatter;
        }
        
        private string Format(string key, params object[] args) {
            if (_formatter != null) {
                return _formatter(key, args);
            } else {
                return String.Format(key, args);
            }
        }

        public MenuItemBuilder New(string caption, params object[] args) {

            if (_menuItem != null) {
                End();
            }

            _menuItem = new MenuItem();
            _menuItem.Header = Format(caption, args);
            return this;
        }

        public MenuItemBuilder Header(object header) {
            _menuItem.Header = header;
            return this;
        }

        public MenuItemBuilder Header(string format, params object[] args) {
            _menuItem.Header = Format(format, args);
            return this;
        }


        public MenuItemBuilder Handler(RoutedEventHandler handler) {
            _menuItem.Click += handler;
            return this;
        }

        public MenuItemBuilder Handler(Action handler) {
            _menuItem.Click += new RoutedEventHandler((source, e) => { handler(); });
            return this;
        }

        public MenuItemBuilder Enabled(bool enabled) {
            _menuItem.IsEnabled = enabled;
            return this;
        }

        public MenuItemBuilder Enabled<T>(Predicate<T> predicate, T obj) {
            _menuItem.IsEnabled = predicate(obj);
            return this;
        }

        public MenuItemBuilder Command(ICommand command) {
            _menuItem.Command = command;
            return this;
        }

        public MenuItemBuilder Checked(bool @checked) {
            _menuItem.IsChecked = @checked;
            return this;
        }

        public MenuItem MenuItem {
            get { return _menuItem; }
        }

        public void End() {
            if (EndAction != null && _menuItem != null) {
                EndAction(MenuItem);
            }
            _menuItem = null;
        }

        public Action<MenuItem> EndAction { get; set; }

    }

    public class ContextMenuBuilder {

        private ContextMenu _menu;
        private MessageFormatterFunc _formatter;
        private MenuItemBuilder _itemBuilder;

        public ContextMenuBuilder(MessageFormatterFunc messageFormatter, ContextMenu menu = null) {
            _formatter = messageFormatter;
            _menu = (menu == null ? new ContextMenu() : menu);
            _itemBuilder = new MenuItemBuilder(_formatter);
            _itemBuilder.EndAction = (menuItem) => {
                _menu.Items.Add(menuItem);
            };

        }

        public MenuItemBuilder New(string caption, params object[] args) {
            _itemBuilder.New(caption, args);
            return _itemBuilder;
        }

        public void Separator() {
            _itemBuilder.End();
            if (_menu.HasItems) {
                _menu.Items.Add(new Separator());
            }
        }

        public ContextMenu ContextMenu { 
            get { return _menu; } 
        }

        public bool HasItems {
            get { return _menu.HasItems; }
        }

        public void AddMenuItem(MenuItem menuItem) {
            _menu.Items.Add(menuItem);
        }

    }
}
