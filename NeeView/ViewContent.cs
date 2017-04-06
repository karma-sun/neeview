// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// Reserver
    /// </summary>
    public class ViewContentReserver
    {
        public Thumbnail Thumbnail { get; set; }
        public Size Size { get; set; }
        public Color Color { get; set; }
    }
    /// <summary>
    /// ページ表示用コンテンツ
    /// </summary>
    public class ViewContent : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        /// <summary>
        /// ViewContentSource
        /// TODO: 他のパラメータとあわせて整備
        /// </summary>
        public ViewContentSource Source { get; set; }

        /// <summary>
        /// コンテンツ
        /// </summary>
        public PageContent Content;

        /// <summary>
        /// Property: View.
        /// </summary>
        private PageContentView _view;
        public PageContentView View
        {
            get { return _view; }
            set { _view = value; RaisePropertyChanged(); }
        }

        // コンテンツの幅 (with DPI)
        #region Property: Width
        private double _width;
        public double Width
        {
            get { return _width; }
            set { _width = value; RaisePropertyChanged(); }
        }
        #endregion

        // コンテンツの高さ (with DPI)
        #region Property: Height
        private double _height;
        public double Height
        {
            get { return _height; }
            set { _height = value; RaisePropertyChanged(); }
        }
        #endregion

        // コンテンツのオリジナルサイズ
        private Size _size;
        public Size Size
        {
            get { return IsValid ? _size : new Size(0, 0); }
            set { _size = value; }
        }

        // コンテンツソースサイズ
        public Size SourceSize { get; set; }

        // コンテンツの色
        public Color Color = Colors.Black;

        // 表示名
        public string FullPath { get; set; }

        // フォルダーの場所 ページの上位の有効パス
        public string FolderPlace { get; set; }

        // ファイルの場所 ページを含む有効パス
        public string FilePlace { get; set; }

        // ファイル名
        public string FileName => LoosePath.GetFileName(FullPath.TrimEnd('\\'));


        // ファイルプロキシ(必要であれば)
        // 寿命確保用。GCされてファイルが消えないように。
        public FileProxy FileProxy { get; set; }

        // ページの場所
        public PagePosition Position { get; set; }

        // 表示パーツサイズ
        public int PartSize { get; set; }

        // 方向
        public PageReadOrder ReadOrder { get; set; }

        // スケールモード
        #region Property: BitmapScalingMode
        private BitmapScalingMode _bitmapScalingMode = BitmapScalingMode.HighQuality;
        public BitmapScalingMode BitmapScalingMode
        {
            get { return _bitmapScalingMode; }
            set { _bitmapScalingMode = value; RaisePropertyChanged(); }
        }
        #endregion

        // 有効判定
        public bool IsValid => (View != null);

        // ページパーツ文字
        public string GetPartString()
        {
            if (PartSize == 1)
            {
                int part = ReadOrder == PageReadOrder.LeftToRight ? 1 - Position.Part : Position.Part;
                return part == 0 ? "(R)" : "(L)";
            }
            else
            {
                return "";
            }
        }

        // 表示スケール(%)
        public double Scale => Width / Size.Width;

        //
        public ViewContentReserver Reserver { get; set; }

        /// <summary>
        /// コンストラクター
        /// </summary>
        public ViewContent()
        {
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="source"></param>
        /// <param name="oldViewContent"></param>
        public ViewContent(ViewContentSource source, ViewContent oldViewContent)
        { 
            var contentType = source.GetContentType();

            this.Source = source;
            this.Content = source.Content;
            this.Size = source.Size;
            this.SourceSize = source.Content.Size;
            this.Color = Colors.Black;
            this.FolderPlace = source.Page.GetFolderPlace();
            this.FilePlace = source.Page.GetFilePlace();
            this.FullPath = source.Page.FullPath;
            this.Position = source.Position;
            this.PartSize = source.PartSize;
            this.ReadOrder = source.ReadOrder;

            //
            var parameter = new ViewContentParameters()
            {
                ForegroundBrush = new Binding("ForegroundBrush"),
                BitmapScalingMode = new Binding(nameof(BitmapScalingMode)) { Source = this },
            };

            switch (contentType)
            {
                case ViewContentType.Message:
                    InitializeMessageContent(source, parameter);
                    break;
                case ViewContentType.Thumbnail:
                    InitializeThumbnailContent(source, parameter, oldViewContent);
                    break;
                case ViewContentType.Anime:
                    InitializeAnimatedContent(source, parameter);
                    break;
                case ViewContentType.Bitmap:
                    InitializeBitmapContent(source, parameter);
                    break;
            }
        }

        /// <summary>
        /// メッセージコンテンツ生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        private void InitializeMessageContent(ViewContentSource source, ViewContentParameters parameter)
        {
            InitializeView(CreateMessageView(source, parameter), source);

            this.Size = new Size(480, 480);
        }

        /// <summary>
        /// サムネイルコンテンツ生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <param name="oldViewContent"></param>
        private void InitializeThumbnailContent(ViewContentSource source, ViewContentParameters parameter, ViewContent oldViewContent)
        {
            InitializeView(CreateThumbnailView(source, parameter), source);

            this.View.SetText(LoosePath.GetFileName(source.Page.FullPath));

            if (this.Size.Width == 0 && this.Size.Height == 0)
            {
                if (oldViewContent != null && oldViewContent.IsValid)
                {
                    this.Size = oldViewContent.Size;
                    this.Color = oldViewContent.Color;
                }
                else
                {
                    this.Size = new Size(480, 680);
                    this.Color = Colors.Black;
                }
            }
            else
            {
                var bitmapinfo = (this.Content as BitmapContent)?.BitmapInfo;
                this.Color = bitmapinfo != null ? bitmapinfo.Color : Colors.Black;
            }
        }

        /// <summary>
        /// アニメーションコンテンツ生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="patameter"></param>
        private void InitializeAnimatedContent(ViewContentSource source, ViewContentParameters patameter)
        {
            InitializeView(CreateAnimatedView(source, patameter), source);

            var animatedContent = source.Content as AnimatedContent;
            this.Color = animatedContent.BitmapInfo.Color;
            this.FileProxy = animatedContent.FileProxy;
        }

        /// <summary>
        /// 画像コンテンツ生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        private void InitializeBitmapContent(ViewContentSource source, ViewContentParameters parameter)
        {
            InitializeView(CreateBitmapView(source, parameter), source);

            var bitmapContent = source.Content as BitmapContent;
            this.Color = bitmapContent.BitmapInfo.Color;
        }

        /// <summary>
        /// ページビュー生成
        /// </summary>
        /// <param name="element">コンテンツビュー</param>
        /// <param name="source"></param>
        private void InitializeView(FrameworkElement element, ViewContentSource source)
        {
            this.View = new PageContentView(element, LoosePath.GetFileName(source.Page.FullPath));
        }


        /// <summary>
        /// メッセージビュー生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public FrameworkElement CreateMessageView(ViewContentSource source, ViewContentParameters parameter)
        {
            var filepage = new FilePageContent()
            {
                Icon = Content.PageMessage.Icon,
                FileName = Content.Entry.EntryName,
                Message = Content.PageMessage.Message,
            };

            var control = new FilePageControl(filepage);
            control.SetBinding(FilePageControl.DefaultBrushProperty, parameter.ForegroundBrush);
            return control;
        }

        /// <summary>
        /// サムネイルビュー生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public FrameworkElement CreateThumbnailView(ViewContentSource source, ViewContentParameters parameter)
        {
            var rectangle = new Rectangle();
            rectangle.Fill = source.CreateThumbnailBrush(parameter.Reserver);
            RenderOptions.SetBitmapScalingMode(rectangle, BitmapScalingMode.HighQuality);
            return rectangle;
        }

        /// <summary>
        /// アニメーションビュー生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public FrameworkElement CreateAnimatedView(ViewContentSource source, ViewContentParameters parameter)
        {
            var media = new MediaElement();
            media.Source = new Uri(((AnimatedContent)Content).FileProxy.Path);
            media.MediaEnded += (s, e_) => media.Position = TimeSpan.FromMilliseconds(1);
            media.MediaFailed += (s, e_) => { throw new ApplicationException("MediaElementで致命的エラー", e_.ErrorException); };
            media.SetBinding(RenderOptions.BitmapScalingModeProperty, parameter.BitmapScalingMode);

            var brush = new VisualBrush();
            brush.Visual = media;
            brush.Stretch = Stretch.Fill;
            brush.Viewbox = source.GetViewBox();

            var rectangle = new Rectangle();
            rectangle.Fill = brush;
            return rectangle;
        }

        /// <summary>
        /// 画像ビュー生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public FrameworkElement CreateBitmapView(ViewContentSource source, ViewContentParameters parameter)
        {
            var rectangle = new Rectangle();
            rectangle.Fill = source.CreatePageImageBrush(((BitmapContent)Content).BitmapSource);
            rectangle.SetBinding(RenderOptions.BitmapScalingModeProperty, parameter.BitmapScalingMode);
            rectangle.UseLayoutRounding = true;
            rectangle.SnapsToDevicePixels = true;
            return rectangle;
        }
    }


    /// <summary>
    /// View生成用パラメータ
    /// </summary>
    public class ViewContentParameters
    {
        public Binding ForegroundBrush { get; set; }
        public Binding BitmapScalingMode { get; set; }
        public ViewContentReserver Reserver { get; set; } // 未使用
    }

}
