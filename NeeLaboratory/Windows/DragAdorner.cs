// from https://github.com/takanemu/WPFDragAndDropSample

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;


namespace NeeLaboratory.Windows
{
    /// <summary>
    /// ドラッグアドナー
    /// </summary>
    internal class DragAdorner : Adorner
    {
        protected UIElement _child;

        protected double _centerX;
        protected double _centerY;

        private double _leftOffset;
        private double _topOffset;
        
        /// <summary>
        /// Left offset
        /// </summary>
        public double LeftOffset
        {
            get { return _leftOffset; }
            set
            {
                _leftOffset = value - _centerX;
                UpdatePosition();
            }
        }

        /// <summary>
        /// Top offset
        /// </summary>
        public double TopOffset
        {
            get { return _topOffset; }
            set
            {
                _topOffset = value - _centerY;
                UpdatePosition();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner"></param>
        public DragAdorner(UIElement owner) : base(owner) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="adornElement"></param>
        /// <param name="opacity"></param>
        /// <param name="dragPos"></param>
        public DragAdorner(UIElement owner, UIElement adornElement, double opacity, Point dragPos)
            : base(owner)
        {
            var brush = new VisualBrush(adornElement) { Opacity = opacity };
            var bounds = VisualTreeHelper.GetDescendantBounds(adornElement);
            var rectamge = new Rectangle() { Width = bounds.Width, Height = bounds.Height };

            _centerX = dragPos.X;
            _centerY = dragPos.Y;

            rectamge.Fill = brush;
            _child = rectamge;
        }

        /// <summary>
        /// Returns a Transform for the adorner, based on the transform that is currently applied to the adorned element.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
            return result;
        }

        /// <summary>
        /// Overrides Visual.GetVisualChild, and returns a child at the specified index from a collection of child elements.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        /// <summary>
        /// Implements any custom measuring behavior for the adorner.
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size finalSize)
        {
            _child.Measure(finalSize);
            return _child.DesiredSize;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a FrameworkElement derived class.
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(_child.DesiredSize));
            return finalSize;
        }

        /// <summary>
        /// 座標更新
        /// </summary>
        private void UpdatePosition()
        {
            var adorner = this.Parent as AdornerLayer;
            if (adorner != null)
            {
                adorner.Update(this.AdornedElement);
            }
        }
    }


}
