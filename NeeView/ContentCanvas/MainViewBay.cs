using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public class MainViewBay : Grid, IHasDeviceInput
    {
        private MouseInput _mouseInput;
        private TouchInput _touchInput;


        public MouseInput MouseInput => _mouseInput;
        public TouchInput TouchInput => _touchInput;


        public MainViewBay()
        {
            this.Name = "MainViewBase";
            this.Background = Brushes.Gray;
            this.IsHitTestVisible = true;

#if false
            var visualBrush = new VisualBrush()
            {
                Visual = _mainView,
                Stretch = Stretch.Uniform,
            };

            var rectangle = new Rectangle()
            {
                Width = 256,
                Height = 256,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 50, 0, 0),
                Fill = visualBrush,
            };

            this.Children.Add(rectangle);
#endif

            // mouse / touch command gesture binding
            var mouseGestureCommandCollection = MouseGestureCommandCollection.Current;
            _mouseInput = new MouseInput(new MouseInputContext(this, mouseGestureCommandCollection, null, null, null));
            _touchInput = new TouchInput(new TouchInputContext(this, null, mouseGestureCommandCollection, null, null));
            RoutedCommandTable.Current.AddMouseInput(_mouseInput);
            RoutedCommandTable.Current.AddTouchInput(_touchInput);
        }
    }
}
