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
using System.Windows.Media.Imaging;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public class Command {

        public Command(string caption, Action<object> action) {
            Caption = caption;
            CommandAction = action;
        }

        public Command(string caption, BitmapSource icon, Action<object> action) {
            Caption = caption;
            Icon = icon;
            CommandAction = action;
        }

        public string Caption { get; set; }
        public BitmapSource Icon { get; set; }
        public Action<object> CommandAction { get; set; }
    }

    // Placeholder class to indicate some kind of seperator between adjacent command when displayed visually (e.g. as menu items or tool bar items).
    public class CommandSeparator : Command {
        public CommandSeparator() : base("", null) { }
    }

}
