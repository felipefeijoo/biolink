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
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for NoteControl.xaml
    /// </summary>
    public partial class NoteControl : UserControl {

        #region Designer Constructor
        public NoteControl() {
            InitializeComponent();
        }
        #endregion

        public NoteControl(User user, NoteViewModel model) {
            InitializeComponent();
            this.User = user;
            Model = model;
            this.DataContext = model;
            txtNote.SelectionChanged += new RoutedEventHandler(txtNote_SelectionChanged);
        }

        void txtNote_SelectionChanged(object sender, RoutedEventArgs e) {
            if (TextSelectionChanged != null) {
                TextSelectionChanged(this, Model);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            RaiseNoteDeleted();
        }

        private void btnProperties_Click(object sender, RoutedEventArgs e) {
            ShowNoteProperties();
        }

        private void ShowNoteProperties() {
            var form = new NoteProperties(User, Model) { IsReadOnly = IsReadOnly };
            form.Owner = this.FindParentWindow();
            form.ShowDialog();
        }

        private void RaiseNoteChanged() {
            if (NoteChanged != null) {
                NoteChanged(this, Model);
            }
        }

        private void RaiseNoteDeleted() {
            if (NoteDeleted != null) {
                NoteDeleted(this, Model);
            }
        }

        #region Properties

        public NoteViewModel Model { get; private set; }

        public bool IsExpanded {
            get { return expander.IsExpanded; }
            set { expander.IsExpanded = value; }
        }

        public User User { get; private set; }

        #endregion

        #region Events

        public event NoteEventHandler NoteDeleted;

        public event NoteEventHandler NoteChanged;

        public event NoteEventHandler TextSelectionChanged;

        public delegate void NoteEventHandler(object source, NoteViewModel note);

        #endregion

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(NoteControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (NoteControl)obj;
            if (control != null) {
                var readOnly = (bool)args.NewValue;
                control.btnDelete.IsEnabled = !readOnly;
                control.txtNote.IsReadOnly = readOnly;                
            }
        }


    }
}
