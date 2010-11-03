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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for NumberUpDown.xaml
    /// </summary>
    public partial class NumberUpDown : UserControl {

        public NumberUpDown() {
            InitializeComponent();
            Delta = 1;
            AllowNegative = false;
            txt.TextChanged += new TextChangedEventHandler(txt_TextChanged);
        }

        void txt_TextChanged(object sender, TextChangedEventArgs e) {
            int val;
            if (Int32.TryParse(txt.Text, out val)) {
                Number = val;
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e) {
            NumberUp();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e) {
            NumberDown();
        }

        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register("Number", typeof(int), typeof(NumberUpDown), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnNumberChanged)));

        public int Number {
            get { return (int) GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value); }
        }

        private static void OnNumberChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (NumberUpDown)obj;
            control.txt.Text = "" + control.Number;
        }

        private void NumberUp() {           
            Number += Delta;
        }

        private void NumberDown() {
            var temp = Number - Delta;
            if (temp < 0 && !AllowNegative) {
                temp = 0;
            }
            Number = temp;
        }

        public int Delta { get; set; }

        public bool AllowNegative { get; set; }

        public bool HasValue {
            get {
                if (string.IsNullOrEmpty(txt.Text)) {
                    return false;
                }

                int val;
                if (!Int32.TryParse(txt.Text, out val)) {
                    return false;
                }

                return true;
            }
        }
    }
}
