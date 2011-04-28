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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for PointSetOptionsWindow.xaml
    /// </summary>
    public partial class PointSetOptionsWindow : Window {

        public PointSetOptionsWindow(string caption, Func<MapPointSet> generator) {
            InitializeComponent();
            this.Generator = generator;
            this.Caption = caption;
            this.Title = "Point options - " + caption;
        }

        protected Func<MapPointSet> Generator { get; private set; }

        protected string Caption { get; private set; }

        public MapPointSet Points { get; private set; }

        private void btnOK_Click(object sender, RoutedEventArgs e) {

            btnCancel.IsEnabled = false;
            btnOK.IsEnabled = false;
            lblStatus.Content = "Generating points...";
            JobExecutor.QueueJob(() => {
                if (Generator != null) {
                    Points = Generator();
                    this.InvokeIfRequired(() => {
                        Points.PointColor = shapeOptions.Color;
                        Points.PointShape = shapeOptions.Shape;
                        Points.Size = shapeOptions.Size;
                        Points.DrawOutline = shapeOptions.DrawOutline;                        
                    });
                }
                this.InvokeIfRequired(() => {
                    lblStatus.Content = "";
                    this.DialogResult = true;
                    this.Close();
                });
            });
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

    }

}
