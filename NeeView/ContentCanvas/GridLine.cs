using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    public class GridLine : BindableBase
    {
        public GridLine()
        {
            Config.Current.ImageGrid.PropertyChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(Content));
            };
        }

        private double _width;
        public double Width
        {
            get { return _width; }
            set { if (SetProperty(ref _width, value)) RaisePropertyChanged(nameof(Content)); }
        }

        private double _height;
        public double Height
        {
            get { return _height; }
            set { if (SetProperty(ref _height, value)) RaisePropertyChanged(nameof(Content)); }
        }

        public UIElement Content
        {
            get { return CreatePath(); }
        }

        public void SetSize(double width, double height)
        {
            if (_width != width || _height != height)
            {
                _width = width;
                _height = height;
                RaisePropertyChanged(nameof(Width));
                RaisePropertyChanged(nameof(Height));
                RaisePropertyChanged(nameof(Content));
            }
        }

        private UIElement CreatePath()
        {
            var imageGrid = Config.Current.ImageGrid;

            if (!imageGrid.IsEnabled || Width <= 0.0 || Height <= 0.0) return null;

            double cellX = imageGrid.DivX > 0 ? Width / imageGrid.DivX : Width;
            double cellY = imageGrid.DivY > 0 ? Height / imageGrid.DivY : Height;

            if (imageGrid.IsSquare)
            {
                if (cellX < cellY)
                {
                    cellX = cellY;
                }
                else
                {
                    cellY = cellX;
                }
            }

            var canvas = new Canvas();
            canvas.Width = Width;
            canvas.Height = Height;

            var stroke = new SolidColorBrush(imageGrid.Color);

            canvas.Children.Add(CreatePath(new Point(0, 0), new Point(0, Height), stroke));
            canvas.Children.Add(CreatePath(new Point(Width, 0), new Point(Width, Height), stroke));
            canvas.Children.Add(CreatePath(new Point(0, 0), new Point(Width, 0), stroke));
            canvas.Children.Add(CreatePath(new Point(0, Height), new Point(Width, Height), stroke));

            for (double i = cellX; i < Width - 1; i += cellX)
            {
                canvas.Children.Add(CreatePath(new Point(i, 0), new Point(i, Height), stroke));
            }

            for (double i = cellY; i < Height - 1; i += cellY)
            {
                canvas.Children.Add(CreatePath(new Point(0, i), new Point(Width, i), stroke));
            }

            return canvas;
        }

        private Path CreatePath(Point startPoint, Point endPoint, Brush stroke)
        {
            var geometry = new LineGeometry(startPoint, endPoint);
            geometry.Freeze();

            return new Path()
            {
                Data = geometry,
                Stroke = stroke,
                StrokeThickness = 1
            };
        }

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public bool IsEnabled { get; set; }

            [DataMember, DefaultValue(8)]
            public int DivX { get; set; }

            [DataMember, DefaultValue(8)]
            public int DivY { get; set; }

            [DataMember]
            public bool IsSquare { get; set; }

            [DataMember, DefaultValue(typeof(Color), "#80808080")]
            public Color Color { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                config.ImageGrid.DivX = DivX;
                config.ImageGrid.DivY = DivY;
                config.ImageGrid.IsSquare = IsSquare;
                config.ImageGrid.Color = Color;
                config.ImageGrid.IsEnabled = IsEnabled;
            }
        }

        #endregion

    }
}
