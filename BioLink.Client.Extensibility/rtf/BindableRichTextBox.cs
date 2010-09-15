using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using BioLink.Client.Utilities;
using System.Text;
using System.IO;

namespace BioLink.Client.Extensibility {

    public class BindableRichTextBox : RichTextBox {

        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register("Document", typeof (FlowDocument), typeof (BindableRichTextBox));

        public BindableRichTextBox() { }

        public BindableRichTextBox(FlowDocument document) : base(document) { }

        public new FlowDocument Document {
            get { return (FlowDocument) GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        protected override void OnInitialized(EventArgs e) {            
            var descriptor = DependencyPropertyDescriptor.FromProperty(DocumentProperty, typeof (BindableRichTextBox));
            descriptor.AddValueChanged(this, delegate {
                base.Document = (FlowDocument) GetValue(DocumentProperty); 
            });

            LostFocus += BindableRichTextBox_LostFocus;
            
            base.OnInitialized(e);
        }

        private void BindableRichTextBox_LostFocus(object sender, RoutedEventArgs e) {
            var binding = BindingOperations.GetBinding(this, DocumentProperty);
            if (binding != null) {
                if (binding.UpdateSourceTrigger == UpdateSourceTrigger.Default || binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus) {
                    BindingOperations.GetBindingExpression(this, DocumentProperty).UpdateSource();
                }
            }
        }

    }
}