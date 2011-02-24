using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace BioLink.Client.Extensibility {

    public class FilenameTextBox : EllipsisTextBox {

        public FilenameTextBox() : base() {
            this.Click += new System.Windows.RoutedEventHandler(FilenameTextBox_Click);
            this.Mode = FilenameTextBoxMode.Open;
            this.Filter = "All files (*.*)|*.*";
        }

        void FilenameTextBox_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (Mode == FilenameTextBoxMode.Open) {
                var dlg = new OpenFileDialog();
                dlg.Filter = Filter;
                if (dlg.ShowDialog().GetValueOrDefault(false)) {
                    this.txt.Text = dlg.FileName;
                }                
            } else {
                var dlg = new SaveFileDialog();
                dlg.Filter = Filter;
                if (dlg.ShowDialog().GetValueOrDefault(false)) {
                    this.txt.Text = dlg.FileName;
                }
            }
        }

        public FilenameTextBoxMode Mode { get; set; }

        public string Filter { get; set; }

    }

    public enum FilenameTextBoxMode {
        Open, Save
    }
}
