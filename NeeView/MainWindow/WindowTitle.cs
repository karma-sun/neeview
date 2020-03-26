using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using NeeView.Windows.Property;
using NeeView.Text;

namespace NeeView
{
    /// <summary>
    /// ウィンドウタイトル更新項目
    /// </summary>
    [Flags]
    public enum WindowTitleMask
    {
        None = 0,
        Book = (1 << 0),
        Page = (1 << 1),
        View = (1 << 2),
        All = 0xFFFF
    }

    /// <summary>
    /// ウィンドウタイトル
    /// </summary>
    public class WindowTitle : BindableBase
    {
        static WindowTitle() => Current = new WindowTitle();
        public static WindowTitle Current { get; }

        #region Fields

        // 標準ウィンドウタイトル
        private string _defaultWindowTitle;

        // ウィンドウタイトル
        private string _title = "";

        // ウィンドウタイトル用キーワード置換
        private ReplaceString _windowTitleFormatter = new ReplaceString();

#if false
        // ウィンドウタイトルフォーマット
        private const string WindowTitleFormat1Default = "$Book ($Page / $PageMax) - $FullName";
        private const string WindowTitleFormat2Default = "$Book ($Page / $PageMax) - $FullNameL | $NameR";
        private const string WindowTitleFormatMediaDefault = "$Book";
        private string _windowTitleFormat1;
        private string _windowTitleFormat2;
        private string _windowTitleFormatMedia;
#endif

        // ロード中表示用
        private string _loadingPath;

        #endregion

        #region Constructors

        private WindowTitle()
        {
            ContentCanvas.Current.ContentChanged += ContentCanvas_ContentChanged;

            DragTransform.Current.AddPropertyChanged(nameof(DragTransform.Scale), DragTransform_ScaleChanged);

            // Window title
            _defaultWindowTitle = $"{Environment.ApplicationName} {Environment.DispVersion}";
#if DEBUG
            _defaultWindowTitle += " [Debug]";
#endif

            BookHub.Current.Loading +=
                (s, e) => this.LoadingPath = e.Path;

            Config.Current.WindowTittle.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(WindowTitleConfig.WindowTitleFormat1):
                    case nameof(WindowTitleConfig.WindowTitleFormat2):
                    case nameof(WindowTitleConfig.WindowTitleFormatMedia):
                        UpdateFomatterFilter();
                        UpdateWindowTitle(WindowTitleMask.None);
                        break;
                }
            };

            UpdateWindowTitle(WindowTitleMask.All);
        }

        #endregion

        #region Properties

        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        public string Title
        {
            get { return _title; }
            private set { _title = value; RaisePropertyChanged(); }
        }

#if false
        /// <summary>
        /// ウィンドウタイトルフォーマット 1P用
        /// </summary>
        [PropertyMember("@ParamWindowTitleFormat1")]
        public string WindowTitleFormat1
        {
            get { return _windowTitleFormat1 ?? WindowTitleFormat1Default; }
            set
            {
                if (SetProperty(ref _windowTitleFormat1, CleanUpTitleFormat(value, WindowTitleFormat1Default)))
                {
                    UpdateFomatterFilter();
                    UpdateWindowTitle(WindowTitleMask.None);
                }
            }
        }

        /// <summary>
        /// ウィンドウタイトルフォーマット 2P用
        /// </summary>
        [PropertyMember("@ParamWindowTitleFormat2")]
        public string WindowTitleFormat2
        {
            get { return _windowTitleFormat2 ?? WindowTitleFormat2Default; }
            set
            {
                if (SetProperty(ref _windowTitleFormat2, CleanUpTitleFormat(value, WindowTitleFormat2Default)))
                {
                    UpdateFomatterFilter();
                    UpdateWindowTitle(WindowTitleMask.None);
                }
            }
        }


        [PropertyMember("@ParamWindowTitleFormatMedia")]
        public string WindowTitleFormatMedia
        {
            get { return _windowTitleFormatMedia ?? WindowTitleFormatMediaDefault; }
            set
            {
                if (SetProperty(ref _windowTitleFormatMedia, CleanUpTitleFormat(value, WindowTitleFormatMediaDefault)))
                {
                    UpdateFomatterFilter();
                    UpdateWindowTitle(WindowTitleMask.None);
                }
            }
        }
