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

        public void BindUser(User user, PickListType pickListType, String categoryName, TraitCategoryType traitCategory) {
            this.User = user;
            this.CategoryName = categoryName;
            this.TraitCategory = traitCategory;
            this.PickListType = pickListType;

            if (pickListType == Extensibility.PickListType.MultimediaType) {
                txt.IsReadOnly = true;
            }

            this.Service = new SupportService(user);
        }

        private void btn_Click(object sender, RoutedEventArgs e) {
            ShowPickList();
        }

        public static string ShowPickList(User user, PickListType type, string categoryName, TraitCategoryType traitCategory) {
            Func<IEnumerable<string>> itemsFunc = null;
            Func<string, bool> addItemFunc = null;
            string caption = "Select a value";
            var service = new SupportService(user);
            switch (type) {
                case PickListType.Phrase:
                    int phraseCategoryID = service.GetPhraseCategoryId(categoryName, true);
                    caption = String.Format("Values for '{0}'", categoryName);
                    itemsFunc = () => service.GetPhrases(phraseCategoryID).ConvertAll((phrase) => phrase.PhraseText);

                    addItemFunc = (newphrase) => {
                        Phrase phrase = new Phrase();
                        phrase.PhraseID = -1;
                        phrase.PhraseCatID = phraseCategoryID;
                        phrase.PhraseText = newphrase;
                        // Save the new phrase value...
                        service.AddPhrase(phrase);
                        return true;
                    };
                    break;
                case PickListType.DistinctList:
                    caption = String.Format("Values for '{0}'", categoryName);
                    itemsFunc = () => service.GetTraitDistinctValues(categoryName, traitCategory.ToString());
                    break;
                case PickListType.Trait:
                    caption = String.Format("Existing trait names for {0}", traitCategory.ToString());
                    itemsFunc = () => service.GetTraitNamesForCategory(traitCategory.ToString());
                    break;
                case PickListType.MultimediaType:
                    caption = "Select a multimedia type...";
                    itemsFunc = () => {
                        return service.GetMultimediaTypes().ConvertAll((mmtype) => mmtype.Name);
                    };
                    break;
                default:
                    throw new Exception("Unhandled pick list type: " + type);
            }

            PickListWindow frm = new PickListWindow(user, caption, itemsFunc, addItemFunc);

            if (frm.ShowDialog().GetValueOrDefault(false)) {
                return frm.SelectedValue;
            };

            return null;
        }

        private void ShowPickList() {
            String value = ShowPickList(User, PickListType, CategoryName, TraitCategory);
            if (value != null) {
                txt.Text = value;
            }
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

        protected string CategoryName { get; private set; }

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

        protected SupportService Service { get; private set; }

    }

    public enum PickListType {
        Phrase,
        Trait,
        Keyword,
        DistinctList,
        MultimediaType
    }
}
