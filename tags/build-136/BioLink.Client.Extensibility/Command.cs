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
        public CommandSeparator()
            : base("", null) {
        }
    }

}
