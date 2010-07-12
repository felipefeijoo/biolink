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
using BioLink.Client.Extensibility;

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DebugControl : UserControl, IDebugLogObserver {
        public DebugControl() {
            InitializeComponent();
        }

        void IDebugLogObserver.Log(DebugLogMessage message) {
            this.lvwLog.Items.Add(String.Format("[{0}] {1}", message.Date, message.Message ));
        }
    }
}
