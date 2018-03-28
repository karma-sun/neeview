using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// ページ表示用コンテンツ
    /// </summary>
    public class ViewContent : BindableBase, IDisposable
    {
        #region Properties, Fields

        /// <summary>
        /// ViewContentSource
        /// TODO: 他のパラメータとあわせて整備
        /// </summary>
        public ViewPage Source { get; set; }

        /// <summary>
        /// ページ
        /// </summary>
        public Page Page => Source?.Page;

        /// <summary>
        /// コンテンツ
        /// </summary>
        public PageContent Content => Source?.Content;

        /// <summary>
        /// Property: View.
        /// </summary>
        private FrameworkElement _view;
        public FrameworkElement View
        {
            get { return _view; }
            set { _view = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// コンテンツの幅 (with DPI).
        /// 表示の基準となるコンテンツサイズ。表示スケール(マウスやルーペ)を除外した値？
        /// </summary>
        private double _width;
        public double Width
        {
            get { return _width; }
            set { _width = value; RaisePropertyChanged(); }
        }

        // コンテンツの高さ (with DPI)
        private double _height;
        public double Height
        {
            get { return _height; }
            set { _height = value; RaisePropertyChanged(); }
        }

        // 分割？
        public bool IsHalf => this.Source != null && this.Source.PagePart.PartSize == 1;

        // コンテンツのオリジナルサイズ
        private Size _size;
        public Size Size
        {
            get { return IsValid ? _size : SizeExtensions.Zero; }
            set { _size = value; }
        }

        // コンテンツの色
        public Color Color = Colors.Black;

        // フルパス名
        public string FullPath => Page?.FullPath;

        // ファイル名
        public string FileName => LoosePath.GetFileName(Page?.FullPath.TrimEnd('\\'));

        // フォルダーの場所
        public string FolderPlace => Page?.GetFolderPlace();


        // ファイルプロキシ(必要であれば)
        // 寿命確保用。GCされてファイルが消えないように。
        public FileProxy FileProxy { get; set; }

        // ページの場所
        public PagePosition Position => Source.PagePart.Position;


        // スケールモード
        private BitmapScalingMode _bitmapScalingMode = BitmapScalingMode.HighQuality;
        public BitmapScalingMode BitmapScalingMode
        {
            get { return _bitmapScalingMode; }
            set { _bitmapScalingMode = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// AnimationImageVisibility property.
        /// </summary>
        private Visibility _AnimationImageVisibility = Visibility.Collapsed;
        public Visibility AnimationImageVisibility
        {
            get { return _AnimationImageVisibility; }
            set { if (_AnimationImageVisibility != value) { _AnimationImageVisibility = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// AnimationPlayerVisibility property.
        /// </summary>
        private Visibility _AnimationPlayerVisibility = Visibility.Visible;
        public Visibility AnimationPlayerVisibility
        {
            get { return _AnimationPlayerVisibility; }
            set { if (_AnimationPlayerVisibility != value) { _AnimationPlayerVisibility = value; RaisePropertyChanged(); } }
        }

        // 有効判定
        public bool IsValid => (View != null);


        // 表示スケール(%)
        public double Scale => Width / Size.Width;

        //
        public ViewContentReserver Reserver { get; set; }

        //
        public bool IgnoreReserver { get; set; }

        /// <summary>
        /// IsResizing property.
        /// </summary>
        public bool IsResizing { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// コンストラクター
        /// </summary>
        public ViewContent()
        {
        }

        public ViewContent(ViewPage source, ViewContent old)
        {
            this.Source = source;
            this.Size = source.Size;
            this.Color = Colors.Black;

            this.Reserver = old.CreateReserver();
        }

        #endregion

        #region Methods

        // ページパーツ文字
        public string GetPartString()
        {
            if (Source.PagePart.PartSize == 1)
            {
                int part = Source.PagePart.PartOrder == PageReadOrder.LeftToRight ? 1 - Source.PagePart.Position.Part : Source.PagePart.Position.Part;
                return part == 0 ? "(R)" : "(L)";
            }
            else
            {
                return "";
            }
        }


        //
        protected ViewContentParameters CreateBindingParameter()
        {
            var parameter = new ViewContentParameters()
            {
                ForegroundBrush = new Binding(nameof(ContentCanvasBrush.ForegroundBrush)) { Source = ContentCanvasBrush.Current },
                PageBackgroundBrush = new Binding(nameof(ContentCanvasBrush.PageBackgroundBrush)) { Source = ContentCanvasBrush.Current },
                BitmapScalingMode = new Binding(nameof(BitmapScalingMode)) { Source = this },
                AnimationImageVisibility = new Binding(nameof(AnimationImageVisibility)) { Source = this },
                AnimationPlayerVisibility = new Binding(nameof(AnimationPlayerVisibility)) { Source = this },
            };

            return parameter;
        }

        //
        public virtual bool IsBitmapScalingModeSupported() => false;

        //
        public virtual Brush GetViewBrush()
        {
            return null;
        }

        //
        public ViewContentReserver CreateReserver()
        {
            if (BookProfile.Current.LoadingPageView == LoadingPageView.None || this.IgnoreReserver)
            {
                return null;
            }

            ImageBrush brush = this.GetViewBrush() as ImageBrush;
            if (brush != null)
            {
                if (BookProfile.Current.LoadingPageView == LoadingPageView.PreThumbnail)
                {
                    var thumbnail = this.Page?.Thumbnail?.BitmapSource;
                    if (thumbnail != null)
                    {
                        return new ViewContentReserver()
                        {
                            Brush = this.Source.ClonePageImageBrush(brush, thumbnail),
                            Size = this.Size,
                            Color = this.Color
                        };
                    }
                }

                return new ViewContentReserver()
                {
                    Brush = brush,
                    Size = this.Size,
                    Color = this.Color
                };
            }

            return this.Reserver;
        }

        //
        public virtual bool Rebuild(double scale)
        {
            ////Debug.WriteLine($"UpdateContent: {Width}x{Height} x{scale}");
            return true;
        }

        /// <summary>
        /// ビューモードの設定
        /// ドットバイドット表示用
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="viewScale"></param>
        public virtual void SetViewMode(ContentViewMode mode, double viewScale) { }

        #endregion

        #region IDisposable Support

        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }

    /// <summary>
    /// ContentViewMode
    /// </summary>
    public enum ContentViewMode
    {
        /// <summary>
        /// 標準
        /// </summary>
        Scale,

        /// <summary>
        /// ドットバイドット表示
        /// </summary>
        Pixeled,
    }

    /// <summary>
    /// Reserver
    /// </summary>
    public class ViewContentReserver
    {
        public ImageBrush Brush { get; set; }
        public Size Size { get; set; }
        public Color Color { get; set; }
    }

    /// <summary>
    /// View生成用パラメータ
    /// </summary>
    public class ViewContentParameters
    {
        public Binding ForegroundBrush { get; set; }
        public Binding PageBackgroundBrush { get; set; }
        public Binding BitmapScalingMode { get; set; }
        public Binding AnimationImageVisibility { get; set; }
        public Binding AnimationPlayerVisibility { get; set; }
    }
}
