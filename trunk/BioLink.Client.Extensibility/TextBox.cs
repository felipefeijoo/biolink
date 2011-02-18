using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Threading;
using BioLink.Client.Utilities;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;

namespace BioLink.Client.Extensibility {

    public class TextBox : System.Windows.Controls.TextBox {

        private Timer _timer;
        private int _delay = 250;

        public TextBox() : base() {

            AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            AddHandler(GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText), true);
            AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(SelectAllText), true);

            this.TextChanged += new System.Windows.Controls.TextChangedEventHandler(TextBox_TextChanged);

            _timer = new Timer(new TimerCallback((obj) => {
                Trigger();
            }), null, Timeout.Infinite, Timeout.Infinite);
            
        }

        private static void SelectivelyIgnoreMouseButton(object sender,
                                                             MouseButtonEventArgs e) {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null) {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin) {
                    // If the text box is not yet focussed, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }

        private static void SelectAllText(object sender, RoutedEventArgs e) {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null) {
                textBox.SelectAll();
            }
        }

        private void Trigger() {
            try {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                this.InvokeIfRequired(() => {

                    BindingExpression expr = GetBindingExpression(TextBox.TextProperty);
                    if (expr != null) {
                        if (expr.Status == BindingStatus.Active) {
                            expr.UpdateSource();
                        }
                    }

                });
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }
        }


        void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            try {
                if (!String.IsNullOrEmpty(Text)) {
                    _timer.Change(_delay, _delay);
                } else {
                    Trigger();
                }
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }
        }


    }
}
