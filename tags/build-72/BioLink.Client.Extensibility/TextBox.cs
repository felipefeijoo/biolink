using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace BioLink.Client.Extensibility {

    public class TextBox : System.Windows.Controls.TextBox {

        public TextBox() : base() {
            this.GotFocus +=new System.Windows.RoutedEventHandler((source, e) => {
                SelectAll();
            });

            this.TextChanged += new System.Windows.Controls.TextChangedEventHandler(TextBox_TextChanged);
            
        }

        void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            BindingExpression expr = GetBindingExpression(TextBox.TextProperty);
            if (expr != null) {
                if (expr.Status == BindingStatus.Active) {
                    expr.UpdateSource();
                }
            }
        }


    }
}