#endif

        /// <summary>
        /// ロード中パス
        /// TODO : 定義位置ここか？
        /// </summary>
        public string LoadingPath
        {
            get { return _loadingPath; }
            set { _loadingPath = value; UpdateWindowTitle(WindowTitleMask.All); }
        }

        #endregion

        #region Methods

#if false
        private string CleanUpTitleFormat(string source, string defaultFormat)
        {
            if (string.IsNullOrEmpty(source) || source == defaultFormat)
            {
                return null;
            }
            else
            {
                return source;
            }
        }
#endif

        // フォーマットの使用キーワード更新
        private void UpdateFomatterFilter()
        {
            _windowTitleFormatter.SetFilter(Config.Current.WindowTittle.WindowTitleFormat1 + " " + Config.Current.WindowTittle.WindowTitleFormat2 + " " + Config.Current.WindowTittle.WindowTitleFormatMedia);
        }

        /// <summary>
        /// ドラッグ操作により画像スケールが変更されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DragTransform_ScaleChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateWindowTitle(WindowTitleMask.View);
        }

        /// <summary>
        /// キャンバスサイズが変更されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentCanvas_ContentChanged(object sender, EventArgs e)
        {
            UpdateWindowTitle(WindowTitleMask.All);
        }

        /// <summary>
        /// ウィンドウタイトル更新
        /// </summary>
        /// <param name="mask"></param>
        private void UpdateWindowTitle(WindowTitleMask mask)
        {
            var address = BookHub.Current.Book?.Address;

            if (_loadingPath != null)
                Title = LoosePath.GetFileName(_loadingPath) + " " + Properties.Resources.NotifyLoadingTitle;

            else if (address == null)
                Title = _defaultWindowTitle;

            else if (ContentCanvas.Current.MainContent?.Source == null)
                Title = LoosePath.GetDispName(address);

            else
                Title = CreateWindowTitle(mask);
        }

        /// <summary>
        /// ウィンドウタイトル作成
        /// </summary>
        /// <param name="mask">更新項目マスク</param>
        /// <returns></returns>
        private string CreateWindowTitle(WindowTitleMask mask)
        {
            var MainContent = ContentCanvas.Current.MainContent;
            var Contents = ContentCanvas.Current.CloneContents;
            var _viewScale = DragTransform.Current.Scale;

            string format = MainContent is MediaViewContent
                ? Config.Current.WindowTittle.WindowTitleFormatMedia
                : Contents[1].IsValid ? Config.Current.WindowTittle.WindowTitleFormat2 : Config.Current.WindowTittle.WindowTitleFormat1;

            bool isMainContent0 = MainContent == Contents[0];

            if ((mask & WindowTitleMask.Book) != 0)
            {
                string bookName = LoosePath.GetDispName(BookOperation.Current.Book?.Address);
                _windowTitleFormatter.Set("$Book", bookName);
            }

            if ((mask & WindowTitleMask.Page) != 0)
            {
                _windowTitleFormatter.Set("$PageMax", (BookOperation.Current.GetMaxPageIndex() + 1).ToString());

                string pageNum0 = GetPageNum(Contents[0]);
                string pageNum1 = GetPageNum(Contents[1]);
                _windowTitleFormatter.Set("$Page", isMainContent0 ? pageNum0 : pageNum1);
                _windowTitleFormatter.Set("$PageL", pageNum1);
                _windowTitleFormatter.Set("$PageR", pageNum0);

                string GetPageNum(ViewContent content)
                {
                    return content.IsValid ? (content.Source.PagePart.PartSize == 2) ? (content.Position.Index + 1).ToString() : (content.Position.Index + 1).ToString() + (content.Position.Part == 1 ? ".5" : ".0") : "";
                }

                string path0 = GetFullName(Contents[0]);
                string path1 = GetFullName(Contents[1]);
                _windowTitleFormatter.Set("$FullName", isMainContent0 ? path0 : path1);
                _windowTitleFormatter.Set("$FullNameL", path1);
                _windowTitleFormatter.Set("$FullNameR", path0);

                string GetFullName(ViewContent content)
                {
                    return content.IsValid ? content.FullPath.Replace("/", " > ").Replace("\\", " > ") + content.GetPartString() : "";
                }

                string name0 = GetName(Contents[0]);
                string name1 = GetName(Contents[1]);
                _windowTitleFormatter.Set("$Name", isMainContent0 ? name0 : name1);
                _windowTitleFormatter.Set("$NameL", name1);
                _windowTitleFormatter.Set("$NameR", name0);

                string GetName(ViewContent content)
                {
                    return content.IsValid ? LoosePath.GetFileName(content.FullPath) + content.GetPartString() : "";
                }

                var bitmapContent0 = Contents[0].Content as BitmapContent;
                var bitmapContent1 = Contents[1].Content as BitmapContent;

                var pictureInfo0 = bitmapContent0?.PictureInfo;
                var pictureInfo1 = bitmapContent1?.PictureInfo;

                string bpp0 = GetSizeEx(pictureInfo0);
                string bpp1 = GetSizeEx(pictureInfo1);
                _windowTitleFormatter.Set("$SizeEx", isMainContent0 ? bpp0 : bpp1);
                _windowTitleFormatter.Set("$SizeExL", bpp1);
                _windowTitleFormatter.Set("$SizeExR", bpp0);

                string GetSizeEx(PictureInfo pictureInfo)
                {
                    return pictureInfo != null ? GetSize(pictureInfo) + "×" + pictureInfo.BitsPerPixel.ToString() : "";
                }

                string size0 = GetSize(pictureInfo0);
                string size1 = GetSize(pictureInfo1);
                _windowTitleFormatter.Set("$Size", isMainContent0 ? size0 : size1);
                _windowTitleFormatter.Set("$SizeL", size1);
                _windowTitleFormatter.Set("$SizeR", size0);

                string GetSize(PictureInfo pictureInfo)
                {
                    return pictureInfo != null ? $"{pictureInfo.OriginalSize.Width}×{pictureInfo.OriginalSize.Height}" : "";
                }
            }

            if ((mask & WindowTitleMask.View) != 0)
            {
                _windowTitleFormatter.Set("$ViewScale", $"{(int)(_viewScale * 100 + 0.1)}%");
            }

            if ((mask & (WindowTitleMask.Page | WindowTitleMask.View)) != 0)
            {
                var _Dpi = Environment.Dpi;

                string scale0 = Contents[0].IsValid ? $"{(int)(_viewScale * Contents[0].Scale * _Dpi.DpiScaleX * 100 + 0.1)}%" : "";
                string scale1 = Contents[1].IsValid ? $"{(int)(_viewScale * Contents[1].Scale * _Dpi.DpiScaleX * 100 + 0.1)}%" : "";
                _windowTitleFormatter.Set("$Scale", isMainContent0 ? scale0 : scale1);
                _windowTitleFormatter.Set("$ScaleL", scale1);
                _windowTitleFormatter.Set("$ScaleR", scale0);
            }

            return _windowTitleFormatter.Replace(format);
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [DataMember(EmitDefaultValue = false)]
            public string WindowTitleFormat1 { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string WindowTitleFormat2 { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string WindowTitleFormatMedia { get; set; }


            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
                // before 34.0
                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0))
                {
                    const string WindowTitleFormat1Default = "$Book($Page/$PageMax) - $FullName";
                    const string WindowTitleFormat2Default = "$Book($Page/$PageMax) - $FullNameL | $NameR";
                    const string WindowTitleFormatMediaDefault = "$Book";

                    if (WindowTitleFormat1 == WindowTitleFormat1Default)
                    {
                        WindowTitleFormat1 = null;
                    }
                    if (WindowTitleFormat2 == WindowTitleFormat2Default)
                    {
                        WindowTitleFormat2 = null;
                    }
                    if (WindowTitleFormatMedia == WindowTitleFormatMediaDefault)
                    {
                        WindowTitleFormatMedia = null;
                    }
                }
            }

            public void RestoreConfig(Config config)
            {
                config.WindowTittle.WindowTitleFormat1 = WindowTitleFormat1;
                config.WindowTittle.WindowTitleFormat2 = WindowTitleFormat2;
                config.WindowTittle.WindowTitleFormatMedia = WindowTitleFormatMedia;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.WindowTitleFormat1 = Config.Current.WindowTittle.WindowTitleFormat1;
            memento.WindowTitleFormat2 = Config.Current.WindowTittle.WindowTitleFormat2;
            memento.WindowTitleFormatMedia = Config.Current.WindowTittle.WindowTitleFormatMedia;
            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            ////this.WindowTitleFormat1 = memento.WindowTitleFormat1;
            ////this.WindowTitleFormat2 = memento.WindowTitleFormat2;
            ////this.WindowTitleFormatMedia = memento.WindowTitleFormatMedia;
        }

        #endregion
    }
}
