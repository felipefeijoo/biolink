using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Extensibility {

    public class AutoNumberBox : EllipsisTextBox {

        public AutoNumberBox() {
            InitializeComponent();
            Click += new System.Windows.RoutedEventHandler(AutoNumberBox_Click);
        }

        public void BindUser(User user, string autoNumberCategory) {
            this.AutoNumberCategory = autoNumberCategory;
            this.User = user;
        }

        void AutoNumberBox_Click(object sender, System.Windows.RoutedEventArgs e) {
            ShowAutoNumber();
        }

        private void ShowAutoNumber() {

            Debug.Assert(User != null, "User has not been set!");

            var frm = new AutoNumberOptions(User, AutoNumberCategory);
            frm.Owner = this.FindParentWindow();
            if (frm.ShowDialog().ValueOrFalse()) {
                txt.Text = frm.AutoNumber;                     
            }
        }

        public string AutoNumberCategory { get; set; }

        public User User { get; private set; }

    }
}
