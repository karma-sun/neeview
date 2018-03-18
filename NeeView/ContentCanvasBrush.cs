using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
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
        // system object
        public static ContentCanvasBrush Current { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="contentCanvas"></param>
        public ContentCanvasBrush(ContentCanvas contentCanvas)
        {
            Current = this;

            _contentCanvas = contentCanvas;

            _contentCanvas.ContentChanged +=
                (s, e) => UpdateBackgroundBrush();

            this.CustomBackground = new BrushSource();
        }

        //
        private ContentCanvas _contentCanvas;


        // Foregroudh Brush：ファイルページのフォントカラー用
        private Brush _foregroundBrush = Brushes.White;
        public Brush ForegroundBrush
        {
            get { return _foregroundBrush; }
            set { if (_foregroundBrush != value) { _foregroundBrush = value; RaisePropertyChanged(); } }
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

        //
        private void UpdateCustomBackgroundBrush()
        {
            _customBackgroundBrush = null;
            _customBackgroundFrontBrush = null;
            if (Background == BackgroundStyle.Custom)
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
            get { return _customBackgroundBrush ?? (_customBackgroundBrush = _customBackground?.CreateBackBrush()); }
        }


        /// <summary>
        /// カスタム背景
        /// </summary>
        private Brush _customBackgroundFrontBrush;
        public Brush CustomBackgroundFrontBrush
        {
            get { return _customBackgroundFrontBrush ?? (_customBackgroundFrontBrush = _customBackground?.CreateFrontBrush()); }
        }

        /// <summary>
        /// チェック模様
        /// </summary>
        public Brush CheckBackgroundBrush { get; } = (DrawingBrush)App.Current.Resources["CheckerBrush"];



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
            BackgroundFrontBrush = CreateBackgroundFrontBrush(Config.Current.Dpi);
        }


        /// <summary>
        /// 背景ブラシ作成
        /// </summary>
        /// <returns></returns>
        public Brush CreateBackgroundBrush()
        {
            switch (this.Background)
            {
                default:
                case BackgroundStyle.Black:
                    return Brushes.Black;
                case BackgroundStyle.White:
                    return Brushes.White;
                case BackgroundStyle.Auto:
                    return new SolidColorBrush(ContentCanvas.Current.GetContentColor());
                case BackgroundStyle.Check:
                    return null;
                case BackgroundStyle.Custom:
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
            switch (this.Background)
            {
                default:
                case BackgroundStyle.Black:
                case BackgroundStyle.White:
                case BackgroundStyle.Auto:
                    return null;
                case BackgroundStyle.Check:
                    {
                        var brush = CheckBackgroundBrush.Clone();
                        brush.Transform = new ScaleTransform(1.0 / dpi.DpiScaleX, 1.0 / dpi.DpiScaleY);
                        return brush;
                    }
                case BackgroundStyle.Custom:
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
        public class Memento
        {
            [DataMember]
            public BackgroundStyle Background { get; set; }
            [DataMember]
            public BrushSource CustomBackground { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.CustomBackground = this.CustomBackground;
            memento.Background = this.Background;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.CustomBackground = memento.CustomBackground;
            this.Background = memento.Background;
        }
        #endregion

    }
}
