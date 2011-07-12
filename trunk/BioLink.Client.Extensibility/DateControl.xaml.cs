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
using Microsoft.VisualBasic;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for DateControl.xaml
    /// </summary>
    public partial class DateControl : UserControl {

        private bool _selfSet = false;
        private KeyEventHandler _keyHandler;

        public DateControl() {
            InitializeComponent();
            txt.TextChanged += new TextChangedEventHandler(txt_TextChanged);
            txt.PreviewLostKeyboardFocus += new KeyboardFocusChangedEventHandler(txt_PreviewLostKeyboardFocus);
            txt.LostFocus += new RoutedEventHandler(txt_LostFocus);
            cal.SelectionMode = CalendarSelectionMode.SingleDate;
            cal.SelectedDatesChanged += new EventHandler<SelectionChangedEventArgs>(cal_SelectedDatesChanged);
            cal.MouseDoubleClick += new MouseButtonEventHandler(cal_MouseDoubleClick);
        }

        void txt_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            
            if (IsHardDate) {
                if (!string.IsNullOrEmpty(Date)) {
                    int year = Int32.Parse(Date.Substring(0, 4));
                    int month = Int32.Parse(Date.Substring(4, 2));
                    int day = Int32.Parse(Date.Substring(6, 2));
                    if (year == 0 || month == 0 || day == 0) {
                        e.Handled = true;
                        return;
                    }
                }
            }

        }

        void DateControl_LostFocus(object sender, RoutedEventArgs e) {
            if (popup.IsOpen) {
                popup.IsOpen = false;
            }
        }

        void cal_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

            var point = e.GetPosition(cal);
            if (point.Y >= 58) { // Ignore the double click if it occurs in the title bar (month selector part) of the date control
                SelectDate();
            } else {
                this.CaptureMouse();
                this.ReleaseMouseCapture();
            }
        }

        void window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (popup.IsOpen) {
                if (e.Key == Key.Escape) {
                    popup.IsOpen = false;
                    e.Handled = true;
                } else if (e.Key == Key.Return) {
                    SelectDate();
                    e.Handled = true;
                }
            }
        }

        void cal_SelectedDatesChanged(object sender, SelectionChangedEventArgs e) {
            this.CaptureMouse();
            this.ReleaseMouseCapture();
        }

        void txt_LostFocus(object sender, RoutedEventArgs e) {
            txt.Text = DateToStr(Date);
        }

        void txt_TextChanged(object sender, TextChangedEventArgs e) {
            SetDate(txt.Text);
        }

        public void SetToToday() {
            Date = DateUtils.DateToBLDate(DateTime.Now);
        }

        public static string DateToStr(string bldate) {
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
                        return string.Format("{0} {1}, {2:0000}", day, DateUtils.GetMonthName(month, true), year);
                    }
                }
            } else {
                return "";
            }
        }

        public void SetDate(string str) {

            if (str == null) {
                return;
            }

            if (str.Equals("today", StringComparison.CurrentCultureIgnoreCase)) {
                str = DateTime.Now.ToShortDateString();
            } 

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

            if (Information.IsDate(strDate)) {
                dt = Microsoft.VisualBasic.DateAndTime.DateValue(strDate);
                if (DateAndTime.Day(dt) == 1) {
                    switch (count) {
                        case 1:
                            bldate = string.Format("{0:4}0000", dt.Year);
                            break;
                        case 2:
                            bldate = string.Format("{0:0000}{1:00}00", dt.Year, dt.Month);
                            break;
                        case 3:
                            bldate = DateUtils.DateToBLDate(dt);
                            break;
                        default:
                            break;
                    }
                } else {
                    bldate = DateUtils.DateToBLDate(dt);
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
                control.txt.Text = DateControl.DateToStr(control.Date);
            }
        }

        public bool IsHardDate {
            get { return (bool)GetValue(IsHardDateProperty); }
            set { SetValue(IsHardDateProperty, value); }
        }

        public static readonly DependencyProperty IsHardDateProperty = DependencyProperty.Register("IsHardDate", typeof(bool), typeof(DateControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsHardDateChanged)));

        private static void OnIsHardDateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = obj as DateControl;
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(DateControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (DateControl)obj;
            if (control != null) {
                control.btnChooseDate.IsEnabled = !(bool)args.NewValue;
            }
        }

        private void ToggleCalendar() {
            popup.IsOpen = !popup.IsOpen;
            var window = this.FindParentWindow();
            if (popup.IsOpen) {
                if (window != null && _keyHandler == null) {
                    _keyHandler = new KeyEventHandler(window_PreviewKeyDown);
                    window.PreviewKeyDown += _keyHandler;
                    window.PreviewKeyUp += _keyHandler;
                }
                popup.Focus();
                var dt = GetDateAsDateTime();
                if (dt.HasValue) {
                    cal.SelectedDate = dt.Value;
                    cal.DisplayDate = dt.Value;
                }
            } else {                
                if (window != null && _keyHandler != null) {
                    window.PreviewKeyDown -= _keyHandler;
                    window.PreviewKeyUp -= _keyHandler;
                }
            }
        }

        public DateTime? GetDateAsDateTime() {
            var str = Date as string;
            if (str != null) {
                var formatted = DateControl.DateToStr(str);
                DateTime dt;
                if (DateTime.TryParse(formatted, out dt)) {
                    return dt;
                } else {
                    int year;
                    if (Int32.TryParse(formatted, out year)) {
                        if (year > 0 && year <= 9999) {
                            var tempDate = "01 jan " + formatted;
                            if (DateTime.TryParse(tempDate, out dt)) {
                                return dt;
                            }
                        }
                    }
                }
            }
            return null;
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

        private void btnAccept_Click(object sender, RoutedEventArgs e) {
            SelectDate();
        }

        private void btnCanel_Click(object sender, RoutedEventArgs e) {
            popup.IsOpen = false;
        }

    }

    public class BLDateIntStrConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int? date = (int?)value;
            if (date.HasValue) {
                return string.Format("{0}", date);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int result = 0;
            Int32.TryParse(value as string, out result);
            return result;
        }
    }

    public class HardDateConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var dt = value as DateTime?;
            if (dt != null && dt.HasValue) {
                return DateUtils.DateToBLDate(dt.Value);
            } else {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var str = value as string;
            if (str != null) {
                var formatted = DateControl.DateToStr(str);
                DateTime dt;
                if (DateTime.TryParse(formatted, out dt)) {
                    return dt;
                } else {
                    int year;
                    if (Int32.TryParse(formatted, out year)) {
                        if (year > 0 && year <= 9999) {
                            var tempDate = "01 jan " + formatted;
                            if (DateTime.TryParse(tempDate, out dt)) {
                                return dt;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
