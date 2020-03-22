using NeeLaboratory.ComponentModel;
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
        static ContentCanvasBrush() => Current = new ContentCanvasBrush();
        public static ContentCanvasBrush Current { get; }


        private ContentCanvasBrush()
        {
            ContentCanvas.Current.ContentChanged +=
                (s, e) => UpdateBackgroundBrush();

            ////this.CustomBackground = new BrushSource();


            Config.Current.Layout.Background.AddPropertyChanged(nameof(BackgroundConfig.CustomBackground), (s, e) =>
            {
                InitializeCustomBackgroundBrush();
            });

            Config.Current.Layout.Background.AddPropertyChanged(nameof(BackgroundConfig.BackgroundType), (s, e) =>
            {
                UpdateBackgroundBrush();
            });

            Config.Current.Layout.Background.AddPropertyChanged(nameof(BackgroundConfig.PageBackgroundColor), (s, e) =>
            {
                RaisePropertyChanged(nameof(PageBackgroundBrush));
            });


            // Initialize
            InitializeCustomBackgroundBrush();
            UpdateBackgroundBrush();
        }

        private void InitializeCustomBackgroundBrush()
        {
            UpdateCustomBackgroundBrush();
            if (Config.Current.Layout.Background.CustomBackground != null)
            {
                Config.Current.Layout.Background.CustomBackground.PropertyChanged += (s, e) => UpdateCustomBackgroundBrush();
            }
        }


        // Foregroudh Brush：ファイルページのフォントカラー用
        private SolidColorBrush _foregroundBrush = Brushes.White;
        public SolidColorBrush ForegroundBrush
        {
            get { return _foregroundBrush; }
            set { if (_foregroundBrush != value) { _foregroundBrush = value; RaisePropertyChanged(); } }
        }

#if false
        // ページの背景色。透過画像用
        private Color _pageBackgroundColor = Colors.Transparent;
        [PropertyMember("@ParamPageBackgroundColor")]
        public Color PageBackgroundColor
        {
            get { return _pageBackgroundColor; }
            set
            {
                if (SetProperty(ref _pageBackgroundColor, value))
                {
                    RaisePropertyChanged(nameof(PageBackgroundBrush));
                }
            }
        }
#endif

        public Brush PageBackgroundBrush
        {
            get { return new SolidColorBrush(Config.Current.Layout.Background.PageBackgroundColor); }
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


#if false
        // 背景スタイル
        private BackgroundStyle _background = BackgroundStyle.Black;
        public BackgroundStyle Background
        {
            get { return _background; }
            set { _background = value; UpdateBackgroundBrush(); RaisePropertyChanged(); }
        }

        /// <summary>
        /// CustomBackground property.
        /// </summary>
        private BrushSource _customBackground;
        [PropertyMember("@ParamCustomBackground", Tips = "@ParamCustomBackgroundTips")]
        public BrushSource CustomBackground
        {
            get { return _customBackground; }
            set
            {
                if (_customBackground != value)
                {
                    _customBackground = value ?? new BrushSource();
                    UpdateCustomBackgroundBrush();
                    _customBackground.PropertyChanged += (s, e) => UpdateCustomBackgroundBrush();
                }
            }
        }
#endif

        //
        private void UpdateCustomBackgroundBrush()
        {
            _customBackgroundBrush = null;
            _customBackgroundFrontBrush = null;
            if (Config.Current.Layout.Background.BackgroundType == BackgroundType.Custom)
            {
                UpdateBackgroundBrush();
            }
        }

        /// <summary>
        /// カスタム背景
        /// </summary>
        private Brush _customBackgroundBrush;
        public Brush CustomBackgroundBrush
        {
            get { return _customBackgroundBrush ?? (_customBackgroundBrush = Config.Current.Layout.Background.CustomBackground?.CreateBackBrush()); }
        }


        /// <summary>
        /// カスタム背景
        /// </summary>
        private Brush _customBackgroundFrontBrush;
        public Brush CustomBackgroundFrontBrush
        {
            get { return _customBackgroundFrontBrush ?? (_customBackgroundFrontBrush = Config.Current.Layout.Background.CustomBackground?.CreateFrontBrush()); }
        }

        /// <summary>
        /// チェック模様
        /// </summary>
        public Brush CheckBackgroundBrush { get; } = (DrawingBrush)App.Current.Resources["CheckerBrush"];
        public Brush CheckBackgroundBrushDark { get; } = (DrawingBrush)App.Current.Resources["CheckerBrushDark"];



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
            BackgroundFrontBrush = CreateBackgroundFrontBrush(Environment.Dpi);
        }


        /// <summary>
        /// 背景ブラシ作成
        /// </summary>
        /// <returns></returns>
        public Brush CreateBackgroundBrush()
        {
            switch (Config.Current.Layout.Background.BackgroundType)
            {
                default:
                case BackgroundType.Black:
                    return Brushes.Black;
                case BackgroundType.White:
                    return Brushes.White;
                case BackgroundType.Auto:
                    return new SolidColorBrush(ContentCanvas.Current.GetContentColor());
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
            switch (Config.Current.Layout.Background.BackgroundType)
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
                config.Layout.Background.CustomBackground = CustomBackground;
                config.Layout.Background.BackgroundType = Background;
                config.Layout.Background.PageBackgroundColor = PageBackgroundColor;
            }
        }


        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.CustomBackground = Config.Current.Layout.Background.CustomBackground;
            memento.Background = Config.Current.Layout.Background.BackgroundType;
            memento.PageBackgroundColor = Config.Current.Layout.Background.PageBackgroundColor;
            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            ////this.CustomBackground = memento.CustomBackground;
            ////this.Background = memento.Background;
            ////this.PageBackgroundColor = memento.PageBackgroundColor;
        }
        #endregion

    }
}
