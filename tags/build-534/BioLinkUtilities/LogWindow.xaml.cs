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
using Microsoft.Practices.EnterpriseLibrary.Logging; 
using System.Collections.ObjectModel;

namespace BioLink.Client.Utilities {
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window {

        private Action<LogEntry> _handler;
        private ObservableCollection<LogEntry> _entries;

        public LogWindow() {
            InitializeComponent();
            Loaded += new RoutedEventHandler(LogWindow_Loaded);
            Unloaded += new RoutedEventHandler(LogWindow_Unloaded);
        }

        void LogWindow_Unloaded(object sender, RoutedEventArgs e) {
            Logger.MessageLogged -= _handler;
        }

        void LogWindow_Loaded(object sender, RoutedEventArgs e) {

            _entries = new ObservableCollection<LogEntry>();
            grid.ItemsSource = _entries;

            _handler = new Action<LogEntry>(Logger_MessageLogged);
            Logger.MessageLogged += _handler;
        }

        void Logger_MessageLogged(Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry entry) {
            this.InvokeIfRequired(() => {
                _entries.Add(entry);
            });
        }

    }
}
