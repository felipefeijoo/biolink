using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace BioLink.Client.Extensibility {

    public static class Preferences {

        public static PreferenceEditor<int> MaxSearchResults = new PreferenceEditor<int>("SearchResults.MaxSearchResults", "Maximum number of search results:", PreferenceScope.User, 2000);

        public static PreferenceEditor<bool> UniqueAccessionNumbers = new PreferenceEditor<bool>("Material.CheckUniqueAccessionNumbers", "Check for duplicate accession numbers when saving material?", PreferenceScope.Global, true);

        public static PreferenceEditor<bool> UseSimplifiedAssociates = new PreferenceEditor<bool>("Associates.UsePestHostControl", "Use the simplified Pest/Host associates editor, if possible?", PreferenceScope.User, false);

        public static PreferenceEditor<bool> UseFloatingEGaz = new PreferenceEditor<bool>("Gazetteer.ShowEgazFloating", "Always show EGaz in a floating window?", PreferenceScope.User, false);

        public static PreferenceEditor<bool> AutoGenerateMaterialNames = new PreferenceEditor<bool>("Material.AlwaysRegenerateMaterialName", "Always re-generate Material names when details are changed?", PreferenceScope.Profile, false);

        public static PreferenceEditor<bool> AutoGenerateSiteVisitNames = new PreferenceEditor<bool>("Material.AlwaysRegenerateSiteVisitName", "Always re-generate Site Visit names when details are changed?", PreferenceScope.Profile, false);

        public static PreferenceEditor<bool> UseLoanPermitNumberTrait = new PreferenceEditor<bool>("Loans.UsePermitNumberTrait", "Seed a loans permit number from the borrowers permit number trait?", PreferenceScope.Profile, true);
        
    }

    public abstract class AbstractPreferenceEditor {

        public AbstractPreferenceEditor(string preferenceKey, string caption, PreferenceScope scope, object @default) {
            this.PreferenceKey = preferenceKey;
            this.Caption = caption;
            this.Scope = scope;
            this.DefaultValue = @default;
        }

        public PT GetValue<PT>() {
            PT @default = (PT) DefaultValue;
            var user = PluginManager.Instance.User;
            switch (Scope) {
                case PreferenceScope.Global:
                    return Config.GetGlobal(PreferenceKey, @default);
                case PreferenceScope.Profile:
                    return Config.GetProfile(user, PreferenceKey, @default);
                case PreferenceScope.User:
                    return Config.GetUser(user, PreferenceKey, @default);
                default:
                    throw new Exception("Unhandled preference scope!");
            }
        }

        protected void SetValue<PT>(PT value) {
            var user = PluginManager.Instance.User;
            switch (Scope) {
                case PreferenceScope.Global:
                    Config.SetGlobal(PreferenceKey, value);
                    break;
                case PreferenceScope.Profile:
                    Config.SetProfile(user, PreferenceKey, value);
                    break;
                case PreferenceScope.User:
                    Config.SetUser(user, PreferenceKey, value);
                    break;
                default:
                    throw new Exception("Unhandled preference scope!");
            }
        }

        public abstract FrameworkElement BuildControl();

        public abstract void Commit();

        public string PreferenceKey { get; private set; }
        public string Caption { get; private set; }        
        public PreferenceScope Scope { get; private set; }
        public Object DefaultValue { get; private set; }

    }

    public class PreferenceEditor<T> : AbstractPreferenceEditor {

        private UIElement _editorControl;

        public PreferenceEditor(string preferenceKey, string caption, PreferenceScope scope, T @default) : base(preferenceKey, caption, scope, @default) {
        }

        public override FrameworkElement BuildControl() {
            var grid = new Grid();
            var type = typeof(T);
            
            if (typeof(Boolean).IsAssignableFrom(type)) {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                var chk = new CheckBox { Content = Caption, IsChecked = GetValue<bool>() };
                _editorControl = chk;
                grid.Children.Add(chk);
            } else if (typeof(int).IsAssignableFrom(type)) {
                _editorControl = AddTextBox(grid, 120);
            } else {
                throw new Exception("Unhandled preference type: " + type.Name);
            }

            return grid;
        }

        private TextBox AddTextBox(Grid grid, int width) {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var txt = new TextBox { Text = GetValue<int>() + "", VerticalAlignment = System.Windows.VerticalAlignment.Center, Width = width, HorizontalAlignment = HorizontalAlignment.Left };
            Grid.SetColumn(txt, 1);
            grid.Children.Add(new Label { Content = Caption, VerticalAlignment = System.Windows.VerticalAlignment.Center });
            grid.Children.Add(txt);
            return txt;
        }

        public override void Commit() {
            if (_editorControl is CheckBox) {
                var value = (_editorControl as CheckBox).IsChecked == true;
                SetValue(value);
            } else if (_editorControl is TextBox) {
                if (typeof(int).IsAssignableFrom(typeof(T))) {
                    var value = Int32.Parse((_editorControl as TextBox).Text);
                    SetValue(value);
                } else {
                    var value = (_editorControl as TextBox).Text;
                    SetValue(value);
                }
            }
        }

        public T Value {
            get { return GetValue<T>(); }
        }
        
    }

    public enum PreferenceScope {
        Profile,
        User,        
        Global
    }

}
