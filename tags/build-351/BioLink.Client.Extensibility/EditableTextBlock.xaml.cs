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
using System.Windows.Threading;

namespace BioLink.Client.Extensibility {

    public partial class EditableTextBlock : UserControl {

        #region Constructor

        public EditableTextBlock() {
            InitializeComponent();
            base.Focusable = true;
            base.FocusVisualStyle = null;
        }

        #endregion Constructor

        #region Member Variables

        // We keep the old text when we go into editmode
        // in case the user aborts with the escape key
        private string oldText;

        #endregion Member Variables

        #region Properties

        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBlock), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnTextChanged)));

        public bool IsEditable {
            get { return (bool)GetValue(IsEditableProperty); }
            set { 
                SetValue(IsEditableProperty, value);
            }
        }
        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(true));

        private bool _cancelled = false;

        public bool IsInEditMode {
            get {
                if (IsEditable) {
                    return (bool)GetValue(IsInEditModeProperty);
                } else {
                    return false;
                }
            }
            set {
                if (IsEditable) {
                    SetValue(IsInEditModeProperty, value);
                }
            }
        }

        public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register("IsInEditMode", typeof(bool), typeof(EditableTextBlock), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsInEditModeChanged));

        public string TextFormat {
            get { return (string) GetValue(TextFormatProperty); }
            set {
                if (value == "") {
                    value = "{0}";
                }
                SetValue(TextFormatProperty, value);
            }
        }

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<string>), typeof(EditableTextBlock));

        private static void OnIsInEditModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var tb = (EditableTextBlock)obj;
            if ((bool)args.NewValue) {
                tb._cancelled = false;
                tb.oldText = tb.Text;
            }
        }

        private static String GetControlText(EditableTextBlock control) {
            TextBox txtBox = FindVisualChild<TextBox>(control);
            if (txtBox != null) {
                return txtBox.Text;
            }

            TextBlock txt = FindVisualChild<TextBlock>(control);
            if (txt != null) {
                return txt.Text;
            }

            return null;
        }
        
        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            if (args.OldValue != null) {
                EditableTextBlock control = (EditableTextBlock)obj;
                if (control.IsInEditMode) {
                    RoutedPropertyChangedEventArgs<string> e = new RoutedPropertyChangedEventArgs<string>((string)args.OldValue, (string)args.NewValue, TextChangedEvent);
                    control.OnTextChanged(e);
                    control._pendingChanges = true;
                } else {
                    (control as EditableTextBlock).Text = args.NewValue as string;

                    TextBox txtBox = FindVisualChild<TextBox>(control);
                    if (txtBox != null) {
                        txtBox.Text = (string)args.NewValue;
                    }

                    TextBlock txt = FindVisualChild<TextBlock>(control);
                    if (txt != null) {
                        txt.Text = (string)args.NewValue;
                    }
                }
            }
        }

        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is childItem)
                    return (childItem)child;
                else {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }

            return null;
        }

        protected virtual void OnTextChanged(RoutedPropertyChangedEventArgs<string> args) {
            RaiseEvent(args);
        }

        public static readonly DependencyProperty TextFormatProperty = DependencyProperty.Register( "TextFormat", typeof(string), typeof(EditableTextBlock), new PropertyMetadata("{0}"));

        public string FormattedText {
            get { return String.Format(TextFormat, Text); }
        }

        public object ViewModel {
            get { return GetValue(ViewModelProperty); }
            set { 
                SetValue(ViewModelProperty, value);                
            }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(object), typeof(EditableTextBlock), new PropertyMetadata(null));

        public TextDecorationCollection TextDecorations {
            get { return GetValue(TextDecorationsProperty) as TextDecorationCollection; }
            set { 
                SetValue(TextDecorationsProperty, value);                 
            }             
        }

        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register("TextDecorations", typeof(TextDecorationCollection), typeof(EditableTextBlock), new PropertyMetadata(new TextBlock().TextDecorations));

        #endregion Properties


        #region Event Handlers

        // Invoked when we enter edit mode.
        void TextBox_Loaded(object sender, RoutedEventArgs e) {
            var txt = sender as System.Windows.Controls.Primitives.TextBoxBase;
            if (txt != null) {
                // Give the TextBox input focus
                txt.Focus();
                txt.SelectAll();
            }
        }

        private bool _pendingChanges = false;

        // Invoked when we exit edit mode.
        void TextBox_LostFocus(object sender, RoutedEventArgs e) {            
            if (_cancelled) {
                if (EditingCancelled != null) {
                    EditingCancelled(this, oldText);
                }
            } else {
                RaiseEditingComplete();
            }
            this.IsInEditMode = false;
        }

        private void RaiseEditingComplete() {
            if (_pendingChanges) {
                if (EditingComplete != null) {
                    EditingComplete(this, Text);
                }
            }
            _pendingChanges = false;
        }

        // Invoked when the user edits the annotation.
        void TextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                RaiseEditingComplete();
                this.IsInEditMode = false;                                
                e.Handled = true;
            } else if (e.Key == Key.Escape) {
                this.IsInEditMode = false;
                Text = oldText;
                e.Handled = true;
                _cancelled = true;
                _pendingChanges = false;
            }
        }      

        public event EditingCancelledHandler EditingCancelled;

        public event EditingCompleteHandler EditingComplete;

        #endregion Event Handlers

        #region Events

        public delegate void EditingCompleteHandler(object sender, string text);

        public delegate void EditingCancelledHandler(object sender, string oldtext);

        #endregion

    }

}
