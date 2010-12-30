using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BioLink.Client.Extensibility {

    public class ItemsGroupBox : ContentControl {

        static ItemsGroupBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemsGroupBox), new FrameworkPropertyMetadata(typeof(ItemsGroupBox)));
            SelectPrevious = new RoutedCommand("SelectPreviousCommand", typeof(ItemsGroupBox));            
        }

        public ItemsGroupBox() {
            var selectPrevBinding = new CommandBinding(SelectPrevious, ExecutedCustomCommand, CanExecuteCustomCommand);
            this.CommandBindings.Add(selectPrevBinding);
        }

        public static RoutedCommand SelectPrevious { get; private set; }


        #region Dependency Properties

        public static readonly DependencyProperty EditButtonVisibilityProperty = DependencyProperty.Register("EditButtonVisibility", typeof(Visibility), typeof(ItemsGroupBox), new FrameworkPropertyMetadata(Visibility.Visible,  new PropertyChangedCallback(OnEditVisibilityChanged)));

        public Visibility EditButtonVisibility {
            get { return (Visibility)GetValue(EditButtonVisibilityProperty); }
            set { SetValue(EditButtonVisibilityProperty, value); }
        }

        private static void OnEditVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
        }

        public static readonly DependencyProperty AddNewButtonVisibilityProperty = DependencyProperty.Register("AddNewButtonVisibility", typeof(Visibility), typeof(ItemsGroupBox), new FrameworkPropertyMetadata(Visibility.Visible, new PropertyChangedCallback(OnAddNewVisibilityChanged)));

        public Visibility AddNewButtonVisibility {
            get { return (Visibility)GetValue(AddNewButtonVisibilityProperty); }
            set { SetValue(AddNewButtonVisibilityProperty, value); }
        }

        private static void OnAddNewVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
        }

        #endregion

        private void CanExecuteCustomCommand(object sender, CanExecuteRoutedEventArgs e) {

            Control target = e.Source as Control;

            if (target != null) {
                e.CanExecute = true;
            } else {
                e.CanExecute = false;
            }
        }

        private void ExecutedCustomCommand(object sender, ExecutedRoutedEventArgs e) {
            MessageBox.Show("Custom Command Executed");
        }
        
    }
}
