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
using System.Threading;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for DelayedTriggerTextbox.xaml
    /// </summary>
    public partial class DelayedTriggerTextbox : UserControl {

        private Timer _timer;

        public DelayedTriggerTextbox() {
            this.Delay = 300;
            InitializeComponent();
            _timer = new Timer(new TimerCallback((obj) => {
                Trigger();
            }), null, Timeout.Infinite, Timeout.Infinite);
        }

        public event TypingPausedEventHandler TypingPaused;

        public event TextChangedEventHandler TextChanged;

        private void Trigger() {
            try {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                if (TypingPaused != null) {
                    string text = null;
                    textBox.InvokeIfRequired(() => {
                        text = textBox.Text;
                        TypingPaused(text);
                    });                    
                }
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {                
                if (!String.IsNullOrEmpty(textBox.Text)) {
                    _timer.Change(Delay, Delay);
                } else {
                    Trigger();
                }

                if (TextChanged != null) {
                    this.InvokeIfRequired(() => {
                        TextChanged(sender, e);
                    });
                }
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }

        }

        public String Text {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }

        public int Delay { get; set; }
    }

    public delegate void TypingPausedEventHandler(string text);
}
