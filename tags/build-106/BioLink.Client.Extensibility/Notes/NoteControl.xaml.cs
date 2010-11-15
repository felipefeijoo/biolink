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

        public NoteControl(NoteViewModel model) {
            InitializeComponent();
            Model = model;
            this.DataContext = model;
        }

        public NoteViewModel Model { get; private set; }
    }
}
