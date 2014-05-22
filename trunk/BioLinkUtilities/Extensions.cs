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
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// A collection of useful extension methods that enhance a variety of built in or library types
    /// </summary>
    public static class Extensions {

        /// <summary>
        /// Exposes a ForEach method on IEnumerables allowing an action to be performed against every element of a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumeration"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action) {
            foreach (T item in enumeration) {
                action(item);
            }
        }

        /// <summary>
        /// Allows an action to performed against each element in a collection. The action is supplied the object, and its index in the containing collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumeration"></param>
        /// <param name="action"></param>
        public static void ForEachIndex<T>(this IEnumerable<T> enumeration, Action<T, int> action) {
            int index = 0;
            foreach (T item in enumeration) {
                action(item, index++);
            }
        }

        /// <summary>
        /// Gets the controls dispatcher and invokes the action with a priority of background
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        public static void BackgroundInvoke(this DispatcherObject control, Action action) {
            control.Dispatcher.Invoke(DispatcherPriority.Background, action);
        }

        /// <summary>
        /// Checks to see if the current (calling) thread is the same as the controls dispatcher thread. If not, will
        /// schedule the action to be performed on the dispatcher thread, otherwise invokes directly
        /// </summary>
        /// <param name="control">A control on which some action needs to be performed</param>
        /// <param name="action">The action</param>
        public static void InvokeIfRequired(this DispatcherObject control, Action action) {
            if (control.Dispatcher.Thread != Thread.CurrentThread) {
                control.Dispatcher.Invoke(DispatcherPriority.Normal, action);
            } else {
                action();
            }
        }

        /// <summary>
        /// Returns true if the environment is Visual Studios designer
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public static bool IsDesignTime(this Control control) {
            return DesignerProperties.GetIsInDesignMode(control);
        }

        /// <summary>
        /// Looks up a string resource from the controls resource dictionarys with the given key.
        /// Can also perform string.format style substitutions
        /// </summary>
        /// <param name="control">The control whose resource dictionary is going to be checked</param>
        /// <param name="messageKey">the message key</param>
        /// <param name="args">Arguments</param>
        /// <returns></returns>
