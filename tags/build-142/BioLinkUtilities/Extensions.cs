using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;

namespace BioLink.Client.Utilities {

    public static class Extensions {

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action) {
            foreach (T item in enumeration) {
                action(item);
            }
        }

        public static void BackgroundInvoke(this DispatcherObject control, Action action) {
            control.Dispatcher.Invoke(DispatcherPriority.Background, action);
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

        public static void WaitCursor(this Control control) {
            control.SetCursor(Cursors.Wait);
        }

        public static void NormalCursor(this Control control) {
            control.SetCursor(Cursors.Arrow);
        }

        public static void SetCursor(this Control control, Cursor cursor) {
            control.InvokeIfRequired( () => {
                control.Cursor = cursor;
            });
        }

        public static Window FindParentWindow(this FrameworkElement control) {            
            var p = control.Parent as FrameworkElement;
            while (!(p is Window) && p != null) {
                p = p.Parent as FrameworkElement;
            }

            if (p != null) {
                return p as Window;
            }
            return null;
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

        public static bool ValueOrFalse(this bool? value) {
            return value.GetValueOrDefault(false);
        }

        public static bool IsNumeric(this string value) {
            int result;
            return Int32.TryParse(value, out result);            
        }

        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic) {
            while (toCheck != typeof(object)) {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public static void SetRTF(this RichTextBox control, string fragment) {
            if (string.IsNullOrEmpty(fragment)) {
                control.Document.Blocks.Clear();
            } else {
                control.SelectAll();                
                control.Selection.Load(new MemoryStream(UTF8Encoding.Default.GetBytes(fragment)), DataFormats.Rtf);                                
            }
        }

        public static TabItem AddTabItem(this TabControl tab, string title, UIElement content) {
            TabItem tabItem = new TabItem();
            tabItem.Header = title;
            tabItem.Content = content;
            tab.Items.Add(tabItem);

            if (content is ILazyPopulateControl) {
                tabItem.RequestBringIntoView += new RequestBringIntoViewEventHandler((s, e) => {
                    var lazyLoadee = content as ILazyPopulateControl;
                    if (!lazyLoadee.IsPopulated) {
                        lazyLoadee.Populate();
                    }
                });
            }

            return tabItem;
        }

        public static TreeViewItem GetTreeViewItemClicked(this TreeView treeView, FrameworkElement sender) {
            Point p = sender.TranslatePoint(new Point(1, 1), treeView);
            DependencyObject obj = treeView.InputHitTest(p) as DependencyObject;
            while (obj != null && !(obj is TreeViewItem)) {
                obj = VisualTreeHelper.GetParent(obj);
            }
            return obj as TreeViewItem;
        }



    }

    public delegate string MessageFormatterFunc(string format, params object[] args);

}
