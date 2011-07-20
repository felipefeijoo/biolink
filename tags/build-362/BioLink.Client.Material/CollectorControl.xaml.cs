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
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for CollectorControl.xaml
    /// </summary>
    public partial class CollectorControl : UserControl {

        public CollectorControl() {
            InitializeComponent();
            txt.PreviewKeyDown += new KeyEventHandler(txt_PreviewKeyDown);
            txt.TextChanged += new TextChangedEventHandler(txt_TextChanged);
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e) {
            Text = txt.Text;
        }

        void txt_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Space && (Keyboard.Modifiers & ModifierKeys.Control) > 0) {
                e.Handled = true;
                txt.SelectAll(); // We do this so that the selection will overwrite the initial filter that was typed in...
                DoFind(true);
            }
        }

        public void BindUser(User user) {
            User = user;
        }

        private void btn_Click(object sender, RoutedEventArgs e) {
            DoFind(false);
        } 

        private void DoFind(bool PositionUnderControl) {

            var preSelectionLength = txt.SelectionLength;

            Func<IEnumerable<string>> itemsFunc = () => {
                var service = new MaterialService(User);
                return service.GetDistinctCollectors();
            };

            Control owner = null;
            if (PositionUnderControl) {
                owner = txt;
            }

            PickListWindow frm = new PickListWindow(User, "Select a collector", itemsFunc, null, txt.Text, owner, this);            
                       
            if (frm.ShowDialog().ValueOrFalse()) {
                if (preSelectionLength == 0 && !string.IsNullOrWhiteSpace(txt.Text)) {
                    txt.Text += ", " + frm.SelectedValue;
                } else {
                    txt.SelectedText = frm.SelectedValue as string;
                }
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CollectorControl), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnTextChanged)));

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (CollectorControl) obj;
            control.txt.Text = args.NewValue as String;            
        }

        public String Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(CollectorControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (CollectorControl)obj;
            if (control != null) {
                control.btn.IsEnabled = !(bool)args.NewValue;
                control.txt.IsReadOnly = (bool)args.NewValue;
            }
        }


        protected User User { get; private set; }
    }
}
