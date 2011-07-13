using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using BioLink.Client.Utilities;
using System.Windows.Input;

namespace BioLink.Client.Extensibility {

    public static class FrameworkExtensions {

        public static T FindParent<T>(this FrameworkElement control) where T : FrameworkElement {
            var p = control.Parent as FrameworkElement;
            while (!(p is T) && p != null) {
                p = p.Parent as FrameworkElement;
            }

            if (p != null) {
                return (T)p;
            }
            return null;
        }

        public static void WaitCursor(this Control control) {
            control.SetCursor(Cursors.Wait);
        }

        public static void NormalCursor(this Control control) {
            control.SetCursor(Cursors.Arrow);
        }

        public static void SetCursor(this Control control, Cursor cursor) {
            control.InvokeIfRequired(() => {
                control.Cursor = cursor;
            });
        }

    }

            


}
