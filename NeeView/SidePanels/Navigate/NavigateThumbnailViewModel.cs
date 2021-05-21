using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    public class NavigateThumbnailViewModel : BindableBase
    {
        private MainViewComponent _mainViewComponent;
        private bool _isEnabled;
        private bool _isVisible;
        private double _rate;
        private double _thumbnailWidth;
        private double _thumbnailHeight = 256.0;
        private Brush _mainViewVisualBrush;
        private StreamGeometry _viewboxGeometry;
        private Size _canvasSize;


        public NavigateThumbnailViewModel(MainViewComponent mainViewComponent)
        {
            _mainViewComponent = mainViewComponent ?? throw new ArgumentNullException(nameof(mainViewComponent));

            InitializeThumbnail();
            InitializeViewbox();
        }


        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (SetProperty(ref _isEnabled, value))
                {
                    UpdateThumbnail();
                    UpdateVisibility();
                }
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            private set { SetProperty(ref _isVisible, value); }
        }

        public double ThumbnailWidth
        {
            get { return _thumbnailWidth; }
            set { SetProperty(ref _thumbnailWidth, value); }
        }

        public double ThumbnailHeight
        {
            get { return _thumbnailHeight; }
            set { SetProperty(ref _thumbnailHeight, value); }
        }

        public Brush MainViewVisualBrush
        {
            get { return _mainViewVisualBrush; }
            set { SetProperty(ref _mainViewVisualBrush, value); }
        }

        public StreamGeometry ViewboxGeometry
        {
            get { return _viewboxGeometry; }
            set { SetProperty(ref _viewboxGeometry, value); }
        }


        private void InitializeThumbnail()
        {
            this.MainViewVisualBrush = new VisualBrush()
            {
                Stretch = Stretch.Uniform,
                Visual = _mainViewComponent.MainView.PageContents,
            };

            _mainViewComponent.MainView.PageContents.SizeChanged +=
                (s, e) => UpdateThumbnail();
        }

        public void SetCanvasSize(Size newSize)
        {
            _canvasSize = newSize;
            UpdateThumbnail();
        }

        private void UpdateThumbnail()
        {
            if (!_isEnabled) return;

            var sourceWidth = _mainViewComponent.MainView.PageContents.ActualWidth;
            var sourceHeight = _mainViewComponent.MainView.PageContents.ActualHeight;

            if (sourceWidth <= 0.0 || sourceHeight <= 0.0)
            {
                this.ThumbnailWidth = 0.0;
                this.ThumbnailHeight = 0.0;
                _rate = 0.0;
            }
            else
            {
                var sourceSize = new Size(sourceWidth, sourceHeight);
                var limitSize = new Size(Math.Max(_canvasSize.Width - 1.0, 0.0), Math.Max(_canvasSize.Height - 1.0, 0.0));
                var size = sourceSize.Limit(limitSize);
                this.ThumbnailWidth = size.Width;
                this.ThumbnailHeight = size.Height;
                _rate = size.Width / sourceWidth;
            }

            UpdateViewbox();
        }


        private void InitializeViewbox()
        {
            var points = new List<Point>()
            {
                new Point(-0.5, -0.5),
                new Point(0.5, -0.5),
                new Point(0.5, 0.5),
                new Point(-0.5, 0.5)
            };

            _viewboxGeometry = new StreamGeometry();
            _viewboxGeometry.FillRule = FillRule.Nonzero;
            using (StreamGeometryContext context = _viewboxGeometry.Open())
            {
                context.BeginFigure(points[0], false, true);
                context.PolyLineTo(new List<Point> { points[1], points[2], points[3] }, true, false);
            }

            _mainViewComponent.MainView.TransformChanged +=
                (s, e) => UpdateViewbox();

            _mainViewComponent.MainView.View.SizeChanged +=
                (s, e) => UpdateViewbox();

            _mainViewComponent.ContentCanvas.ContentChanged +=
                (s, e) => UpdateViewbox();
        }

        private void UpdateViewbox()
        {
            if (!_isEnabled) return;

            var transformGroup = new TransformGroup();

            // 表示エリアの大きさに変換
            transformGroup.Children.Add(new ScaleTransform(_mainViewComponent.MainView.View.ActualWidth, _mainViewComponent.MainView.View.ActualHeight));

            // コンテンツ座標系の逆変換
            var mainViewTransform = _mainViewComponent.MainView.Transform;
            var inverse = mainViewTransform.Inverse;
            if (inverse is Transform inverseTransform)
            {
                transformGroup.Children.Add(inverseTransform);
            }
            else
            {
                // NOTE: 拡大することで範囲外にする
                transformGroup.Children.Add(new ScaleTransform(2.0, 2.0));
            }

            // キャンバス座標系に変換
            transformGroup.Children.Add(new ScaleTransform(_rate, _rate));
            transformGroup.Children.Add(new TranslateTransform(_canvasSize.Width * 0.5, _canvasSize.Height * 0.5));

            _viewboxGeometry.Transform = transformGroup;

            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            this.IsVisible = _isEnabled && _rate >= 0.0 && _mainViewComponent.ContentCanvas.IsViewContents();
        }

        public void LookAt(Point point)
        {
            if (_rate <= 0.0) return;

            var x = (this.ThumbnailWidth * 0.5 - point.X) / _rate;
            var y = (this.ThumbnailHeight * 0.5 - point.Y) / _rate;
            _mainViewComponent.DragTransformControl.LookAt(new Point(x, y));
        }
    }
}
