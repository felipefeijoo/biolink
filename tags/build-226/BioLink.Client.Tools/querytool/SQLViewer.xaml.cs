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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for SQLViewer.xaml
    /// </summary>
    public partial class SQLViewer : Window {
        public SQLViewer(string sql) {
            InitializeComponent();
            txtSQL.Text = sql;
        }
    }
}
