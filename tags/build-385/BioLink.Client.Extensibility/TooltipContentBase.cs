using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public abstract class TooltipContentBase : UserControl {

        #region Designer Ctor
        public TooltipContentBase() {
        }
        #endregion

        public TooltipContentBase(int objectID, ViewModelBase viewModel) {
            ObjectID = objectID;
            ViewModel = viewModel;
            Loaded += new RoutedEventHandler(TooltipContentBase_Loaded);
        }

        void TooltipContentBase_Loaded(object sender, RoutedEventArgs e) {
            var outer = new Grid();
            outer.RowDefinitions.Add(new RowDefinition { Height= new GridLength(28) });
            outer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(6) });
            outer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            outer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(6) });
            outer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });

            var header = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
            var icon = new Image { SnapsToDevicePixels = true, UseLayoutRounding = true, Stretch = System.Windows.Media.Stretch.None, Margin = new Thickness(6, 0, 6, 0), Source = Icon, VerticalAlignment = System.Windows.VerticalAlignment.Center };
            header.Children.Add(icon);

            var lblTitle = new TextBlock { FontWeight = FontWeights.Bold, Text = Title, VerticalAlignment = System.Windows.VerticalAlignment.Center };
            header.Children.Add(lblTitle);

            outer.Children.Add(header);

            var sep1 = new Separator { Margin = new Thickness(6, 0, 6, 0) };
            Grid.SetRow(sep1, 1);
            outer.Children.Add(sep1);

            var model = GetModel();
            var detail = GetDetailContent(model);
            if (detail != null) {
                Grid.SetRow(detail, 2);
                outer.Children.Add(detail);

                var sep2 = new Separator { Margin = new Thickness(6, 0, 6, 0) };
                Grid.SetRow(sep2, 3);
                outer.Children.Add(sep2);
            }

            var lblSystem = new Label { FontSize = 10, FontWeight = FontWeights.Light };
            Grid.SetRow(lblSystem, 4);

            
            var sb = new StringBuilder();
            if (model != null) {
                sb.AppendFormat("{0} {1}", model.GetType().Name, ObjectID);
                if (model is OwnedDataObject) {
                    var odo = model as OwnedDataObject;
                    sb.AppendFormat(", Modified: {0:g} by {1}", odo.DateLastUpdated, odo.WhoLastUpdated);                    
                }
                if (model is GUIDObject) {
                    sb.AppendFormat("\n{0}", (model as GUIDObject).GUID.ToString());
                }

            } else {
                sb.AppendFormat("{0} {1}", ViewModel.GetType().Name, ObjectID);
            }
            

            lblSystem.Content = sb.ToString();
            outer.Children.Add(lblSystem);


            this.Content = outer;
        }

        protected virtual string Title {
            get { return ViewModel.DisplayLabel; }
        }

        protected virtual ImageSource Icon {
            get { return ViewModel.Icon; }
        }

        protected virtual FrameworkElement GetDetailContent(BioLinkDataObject model) {
            var builder = new TextTableBuilder();
            GetDetailText(model, builder);
            return builder.GetAsContent();
        }

        protected abstract void GetDetailText(BioLinkDataObject model, TextTableBuilder builder);

        protected abstract BioLinkDataObject GetModel();

        protected int ObjectID { get; private set; }

        protected ViewModelBase ViewModel { get; private set; }

        protected User User {
            get { return PluginManager.Instance.User; }
        }

    }

    public class TextTableBuilder {

        private List<Pair<string, string>> _list = new List<Pair<string, string>>();

        public TextTableBuilder() { }

        public void Add(string heading, DateTime? value) {
            if (value.HasValue) {
                AddFormat(heading, "{0:d}", value.Value);
            }
        }

        public void Add(string heading, int? value) {
            if (value.HasValue) {
                AddFormat(heading, "{0}", value.Value);
            }
        }

        public void Add(string heading, double? value) {
            if (value.HasValue) {
                AddFormat(heading, "{0}", value.Value);
            }
        }

        public void Add(string heading, string value) {
            if (!string.IsNullOrWhiteSpace(heading) && !string.IsNullOrWhiteSpace(value)) {
                _list.Add(new Pair<string, string>(heading, value));
            }
        }

        public void AddFormat(string heading, string valuefmt, params object[] args) {
            if (!string.IsNullOrWhiteSpace(heading) && !string.IsNullOrWhiteSpace(valuefmt)) {
                _list.Add(new Pair<string, string>(heading, string.Format(valuefmt, args)));
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();
            foreach (Pair<string, string> p in _list) {
                sb.AppendFormat("{0}: {1}\n", p.First, p.Second);
            }
            return sb.ToString();
        }

        public FrameworkElement GetAsContent() {
            var grid = new Grid();

            for (int i = 0; i < _list.Count; ++i) {
                grid.RowDefinitions.Add(new RowDefinition { /* Height = new GridLength(23) */ });
            }

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });            
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            int row = 0;
            foreach (Pair<string, string> p in _list) {
                var heading = new Label { Content = p.First, Padding = new Thickness(0) };
                var value = new Label { Content = p.Second, FontWeight = FontWeights.SemiBold, Padding = new Thickness(0), Margin = new Thickness(6,0,6,0) };
                Grid.SetColumn(value, 1);
                Grid.SetRow(heading, row);
                Grid.SetRow(value, row);

                grid.Children.Add(heading);
                grid.Children.Add(value);
                row++;
            }

            return grid;
        }

    }

}
