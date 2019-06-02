using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        #region Constructors

        /// <summary>
        /// コンストラクター
        /// </summary>
        public ViewContent()
        {
        }

        public ViewContent(ViewContentSource source)
        {
            this.Source = source;
            this.Size = source.Size;
            this.Color = Colors.Black;
        }

        #endregion

        #region Properties, Fields

        /// <summary>
        /// ViewContentSource
        /// TODO: 他のパラメータとあわせて整備
        /// </summary>
        public ViewContentSource Source { get; set; }

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
        private ViewContentControl _view;
        public ViewContentControl View
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
        public string FullPath => Page?.EntryFullName;

        // ファイル名
        public string FileName => LoosePath.GetFileName(Page?.EntryFullName.TrimEnd('\\'));

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

        // 表示スケール
        public double Scale => Source != null ? Width / Source.Size.Width : 1.0;

        //
        public bool IgnoreReserver { get; set; }

        /// <summary>
        /// IsResizing property.
        /// </summary>
        public bool IsResizing { get; protected set; }

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
