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
            // We need to take a copy so that we can cancel out without making any changes...
            Pipeline = TransformationPipline.BuildFromState(transform.GetState());
            
            RedrawPipeline();
        }

        private void RedrawPipeline() {
            String testValue = "";

            if (InputPanel != null) {
                testValue = InputPanel.txtTestValue.Text;
            }

            transformersPanel.Children.Clear();
            InputPanel = new TransformationPiplineStartControl();
            InputPanel.txtTestValue.Text = testValue;
            InputPanel.TestClicked += new RoutedEventHandler(start_TestClicked);
            transformersPanel.Children.Add(InputPanel);
            ControlPipeline = new List<ValueTransformerControl>();
            if (Pipeline != null) {
                foreach (ValueTransformer t in Pipeline.Transformers) {
                    var tt = new ValueTransformerControl(t);
                    transformersPanel.Children.Add(tt);
                    tt.RemoveClicked += new RoutedEventHandler(tt_RemoveClicked);
                    tt.MoveUpClicked += new RoutedEventHandler(tt_MoveUpClicked);
                    tt.MoveDownClicked += new RoutedEventHandler(tt_MoveDownClicked);
                    tt.OptionsClicked += new RoutedEventHandler(tt_OptionsClicked);
                    ControlPipeline.Add(tt);
                }
            }
            this.OutputPanel = new TransformationPipelineOutputControl();
            transformersPanel.Children.Add(OutputPanel);

            if (!String.IsNullOrEmpty(testValue)) {
                TestPipeline(testValue);
            }
        }

        void tt_OptionsClicked(object sender, RoutedEventArgs e) {
            var ctl = sender as ValueTransformerControl;
            if (ctl != null) {
                if (ctl.ValueTransformer.HasOptions) {
                    ctl.ValueTransformer.ShowOptions(this);
                    RedrawPipeline();
                }                
            }

        }

        protected TransformationPiplineStartControl InputPanel { get; private set; }
        protected TransformationPipelineOutputControl OutputPanel { get; private set; }
        protected List<ValueTransformerControl> ControlPipeline { get; private set; }

        void start_TestClicked(object sender, RoutedEventArgs e) {
            var start = sender as TransformationPiplineStartControl;
            if (start != null) {
                TestPipeline(start.TestValue);
            }            
        }

        private void TestPipeline(String testValue) {            
            foreach (ValueTransformerControl ctl in ControlPipeline) {
                testValue = ctl.Test(testValue);
            }
            OutputPanel.lblOutput.Text = testValue;
        }

        void tt_MoveDownClicked(object sender, RoutedEventArgs e) {
            var ctl = sender as ValueTransformerControl;
            if (ctl != null) {
                Pipeline.MoveTransformerDown(ctl.ValueTransformer);
                RedrawPipeline();
            }
        }

        void tt_MoveUpClicked(object sender, RoutedEventArgs e) {
            var ctl = sender as ValueTransformerControl;
            if (ctl != null) {
                Pipeline.MoveTransformerUp(ctl.ValueTransformer);
                RedrawPipeline();
            }
        }

        void tt_RemoveClicked(object sender, RoutedEventArgs e) {
            var ctl = sender as ValueTransformerControl;
            if (ctl != null) {
                Pipeline.RemoveTransformer(ctl.ValueTransformer);
                RedrawPipeline();
            }
        }


        public TransformationPipline Pipeline { get; private set; }

        public User User {
            get { return PluginManager.Instance.User; }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        private void btnAddTransform_Click(object sender, RoutedEventArgs e) {
            var frm = new SelectTransformWindow();
            frm.Owner = this;
            frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            if (frm.ShowDialog().GetValueOrDefault()) {
                if (frm.SelectedValueTransformerKey != null) {
                    var transform = TransformFactory.CreateTransform(frm.SelectedValueTransformerKey, null);
                    if (transform != null) {
                        this.Pipeline.AddTransformer(transform);
                    }
                }
            }
            
            RedrawPipeline();
        }

    }
}
