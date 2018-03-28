using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
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
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (SetProperty(ref _isEnabled, value)) RaisePropertyChanged(nameof(Content)); }
        }

        private int _divX = 8;
        [PropertyRange("@ParamGridLineDivX", 1, 50, TickFrequency = 1)]
        public int DivX
        {
            get { return _divX; }
            set { if (SetProperty(ref _divX, value)) RaisePropertyChanged(nameof(Content)); }
        }

        private int _divY = 8;
        [PropertyRange("@ParamGridLineDivY", 1, 50, TickFrequency = 1)]
        public int DivY
        {
            get { return _divY; }
            set { if (SetProperty(ref _divY, value)) RaisePropertyChanged(nameof(Content)); }
        }

        private bool _isSquare;
        [PropertyMember("@ParamGridLineIsSquare")]
        public bool IsSquare
        {
            get { return _isSquare; }
            set { if (SetProperty(ref _isSquare, value)) RaisePropertyChanged(nameof(Content)); }
        }

        private Color _color = Color.FromArgb(0x80, 0x80, 0x80, 0x80);
        [PropertyMember("@ParamGridLineColor")]
        public Color Color
        {
            get { return _color; }
            set { if (SetProperty(ref _color, value)) RaisePropertyChanged(nameof(Content)); }
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
            if (!IsEnabled || Width <= 0.0 || Height <= 0.0) return null;

            double cellX = DivX > 0 ? Width / DivX : Width;
            double cellY = DivY > 0 ? Height / DivY : Height;

            if (IsSquare)
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

            var stroke = new SolidColorBrush(Color);

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
        public class Memento
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
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsEnabled = this.IsEnabled;
            memento.DivX = this.DivX;
            memento.DivY = this.DivY;
            memento.IsSquare = this.IsSquare;
            memento.Color = this.Color;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            // 更新回数を抑えるために設定前に無効にする
            this.IsEnabled = false;

            this.DivX = memento.DivX;
            this.DivY = memento.DivY;
            this.IsSquare = memento.IsSquare;
            this.Color = memento.Color;
            this.IsEnabled = memento.IsEnabled;
        }

        #endregion

    }
}
