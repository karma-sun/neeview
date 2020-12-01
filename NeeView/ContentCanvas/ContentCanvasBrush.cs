using NeeLaboratory.ComponentModel;
using NeeView.Windows.Media;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class ContentCanvasBrush : BindableBase
    {
        private ContentCanvas _contentCanvas;

        public ContentCanvasBrush(ContentCanvas contentCanvas)
        {
            _contentCanvas = contentCanvas;

            _contentCanvas.ContentChanged +=
                (s, e) => UpdateBackgroundBrush();

            _contentCanvas.DpiChanged +=
                (s, e) => UpdateBackgroundBrush();

            Config.Current.Background.AddPropertyChanged(nameof(BackgroundConfig.CustomBackground), (s, e) =>
            {
                InitializeCustomBackgroundBrush();
            });

            Config.Current.Background.AddPropertyChanged(nameof(BackgroundConfig.BackgroundType), (s, e) =>
            {
                UpdateBackgroundBrush();
            });

            Config.Current.Background.AddPropertyChanged(nameof(BackgroundConfig.PageBackgroundColor), (s, e) =>
            {
                UpdatePageBackgroundBrush();
            });

            Config.Current.Background.AddPropertyChanged(nameof(BackgroundConfig.IsPageBackgroundChecker), (s, e) =>
            {
                UpdatePageBackgroundBrush();
            });


            // Initialize
            InitializeCustomBackgroundBrush();
            UpdateBackgroundBrush();
            UpdatePageBackgroundBrush();
        }

        private void InitializeCustomBackgroundBrush()
        {
            UpdateCustomBackgroundBrush();
            if (Config.Current.Background.CustomBackground != null)
            {
                Config.Current.Background.CustomBackground.PropertyChanged += (s, e) => UpdateCustomBackgroundBrush();
            }
        }


        // Foregroudh Brush：ファイルページのフォントカラー用
        private SolidColorBrush _foregroundBrush = Brushes.White;
        public SolidColorBrush ForegroundBrush
        {
            get { return _foregroundBrush; }
            set { if (_foregroundBrush != value) { _foregroundBrush = value; RaisePropertyChanged(); } }
        }

        // ページ背景ブラシ
        private Brush _pageBackgroundBrush = null;
        public Brush PageBackgroundBrush
        {
            get { return _pageBackgroundBrush; }
            set { SetProperty(ref _pageBackgroundBrush, value); }
        }


        // Backgroud Brush
        private Brush _backgroundBrush = Brushes.Black;
        public Brush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { if (_backgroundBrush != value) { _backgroundBrush = value; RaisePropertyChanged(); UpdateForegroundBrush(); } }
        }

        /// <summary>
        /// BackgroundFrontBrush property.
        /// </summary>
        private Brush _BackgroundFrontBrush;
        public Brush BackgroundFrontBrush
        {
            get { return _BackgroundFrontBrush; }
            set { if (_BackgroundFrontBrush != value) { _BackgroundFrontBrush = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// カスタム背景
        /// </summary>
        private Brush _customBackgroundBrush;
        public Brush CustomBackgroundBrush
        {
            get { return _customBackgroundBrush ?? (_customBackgroundBrush = Config.Current.Background.CustomBackground?.CreateBackBrush()); }
        }


        /// <summary>
        /// カスタム背景
        /// </summary>
        private Brush _customBackgroundFrontBrush;
        public Brush CustomBackgroundFrontBrush
        {
            get { return _customBackgroundFrontBrush ?? (_customBackgroundFrontBrush = Config.Current.Background.CustomBackground?.CreateFrontBrush()); }
        }

        /// <summary>
        /// チェック模様
        /// </summary>
        public Brush CheckBackgroundBrush { get; } = (DrawingBrush)App.Current.Resources["CheckerBrush"];
        public Brush CheckBackgroundBrushDark { get; } = (DrawingBrush)App.Current.Resources["CheckerBrushDark"];



        public void UpdatePageBackgroundBrush()
        {
            PageBackgroundBrush = Config.Current.Background.PageBackgroundColor.A > 0
                ? Config.Current.Background.IsPageBackgroundChecker ? CreateCheckerBrush(Config.Current.Background.PageBackgroundColor) : new SolidColorBrush(Config.Current.Background.PageBackgroundColor)
                : null;
        }

        // from http://msdn.microsoft.com/en-us/library/aa970904.aspx
        public static Brush CreateCheckerBrush(Color color)
        {
            var brush1 = new SolidColorBrush(color);

            var hsv = color.ToHSV();
            hsv.V = hsv.V + (hsv.V < 0.1 ? 0.1 : -0.1);
            var brush2 = new SolidColorBrush(hsv.ToARGB());

            DrawingBrush checkerBrush = new DrawingBrush();

            GeometryDrawing backgroundSquare =
                new GeometryDrawing(
                    brush1,
                    null,
                    new RectangleGeometry(new Rect(0, 0, 8, 8)));

            GeometryGroup aGeometryGroup = new GeometryGroup();
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 4, 4)));
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(4, 4, 4, 4)));

            GeometryDrawing checkers = new GeometryDrawing(brush2, null, aGeometryGroup);

            DrawingGroup checkersDrawingGroup = new DrawingGroup();
            checkersDrawingGroup.Children.Add(backgroundSquare);
            checkersDrawingGroup.Children.Add(checkers);

            checkerBrush.Drawing = checkersDrawingGroup;
            checkerBrush.ViewportUnits = BrushMappingMode.Absolute;
            checkerBrush.Viewport = new Rect(0, 0, 16, 16);
            checkerBrush.TileMode = TileMode.Tile;

            return checkerBrush;
        }


        // Foregroud Brush 更新
        private void UpdateForegroundBrush()
        {
            var solidColorBrush = BackgroundBrush as SolidColorBrush;
            if (solidColorBrush != null)
            {
                double y =
                    (double)solidColorBrush.Color.R * 0.299 +
                    (double)solidColorBrush.Color.G * 0.587 +
                    (double)solidColorBrush.Color.B * 0.114;

                ForegroundBrush = (y < 128.0) ? Brushes.White : Brushes.Black;
            }
            else
            {
                ForegroundBrush = Brushes.Black;
            }
        }

        // Background Brush 更新
        public void UpdateBackgroundBrush()
        {
            BackgroundBrush = CreateBackgroundBrush();
            BackgroundFrontBrush = CreateBackgroundFrontBrush(_contentCanvas.Dpi);
        }
        
        private void UpdateCustomBackgroundBrush()
        {
            _customBackgroundBrush = null;
            _customBackgroundFrontBrush = null;
            if (Config.Current.Background.BackgroundType == BackgroundType.Custom)
            {
                UpdateBackgroundBrush();
            }
        }

        /// <summary>
        /// 背景ブラシ作成
        /// </summary>
        /// <returns></returns>
        public Brush CreateBackgroundBrush()
        {
            switch (Config.Current.Background.BackgroundType)
            {
                default:
                case BackgroundType.Black:
                    return Brushes.Black;
                case BackgroundType.White:
                    return Brushes.White;
                case BackgroundType.Auto:
                    return new SolidColorBrush(_contentCanvas.GetContentColor());
                case BackgroundType.Check:
                    return null;
                case BackgroundType.Custom:
                    return CustomBackgroundBrush;
            }
        }

        /// <summary>
        /// 背景ブラシ(画像)作成
        /// </summary>
        /// <param name="dpi">適用するDPI</param>
        /// <returns></returns>
        public Brush CreateBackgroundFrontBrush(DpiScale dpi)
        {
            switch (Config.Current.Background.BackgroundType)
            {
                default:
                case BackgroundType.Black:
                case BackgroundType.White:
                case BackgroundType.Auto:
                    return null;
                case BackgroundType.Check:
                    {
                        var brush = CheckBackgroundBrush.Clone();
                        brush.Transform = new ScaleTransform(1.0 / dpi.DpiScaleX, 1.0 / dpi.DpiScaleY);
                        return brush;
                    }
                case BackgroundType.CheckDark:
                    {
                        var brush = CheckBackgroundBrushDark.Clone();
                        brush.Transform = new ScaleTransform(1.0 / dpi.DpiScaleX, 1.0 / dpi.DpiScaleY);
                        return brush;
                    }
                case BackgroundType.Custom:
                    {
                        var brush = CustomBackgroundFrontBrush?.Clone();
                        // 画像タイルの場合はDPI考慮
                        if (brush is ImageBrush imageBrush && imageBrush.TileMode == TileMode.Tile)
                        {
                            brush.Transform = new ScaleTransform(1.0 / dpi.DpiScaleX, 1.0 / dpi.DpiScaleY);
                        }
                        return brush;
                    }
            }
        }

        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [Obsolete, DataMember(Name = "Background", EmitDefaultValue = false)]
            public BackgroundStyleV1 BackgroundV1 { get; set; }

            [DataMember(Name = "BackgroundV2")]
            public BackgroundType Background { get; set; }

            [DataMember]
            public BrushSource CustomBackground { get; set; }

            [DataMember, DefaultValue(typeof(Color), "Transparent")]
            public Color PageBackgroundColor { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
#pragma warning disable CS0612
                // before 34.0
                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0))
                {
                    if (Enum.TryParse(BackgroundV1.ToString(), out BackgroundType value))
                    {
                        Background = value;
                    }
                }
#pragma warning restore CS0612
            }

            public void RestoreConfig(Config config)
            {
                config.Background.CustomBackground = CustomBackground;
                config.Background.BackgroundType = Background;
                config.Background.PageBackgroundColor = PageBackgroundColor;
            }
        }

        #endregion

    }
}
