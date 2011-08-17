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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using BioLink.Client.Utilities;
using System.Text;
using System.IO;
using System.Threading;

namespace BioLink.Client.Extensibility {

    public class BindableRichTextBox : RichTextBox {

        private bool _selfSetting;
        private bool _ignoreTextChanged = false;
        private Timer _timer;
        private int _delay = 250;


        public BindableRichTextBox() {
            this.TextChanged += new TextChangedEventHandler(txt_TextChanged);

            _timer = new Timer(new TimerCallback((obj) => {
                Trigger();
            }), null, Timeout.Infinite, Timeout.Infinite);

            this.FontSize = 12;
        }

        private void Trigger() {
            try {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                this.InvokeIfRequired(() => {
                    
                    var range = new TextRange(Document.ContentStart, Document.ContentEnd);
                    string rtf;

                    using (var stream = new MemoryStream()) {
                        range.Save(stream, DataFormats.Rtf);
                        stream.Seek(0, SeekOrigin.Begin);
                        using (var reader = new StreamReader(stream)) {
                            rtf = reader.ReadToEnd();
                        }
                    }
                    _selfSetting = true;
                    RTF = rtf;
                    _selfSetting = false;
                });
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }
        }


        void txt_TextChanged(object sender, TextChangedEventArgs e) {
            if (!_ignoreTextChanged) {
                try {
                    _timer.Change(_delay, _delay);
                } catch (Exception ex) {
                    GlobalExceptionHandler.Handle(ex);
                }
            }
        }

        public static readonly DependencyProperty RTFProperty = DependencyProperty.Register("RTF", typeof(string), typeof(BindableRichTextBox), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnRTFChanged)));

        private static void OnRTFChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (BindableRichTextBox)obj;
            if (!control._selfSetting) {
                control._ignoreTextChanged = true;
                var rtf = args.NewValue as string;
                var doc = control.Document;
                if (string.IsNullOrEmpty(rtf)) {
                    doc.Blocks.Clear();
                } else {
                    using (new OverrideCursor(System.Windows.Input.Cursors.Wait)) {
                        using (var stream = new MemoryStream((new UTF8Encoding()).GetBytes(rtf))) {
                            var text = new TextRange(doc.ContentStart, doc.ContentEnd);
                            text.Load(stream, DataFormats.Rtf);
                        }
                    }
                }
                control._ignoreTextChanged = false;
            }
        }

        public String RTF {
            get { return (string)GetValue(RTFProperty); }
            set { SetValue(RTFProperty, value); }
        }


        public String PlainText {
            get { return new TextRange(Document.ContentStart, Document.ContentEnd).Text; }
        }


    }
}