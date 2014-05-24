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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for EditTransformationPipeline.xaml
    /// </summary>
    public partial class EditTransformationPipelineWindow : Window {

        public EditTransformationPipelineWindow() {
            InitializeComponent();
        }

        public EditTransformationPipelineWindow(TransformationPipline transform) {
            InitializeComponent();
            this.TransformationPipeline = transform;
            RedrawPipeline();
        }

        private void RedrawPipeline() {
            transformersPanel.Children.Clear();
            transformersPanel.Children.Add(new TransformationPiplineStartControl());
            if (TransformationPipeline != null) {
                foreach (IValueTransformer t in TransformationPipeline.Transformers) {
                    var tt = new ValueTransformerControl(t);
                    transformersPanel.Children.Add(tt);
                }
            }
            transformersPanel.Children.Add(new TransformationPipelineOutputControl());
        }


        public TransformationPipline TransformationPipeline { get; private set; }

        public User User {
            get { return PluginManager.Instance.User; }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {

        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {

        }

        private void btnAddTransform_Click(object sender, RoutedEventArgs e) {
            this.TransformationPipeline.AddTransformer(new UpperCaseTransformer());
            RedrawPipeline();
        }

    }
}
