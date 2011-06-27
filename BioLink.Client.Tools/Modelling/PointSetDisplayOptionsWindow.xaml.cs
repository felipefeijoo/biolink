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
using BioLink.Client.Utilities;
using BioLink.Data;



namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for PointSetDisplayOptionsWindow.xaml
    /// </summary>
    public partial class PointSetDisplayOptionsWindow : Window {

        private PointSetViewModel _pointSet;

        public PointSetDisplayOptionsWindow(PointSetViewModel pointSet) {
            InitializeComponent();
            _pointSet = pointSet;
            if (_pointSet != null) {
                ctlOptions.Shape = pointSet.PointShape;
                ctlOptions.Size = pointSet.Size;
                ctlOptions.Color = pointSet.PointColor;
                ctlOptions.DrawOutline = pointSet.DrawOutline;                
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (_pointSet != null) {
                _pointSet.PointShape = ctlOptions.Shape;
                _pointSet.Size = ctlOptions.Size;
                _pointSet.PointColor = ctlOptions.Color;
                _pointSet.DrawOutline = ctlOptions.DrawOutline;
                this.DialogResult = true;
            }
            this.Close();
        }

    }
}
