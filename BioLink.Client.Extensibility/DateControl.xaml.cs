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
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for DateControl.xaml
    /// </summary>
    public partial class DateControl : UserControl {

        private bool _selfSet = false;

        public DateControl() {
            InitializeComponent();
            txt.TextChanged += new TextChangedEventHandler(txt_TextChanged);
            txt.LostFocus += new RoutedEventHandler(txt_LostFocus);
            cal.SelectionMode = CalendarSelectionMode.SingleDate;
            cal.SelectedDatesChanged += new EventHandler<SelectionChangedEventArgs>(cal_SelectedDatesChanged);
        }

        void cal_SelectedDatesChanged(object sender, SelectionChangedEventArgs e) {
            SelectDate();
        }

        void txt_LostFocus(object sender, RoutedEventArgs e) {
            txt.Text = DateToStr(Date);
        }

        void txt_TextChanged(object sender, TextChangedEventArgs e) {
            SetDate(txt.Text);   
        }

        private string DateToStr(string bldate) {
            if (bldate == null) {
                return "";
            }

            if (bldate.Length == 8) {                
                int year = Int32.Parse(bldate.Substring(0, 4));
                int month = Int32.Parse(bldate.Substring(4, 2));
                int day = Int32.Parse(bldate.Substring(6, 2));

                if (month == 0) {
                    return string.Format("{0:0000}", year);
                } else {
                    if (day == 0) {
                        return string.Format("{0}, {1:0000}", DateUtils.GetMonthName(month, true), year);
                    } else {
                        return string.Format("{0:00} {1}, {2:0000}", day, DateUtils.GetMonthName(month, true), year);
                    }
                }
            } else {
                return "";
            }
        }

        private void SetDate(string str) {

            var bits = str.Split(' ', '-', '/', '.', '\\', '_', ',', '%', '#', '!', '~', ';', ':');
            var strDate = String.Join(" ", bits);
            int count = 0;
            foreach (string s in bits) {
                if (!String.IsNullOrEmpty(s)) {
                    count++;
                }
            }
            
            DateTime dt;

            string bldate = null;

            if (DateTime.TryParse(strDate, out dt)) {
                switch (count) {
                    case 1:
                        bldate = string.Format("{0:4}0000", dt.Year);
                        break;
                    case 2:
                        bldate = string.Format("{0:0000}{1:00}00", dt.Year, dt.Month);
                        break;
                    case 3:
                        bldate = string.Format("{0:0000}{1:00}{2:00}", dt.Year, dt.Month, dt.Day);
                        break;
                    default:
                        break;
                }               
            } else {
                int year;
                if (Int32.TryParse(str, out year)) {
                    if (year > 0 && year <= 9999) {
                        var tempDate = "01 jan " + str;
                        if (DateTime.TryParse(tempDate, out dt)) {
                            bldate = string.Format("{0:0000}0000", dt.Year);
                        }
                    }
                }
            }

            _selfSet = true;
            if (bldate != null) {
                this.Date = bldate;
            } else {
                this.Date = "0";
            }
            _selfSet = false;
        }

        public string Date {
            get { return (string)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }

        public static readonly DependencyProperty DateProperty = DependencyProperty.Register("Date", typeof(string), typeof(DateControl), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnDateChanged)));

        private static void OnDateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = obj as DateControl;
            control.lblDebug.Content = control.Date;
            if (!control._selfSet) {
                control.txt.Text = control.DateToStr(control.Date);
            }
        }

        private void ToggleCalendar() {
            popup.IsOpen = !popup.IsOpen;
            if (popup.IsOpen) {
                popup.Focus();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            ToggleCalendar();
        }

        private void SelectDate() {
            if (cal.SelectedDate.HasValue) {
                var dt = cal.SelectedDate.Value;
                txt.Text = String.Format("{0:00} {1}, {2:0000}", dt.Day, DateUtils.GetMonthName(dt.Month, true), dt.Year);
                popup.IsOpen = false;
            }
        }

        private void popup_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                popup.IsOpen = false;
                e.Handled = true;
            } else if (e.Key == Key.Return) {
                SelectDate();
                e.Handled = true;
            }
        }

    }
}
