using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;

namespace BioLink.Client.Utilities {

    public static class Extensions {

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action) {
            foreach (T item in enumeration) {
                action(item);
            }
        }

        public static void InvokeIfRequired(this DispatcherObject control, Action action) {
            if (control.Dispatcher.Thread != Thread.CurrentThread) {
                control.Dispatcher.Invoke(action);
            } else {
                action();
            }
        }

        public static bool IsDesignTime(this Control control) {
            return DesignerProperties.GetIsInDesignMode(control);
        }

        public static string _R(this Control control, string messageKey, params object[] args) {
            string message = null;
            control.InvokeIfRequired(() => {

                try {
                    message = control.FindResource(messageKey) as string;
                } catch (Exception) {
                }

                if (message == null) {
                    Logger.Warn("Failed for find message for key '{0}' - using key instead", messageKey);
                    message = messageKey;
                }

                if (args != null) {
                    message = String.Format(message, args);
                }
            });

            return message;
        }

        public static bool Question(this Control control, string question, string caption) {            
            Window parent = Window.GetWindow(control);
            MessageBoxResult result = MessageBox.Show(parent, question, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

    }

}
