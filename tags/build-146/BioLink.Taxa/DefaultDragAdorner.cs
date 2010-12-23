using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BioLink.Client.Taxa {

    public class DragAdorner : Adorner {

        protected UIElement _child;
        protected UIElement _owner;
        protected double XCenter;
        protected double YCenter;

        public DragAdorner(UIElement owner) : base(owner) { }

        public DragAdorner(UIElement owner, FrameworkElement adornElement, bool useVisualBrush, double opacity, Point offset)
            : base(owner) {
            System.Diagnostics.Debug.Assert(owner != null);
            System.Diagnostics.Debug.Assert(adornElement != null);
            _owner = owner;
            if (useVisualBrush) {

                VisualBrush _brush = new VisualBrush(adornElement);
                _brush.Opacity = opacity;
                _brush.Stretch = Stretch.None;
                _brush.AlignmentY = AlignmentY.Top;
                _brush.AlignmentX = AlignmentX.Left;
                Rectangle r = new Rectangle();
                r.Width = adornElement.ActualWidth;
                r.Height = adornElement.ActualHeight;
                XCenter = offset.X;
                YCenter = offset.Y;
                r.Fill = _brush;
                _child = r;
            } else {
                _child = adornElement;
            }
        }


        private double _leftOffset;
        public double LeftOffset {
            get { return _leftOffset; }
            set {
                _leftOffset = value - XCenter;
                UpdatePosition();
            }
        }

        private double _topOffset;
        public double TopOffset {
            get { return _topOffset; }
            set {
                _topOffset = value - YCenter;

                UpdatePosition();
            }
        }

        private void UpdatePosition() {
            AdornerLayer adorner = (AdornerLayer)this.Parent;
            if (adorner != null) {
                adorner.Update(this.AdornedElement);
            }
        }

        protected override Visual GetVisualChild(int index) {
            return _child;
        }

        protected override int VisualChildrenCount {
            get {
                return 1;
            }
        }


        protected override Size MeasureOverride(Size finalSize) {
            _child.Measure(finalSize);
            return _child.DesiredSize;
        }
        protected override Size ArrangeOverride(Size finalSize) {

            _child.Arrange(new Rect(_child.DesiredSize));
            return finalSize;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform) {
            GeneralTransformGroup result = new GeneralTransformGroup();

            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
            return result;
        }
    }

}
