using System;
using System.Collections.Generic;
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

    public partial class CustomColorPicker : UserControl {

        public event Action<Color> SelectedColorChanged;

        public event Action<Color> ColorSelected;

        String _hexValue = string.Empty;

        public String HexValue {
            get { return _hexValue; }
            set { _hexValue = value; }
        }

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(CustomColorPicker), new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectedColorChanged)));

        public Color SelectedColor {
            get { return (Color) GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        private static void OnSelectedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (CustomColorPicker) obj;
            control.recContent.Fill = new SolidColorBrush(control.SelectedColor);
            if (control.SelectedColorChanged != null) {
                control.SelectedColorChanged(control.SelectedColor);
                control.HexValue = string.Format("#{0}", control.SelectedColor.ToString().Substring(1));
            }
        }

        bool _isContexMenuOpened = false;
        public CustomColorPicker() {
            InitializeComponent();
            b.ContextMenu.Closed += new RoutedEventHandler(ContextMenu_Closed);
            b.ContextMenu.Opened += new RoutedEventHandler(ContextMenu_Opened);
            b.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(b_PreviewMouseLeftButtonUp);
        }

        void ContextMenu_Opened(object sender, RoutedEventArgs e) {
            _isContexMenuOpened = true;
        }

        void ContextMenu_Closed(object sender, RoutedEventArgs e) {
            if (!b.ContextMenu.IsOpen) {
                SelectedColor = cp.CustomColor;
                if (ColorSelected != null) {
                    ColorSelected(cp.CustomColor);
                }
            }
            _isContexMenuOpened = false;
        }

        void b_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (!_isContexMenuOpened) {
                if (b.ContextMenu != null && b.ContextMenu.IsOpen == false) {
                    b.ContextMenu.PlacementTarget = b;
                    b.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    ContextMenuService.SetPlacement(b, System.Windows.Controls.Primitives.PlacementMode.Bottom);
                    b.ContextMenu.IsOpen = true;
                }
            }
        }
    }
}
