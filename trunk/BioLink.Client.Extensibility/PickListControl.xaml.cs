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
using BioLink.Data;
using BioLink.Data.Model;


namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for PhraseSelector.xaml
    /// </summary>
    public partial class PickListControl : UserControl {

        #region DesignerConstructor
        public PickListControl() {
            InitializeComponent();
        }
        #endregion

        public PickListControl(User user, PickListType type, string phraseCategory, TraitCategoryType traitCategory) {
            InitializeComponent();
            BindUser(user, type, phraseCategory, traitCategory);
        }

        public void BindUser(User user, PickListType pickListType, String phraseCategory, TraitCategoryType traitCategory) {
            this.User = user;
            this.PhraseCategory = phraseCategory;
            this.TraitCategory = traitCategory;
            this.PickListType = pickListType;
        }

        private void btn_Click(object sender, RoutedEventArgs e) {
            ShowPickList();
        }

        private void ShowPickList() {
            PickListWindow frm = new PickListWindow(this.User, PickListType, PhraseCategory, TraitCategory);
            if (frm.ShowDialog().GetValueOrDefault(false)) {
                txt.Text = frm.SelectedValue;
            };
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(PickListControl), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnTextChanged)));

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (PickListControl) obj;
            control.txt.Text = args.NewValue as String;
            control.FireValueChanged(control.txt.Text);
        }

        protected void FireValueChanged(string text) {
            if (this.ValueChanged != null) {
                ValueChanged(this, text);
            }
        }

        public String Text {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        protected User User { get; private set; }

        protected string PhraseCategory { get; private set; }

        protected TraitCategoryType TraitCategory { get; private set; }

        protected PickListType PickListType { get; private set; }

        private void txt_TextChanged(object sender, TextChangedEventArgs e) {
            Text = txt.Text;
        }

        private void txt_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) > 0) {
                ShowPickList();
                e.Handled = true;
            }
        }

        public event ValueChangedHandler ValueChanged;

        public delegate void ValueChangedHandler(object source, string value);

    }

    public enum PickListType {
        Phrase,
        Trait,
        Keyword,
        DistinctList
    }
}
