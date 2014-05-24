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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;

namespace BioLink.Client.Extensibility.Transform {
    /// <summary>
    /// Interaction logic for EditTransformationPipeline.xaml
    /// </summary>
    public partial class EditTransformationPipeline : Window {

        public EditTransformationPipeline() {
            InitializeComponent();
        }

        public EditTransformationPipeline(TransformationPipline transform) {
            InitializeComponent();
            this.TransformationPipeline = transform;
            RenderTransforms();
        }

        private void RenderTransforms() {
            transformersPanel.Children.Clear();
            if (TransformationPipeline != null) {
                foreach (IValueTransformer t in TransformationPipeline.Transformers) {
                    var tt = new ValueTransformerControl(t);
                    transformersPanel.Children.Add(tt);
                }
            }
        }


        public TransformationPipline TransformationPipeline { get; private set; }

        public User User {
            get { return PluginManager.Instance.User; }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {

        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {

        }

    }
}
