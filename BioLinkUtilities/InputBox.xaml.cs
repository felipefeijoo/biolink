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

namespace BioLink.Client.Utilities {
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window {
        public InputBox() {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(InputBox_Loaded);
        }

        void InputBox_Loaded(object sender, RoutedEventArgs e) {
            txt.Focus();
        }

        public static void Show(Window owner, string title, object question, Action<string> handler) {
            Show(owner, title, question, null, handler);
        }

        public static void Show(Window owner, string title, object question, string prefill, Action<string> handler) {
            InputBox frm = new InputBox();
            frm.Owner = owner;
            frm.Title = title;
            frm.lblQuestion.Content = question;

            if (!String.IsNullOrEmpty(prefill)) {
                frm.txt.Text = prefill;
                frm.txt.SelectAll();
            }

            if (frm.ShowDialog().GetValueOrDefault(false)) {
                if (handler != null) {
                    handler(frm.txt.Text);
                }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Hide();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Hide();
        }

    }
}
