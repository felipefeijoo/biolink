/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
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

            textBox.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(textBox_GotKeyboardFocus);
            textBox.MouseDoubleClick += new MouseButtonEventHandler(textBox_MouseDoubleClick);

        }

        void textBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            textBox.SelectAll();
        }

        void textBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            textBox.SelectAll();
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
                    if (!this.TimerDisabled) {
                        _timer.Change(Delay, Delay);
                    }
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

        public bool TimerDisabled { get; set; }

    }

    public delegate void TypingPausedEventHandler(string text);
}
