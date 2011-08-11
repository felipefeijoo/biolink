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
    /// Interaction logic for TimeControl.xaml
    /// </summary>
    public partial class TimeControl : UserControl {

        private bool _selfChanged;

        public TimeControl() {
            InitializeComponent();
            txt.LostFocus += new RoutedEventHandler(txt_LostFocus);
            txt.TextChanged += new TextChangedEventHandler(txt_TextChanged);
        }

        void txt_TextChanged(object sender, TextChangedEventArgs e) {
            _selfChanged = true;
            ConvertTime();
            _selfChanged = false;
        }

        void txt_LostFocus(object sender, RoutedEventArgs e) {
            ConvertTime();
            DisplayTime();
        }

        private void ConvertTime() {
            DateTime dt;
            if (DateTime.TryParse(txt.Text, out dt)) {
                Time = dt.Hour * 100 + dt.Minute;
            } else {
                Time = -1;
            }
        }

        public void DisplayTime() {
            if (Time < 0 || !Time.HasValue) {
                txt.Text = "";
                return;
            }

            int t = Time.Value;
            int hour = t / 100;
            int minute = t - (hour * 100);
            txt.Text = string.Format("{0:00}:{1:00}", hour, minute);
        }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(int?), typeof(TimeControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnTimeChanged)));

        private static void OnTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (TimeControl)obj;
            if (!control._selfChanged) {
                control.DisplayTime();
            }

        }

        public int? Time {
            get { return (int?) GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

    }
}