// ReSharper disable InconsistentNaming
        public static string _R(this Control control, string messageKey, params object[] args) {
// ReSharper restore InconsistentNaming
            string message = null;
            control.InvokeIfRequired(() => {

                try {
                    message = control.FindResource(messageKey) as string;
                } catch (Exception) {
                    message = null;
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

        /// <summary>
        /// Wrapper over MessageBox that asks a Yes/No question
        /// </summary>
        /// <param name="control">The source of the question</param>
        /// <param name="question">Question Text</param>
        /// <param name="caption">Window caption</param>
        /// <param name="icon">Alternate icon (optional)</param>
        /// <returns>Returns true if the Yes button is pressed</returns>
        public static bool Question(this Control control, string question, string caption, MessageBoxImage icon = MessageBoxImage.Question) {

            MessageBoxResult result = MessageBoxResult.No;
            control.InvokeIfRequired(() => {
                Window parent = Window.GetWindow(control);
                if (parent != null) {
                    result = MessageBox.Show(parent, question, caption, MessageBoxButton.YesNo, icon);
                }
            });
            return result == MessageBoxResult.Yes;
        }

        public static bool DiscardChangesQuestion(this Control control, string question="You have unsaved changes. Are you sure you wish to discard those changes?") {
            bool discard = false;
            control.InvokeIfRequired(() => {
                Window parent = Window.GetWindow(control);
                var frm = new DiscardChangesWindow {Owner = parent, label1 = {Text = question}, WindowStartupLocation = WindowStartupLocation.CenterOwner};
                if (frm.ShowDialog() == true) {
                    discard = true;
                }
            });
            return discard;
        }

        /// <summary>
        /// Useful for nullable booleans (bool?). Slighlty simplifies the GetValueOrDefault call
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ValueOrFalse(this bool? value) {
            return value.GetValueOrDefault(false);
        }

        /// <summary>
        /// Checks if a string holds a numeric value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNumeric(this string value) {
            double result;
            return Double.TryParse(value, out result);
        }

        /// <summary>
        /// Checks if a string holds an integer value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInteger(this string value) {
            int result;
            return Int32.TryParse(value, out result);
        }

        /// <summary>
        /// Truncates a string to a specified length, and will append ellipsis (or other suffix) if truncation is actually necessary
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string Truncate(this string value, int length, string suffix = "...") {
            if (value.Length > length) {
                return value.Substring(0, length - suffix.Length) + suffix;
            }
            return value;
        }

        public static int? StripNonInteger(this string value) {
            var b = new StringBuilder();
            foreach (char ch in value) {
                if (Char.IsNumber(ch)) {
                    b.Append(ch);
                }
            }

            if (b.Length == 0) {
                return null;
            }
            return Int32.Parse(b.ToString());
        }

        /// <summary>
        /// Returns turn if the type to check is a subclass of generic type
        /// </summary>
        /// <param name="toCheck"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic) {
            while (toCheck != typeof(object)) {
                var cur = toCheck != null && toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) {
                    return true;
                }
                if (toCheck != null) {
                    toCheck = toCheck.BaseType;
                }
            }
            return false;
        }

        /// <summary>
        /// Helper function to load text into a RichTextControl
        /// </summary>
        /// <param name="control"></param>
        /// <param name="fragment"></param>
        public static void SetRTF(this RichTextBox control, string fragment) {
            if (string.IsNullOrEmpty(fragment)) {
                control.Document.Blocks.Clear();
            } else {
                control.SelectAll();
                control.Selection.Load(new MemoryStream(Encoding.Default.GetBytes(fragment)), DataFormats.Rtf);
            }
        }

        public static TabItem AddDeferredLoadingTabItem(this TabControl tab, string title, Func<UIElement> controlFactory) {
            var tabItem = new TabItem {Header = title, Content = null};
            tab.Items.Add(tabItem);

            tabItem.RequestBringIntoView += (s, e) => {                
                if (tabItem.Content == null) {
                    var control = controlFactory();
                    tabItem.Content = control;
                    if (control is ILazyPopulateControl) {
                        var lazyLoadee = control as ILazyPopulateControl;
                        if (!lazyLoadee.IsPopulated) {
                            lazyLoadee.Populate();
                        }
                    }
                }
            };


            return tabItem;

        }

        /// <summary>
        /// Adds a new tab item to specified tab control. Will bind a handler to RequestBringIntoView should
        /// the content be ILazyPopulateControl
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static TabItem AddTabItem(this TabControl tab, string title, UIElement content) {
            var tabItem = new TabItem {Header = title, Content = content};
            tab.Items.Add(tabItem);

            if (content is ILazyPopulateControl) {
                var lazyLoadee = content as ILazyPopulateControl;
                if (tab.Items.Count == 1) {
                    lazyLoadee.Populate();
                } else {
                    tabItem.RequestBringIntoView += (s, e) => {
                        if (!lazyLoadee.IsPopulated) {
                            lazyLoadee.Populate();
                        }
                    };
                }
            }

            return tabItem;
        }

        /// <summary>
        /// Helper to retrieve a TreeViewItem from its framework element 
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static TreeViewItem GetTreeViewItemClicked(this TreeView treeView, FrameworkElement sender) {
            Point p = sender.TranslatePoint(new Point(1, 1), treeView);
            var obj = treeView.InputHitTest(p) as DependencyObject;
            while (obj != null && !(obj is TreeViewItem)) {
                obj = VisualTreeHelper.GetParent(obj);
            }
            return obj as TreeViewItem;
        }

        public static TItemType FindAncestorOfType<TItemType>(this FrameworkElement element, FrameworkElement sender) where TItemType : class {
           Point p = sender.TranslatePoint(new Point(1, 1), element);
            var obj = element.InputHitTest(p) as DependencyObject;
            while (obj != null && !(obj is TItemType)) {
                obj = VisualTreeHelper.GetParent(obj);
            }
            return obj as TItemType;

        }

        /// <summary>
        /// Builds a string by repeatingthe source n times
        /// </summary>
        /// <param name="stringToRepeat"></param>
        /// <param name="repeat"></param>
        /// <returns></returns>
        public static string Repeat(this string stringToRepeat, int repeat) {
            var builder = new StringBuilder(repeat);
            for (int i = 0; i < repeat; i++) {
                builder.Append(stringToRepeat);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Construct a string that is a delimited list of the elements of an array (using toString())
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string Join<T>(this T[] items, string delimiter) {            
            if (items.Length == 0) {
                return "";
            }
            var sb = new StringBuilder(items[0].ToString());
            for (int i = 1; i < items.Length; ++i) {
                sb.Append(delimiter);
                sb.Append(items[i] == null ? "" : items[i].ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Construct a string that is a delimited list of the elements of a collection (using toString())
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string Join<T>(this IList<T> items, string delimiter) {
            if (items.Count == 0) {
                return "";
            }
            var sb = new StringBuilder(items[0].ToString());
            for (int i = 1; i < items.Count; ++i) {
                sb.Append(delimiter);
                sb.Append(items[i] == null ? "" : items[i].ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Reads the next line from the stream, and attempts to match the supplied pattern, and yield the first group as an int 
        /// Throws if no match
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static int ReadRegexInt(this StreamReader reader, string pattern) {
            return ReadRegex(reader, pattern, Int32.Parse);
        }

        /// <summary>
        /// Reads the next line from the stream, and attempts to match the supplied pattern, and yield the first group as a double
        /// Throws if no match
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static double ReadRegexDouble(this StreamReader reader, string pattern) {
            return ReadRegex(reader, pattern, Double.Parse);
        }

        /// <summary>
        /// Reads the next line from the stream, and attempts to match the supplied pattern, and yield the first group as an instance of type T 
        /// using the supplied convert function
        /// Throws if no match
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="pattern"></param>
        /// <param name="convert"></param>
        /// <returns></returns>
        public static T ReadRegex<T>(this StreamReader reader, string pattern, Func<string, T> convert) {
            var str = ReadRegex(reader, pattern);
            if (str != null) {
                return convert(str);
            }
            throw new Exception(string.Format("Failed to read data in expected format: {0}", pattern));
        }

        /// <summary>
        /// Reads the next double value from the reader. will consume characters until a non-numeric is encountered
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static double ReadDouble(this StreamReader reader) {
            const string valid = "0123456789.-";
            int ch;
            var b = new StringBuilder();
            while ((ch = reader.Read()) >= 0 && valid.IndexOf((char) ch) >= 0) {
                b.Append((char)ch);
            }
            if (ch == 10) {
                reader.Read();
                return ReadDouble(reader);
            }
            double ret;
            if (double.TryParse(b.ToString(), out ret)) {
                return ret;
            }
            throw new Exception("Failed to read double from stream!");
        }

        /// <summary>
        /// Reads the next line from the stream, and matches it against the supplied pattern. Returns the first group as a string
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string ReadRegex(this StreamReader reader, string pattern) {

            var line = reader.ReadLine();
            if (line == null) {
                return null;
            }

            var regex = new Regex(pattern);
            var m = regex.Match(line);
            if (m.Success) {
                return m.Groups[1].Value;
            }

            // failure
            return null;
        }

        /// <summary>
        /// Traverses a controls ancestry to find a Window container
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public static Window FindParentWindow(this FrameworkElement control) {
            if (control is Window) {
                return control as Window;
            }

            var p = control.Parent as FrameworkElement;
            while (!(p is Window) && p != null) {
                p = p.Parent as FrameworkElement;
            }

            if (p != null) {
                return p as Window;
            }
            return null;
        }

    }

    /// <summary>
    /// A delegate used for formatting message strings in the same style as String.Format()
    /// </summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate string MessageFormatterFunc(string format, params object[] args);

    /// <summary>
    /// Help class that adds extensions to list boxes
    /// </summary>
    public class ListBoxExtenders : DependencyObject {

        public static readonly DependencyProperty AutoScrollToCurrentItemProperty = DependencyProperty.RegisterAttached("AutoScrollToCurrentItem", typeof(bool), typeof(ListBoxExtenders), new UIPropertyMetadata(default(bool), OnAutoScrollToCurrentItemChanged));

        public static bool GetAutoScrollToCurrentItem(DependencyObject obj) {
            return (bool)obj.GetValue(AutoScrollToCurrentItemProperty);
        }

        public static void SetAutoScrollToCurrentItem(DependencyObject obj, bool value) {
            obj.SetValue(AutoScrollToCurrentItemProperty, value);
        }

        public static void OnAutoScrollToCurrentItemChanged(DependencyObject s, DependencyPropertyChangedEventArgs e) {
            var listBox = s as ListBox;
            if (listBox != null) {
                var listBoxItems = listBox.Items;
                if (listBoxItems != null) {
                    var newValue = (bool)e.NewValue;

                    var autoScrollToCurrentItemWorker = new EventHandler((s1, e2) => OnAutoScrollToCurrentItem(listBox, listBox.Items.CurrentPosition));

                    if (newValue)
                        listBoxItems.CurrentChanged += autoScrollToCurrentItemWorker;
                    else
                        listBoxItems.CurrentChanged -= autoScrollToCurrentItemWorker;
                }
            }
        }

        public static void OnAutoScrollToCurrentItem(ListBox listBox, int index) {
            if (listBox != null && listBox.Items != null && listBox.Items.Count > index && index >= 0)
                listBox.ScrollIntoView(listBox.Items[index]);
        }

    }


}
