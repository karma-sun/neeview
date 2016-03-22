// from http://mslaboratory.blog.eonet.jp/default/2012/10/behaviordragdro-cee4.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DragExtensions
{
    public class DragAdorner : Adorner
    {
        private Rectangle _Ghost;
        protected Vector _Move;

        public DragAdorner(UIElement element, UIElement view, double opacity, Point point) : base(element)
        {
            if (view == null) view = element;

            Rect bounds = VisualTreeHelper.GetDescendantBounds(view);
            _Ghost = new Rectangle() 
            {
                Height = bounds.Height,
                Width = bounds.Width, 
                Fill = new VisualBrush(view) { Opacity = opacity }
            };

            _Move = (Vector)Window.GetWindow(element).PointFromScreen(element.PointToScreen(point));
            _Move.Negate();

            AdornerLayer adorner = AdornerLayer.GetAdornerLayer((Visual)WPFUtil.FindVisualParent<Window>(this.AdornedElement).Content);
            if (adorner != null) adorner.Add(this);
        }

        private Point _Position;

        public Point Position
        {
            get { return _Position; }
            set
            {
                _Position = (Point)(value + _Move);
                UpdatePosition();
            }
        }

        public void Remove()
        {
            AdornerLayer adorner = this.Parent as AdornerLayer;
            if (adorner != null) adorner.Remove(this);
        }

        protected void UpdatePosition()
        {
            AdornerLayer adorner = this.Parent as AdornerLayer;
            if (adorner != null) adorner.Update(this.AdornedElement);
        }

        protected override Visual GetVisualChild(int index)
        {
            return _Ghost;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Size MeasureOverride(Size finalSize)
        {
            _Ghost.Measure(finalSize);
            return _Ghost.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {

            _Ghost.Arrange(new Rect(_Ghost.DesiredSize));
            return finalSize;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(Position.X, Position.Y));
            return result;
        }
    }
}
