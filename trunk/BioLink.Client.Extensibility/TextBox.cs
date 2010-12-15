using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Threading;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class TextBox : System.Windows.Controls.TextBox {

        private Timer _timer;
        private int _delay = 250;

        public TextBox() : base() {

            this.GotFocus +=new System.Windows.RoutedEventHandler((source, e) => {
                SelectAll();
            });

            this.TextChanged += new System.Windows.Controls.TextChangedEventHandler(TextBox_TextChanged);

            _timer = new Timer(new TimerCallback((obj) => {
                Trigger();
            }), null, Timeout.Infinite, Timeout.Infinite);
            
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
