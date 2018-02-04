// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

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
        public static WindowTitle Current { get; private set; }

        #region Fields

        // 標準ウィンドウタイトル
        private string _defaultWindowTitle;

        // ウィンドウタイトル
        private string _title = "";

        // ウィンドウタイトル用キーワード置換
        private ReplaceString _windowTitleFormatter = new ReplaceString();

        // ウィンドウタイトルフォーマット
        private const string WindowTitleFormat1Default = "$Book($Page/$PageMax) - $FullName";
        private const string WindowTitleFormat2Default = "$Book($Page/$PageMax) - $FullNameL | $NameR";
        private string _windowTitleFormat1 = WindowTitleFormat1Default;
        private string _windowTitleFormat2 = WindowTitleFormat2Default;

        // コンテンツキャンバス
        // TODO: ここで保持するものか？
        private ContentCanvas _contentCanvas;

        // ロード中表示用
        private string _loadingPath;

        #endregion

        #region Constructors

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="contentCanvas"></param>
        public WindowTitle(ContentCanvas contentCanvas)
        {
            Current = this;

            _contentCanvas = contentCanvas;
            _contentCanvas.ContentChanged += ContentCanvas_ContentChanged;

            DragTransform.Current.AddPropertyChanged(nameof(DragTransform.Scale), DragTransform_ScaleChanged);

            // Window title
            _defaultWindowTitle = $"{Config.Current.ApplicationName} {Config.Current.ProductVersion}";
#if DEBUG
            _defaultWindowTitle += " [Debug]";
#endif

            BookHub.Current.Loading +=
                (s, e) => this.LoadingPath = e;

            //
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

        /// <summary>
        /// ウィンドウタイトルフォーマット 1P用
        /// </summary>
        public string WindowTitleFormat1
        {
            get { return _windowTitleFormat1; }
            set { _windowTitleFormat1 = value; _windowTitleFormatter.SetFilter(_windowTitleFormat1 + " " + _windowTitleFormat2); }
        }

        /// <summary>
        /// ウィンドウタイトルフォーマット 2P用
        /// </summary>
        public string WindowTitleFormat2
        {
            get { return _windowTitleFormat2; }
            set { _windowTitleFormat2 = value; _windowTitleFormatter.SetFilter(_windowTitleFormat1 + " " + _windowTitleFormat2); }
        }

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
            var place = BookHub.Current.Book?.Place;

            if (_loadingPath != null)
                Title = LoosePath.GetFileName(_loadingPath) + " (読込中)";

            else if (place == null)
                Title = _defaultWindowTitle;

            else if (_contentCanvas.MainContent?.Source == null)
                Title = NVUtility.PlaceToTitle(place);

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
            var MainContent = _contentCanvas.MainContent;
            var Contents = _contentCanvas.Contents;
            var _viewScale = DragTransform.Current.Scale;

            string format = Contents[1].IsValid ? WindowTitleFormat2 : WindowTitleFormat1;

            bool isMainContent0 = MainContent == Contents[0];

            if ((mask & WindowTitleMask.Book) != 0)
            {
                string bookName = NVUtility.PlaceToTitle(BookOperation.Current.Book?.Place);
                _windowTitleFormatter.Set("$Book", bookName);
            }

            if ((mask & WindowTitleMask.Page) != 0)
            {
                string pageNum = (MainContent.Source.PagePart.PartSize == 2)
                ? (MainContent.Position.Index + 1).ToString()
                : (MainContent.Position.Index + 1).ToString() + (MainContent.Position.Part == 1 ? ".5" : ".0");
                _windowTitleFormatter.Set("$PageMax", (BookOperation.Current.GetMaxPageIndex() + 1).ToString());
                _windowTitleFormatter.Set("$Page", pageNum);

                string path0 = Contents[0].IsValid ? Contents[0].FullPath.Replace("/", " > ").Replace("\\", " > ") + Contents[0].GetPartString() : "";
                string path1 = Contents[1].IsValid ? Contents[1].FullPath.Replace("/", " > ").Replace("\\", " > ") + Contents[1].GetPartString() : "";
                _windowTitleFormatter.Set("$FullName", isMainContent0 ? path0 : path1);
                _windowTitleFormatter.Set("$FullNameL", path1);
                _windowTitleFormatter.Set("$FullNameR", path0);

                string name0 = Contents[0].IsValid ? LoosePath.GetFileName(Contents[0].FullPath) + Contents[0].GetPartString() : "";
                string name1 = Contents[1].IsValid ? LoosePath.GetFileName(Contents[1].FullPath) + Contents[1].GetPartString() : "";
                _windowTitleFormatter.Set("$Name", isMainContent0 ? name0 : name1);
                _windowTitleFormatter.Set("$NameL", name1);
                _windowTitleFormatter.Set("$NameR", name0);

                var bitmapContent0 = Contents[0].Content as BitmapContent;
                var bitmapContent1 = Contents[1].Content as BitmapContent;

                var pictureInfo0 = bitmapContent0?.Picture?.PictureInfo;
                var pictureInfo1 = bitmapContent1?.Picture?.PictureInfo;

                string size0 = pictureInfo0 != null ? $"{pictureInfo0.OriginalSize.Width}×{pictureInfo0.OriginalSize.Height}" : "";
                string size1 = pictureInfo1 != null ? $"{pictureInfo1.OriginalSize.Width}×{pictureInfo1.OriginalSize.Height}" : "";
                _windowTitleFormatter.Set("$Size", isMainContent0 ? size0 : size1);
                _windowTitleFormatter.Set("$SizeL", size1);
                _windowTitleFormatter.Set("$SizeR", size0);

                string bpp0 = pictureInfo0 != null ? size0 + "×" + pictureInfo0.BitsPerPixel.ToString() : "";
                string bpp1 = pictureInfo1 != null ? size1 + "×" + pictureInfo1.BitsPerPixel.ToString() : "";
                _windowTitleFormatter.Set("$SizeEx", isMainContent0 ? bpp0 : bpp1);
                _windowTitleFormatter.Set("$SizeExL", bpp1);
                _windowTitleFormatter.Set("$SizeExR", bpp0);
            }

            if ((mask & WindowTitleMask.View) != 0)
            {
                _windowTitleFormatter.Set("$ViewScale", $"{(int)(_viewScale * 100 + 0.1)}%");
            }

            if ((mask & (WindowTitleMask.Page | WindowTitleMask.View)) != 0)
            {
                var _Dpi = Config.Current.Dpi;

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
        public class Memento
        {
            private string _windowTitleFormat1;
            [DataMember]
            public string WindowTitleFormat1
            {
                get { return _windowTitleFormat1; }
                set { _windowTitleFormat1 = string.IsNullOrEmpty(value) ? WindowTitleFormat1Default : value; }
            }

            private string _windowTitleFormat2;
            [DataMember]
            public string WindowTitleFormat2
            {
                get { return _windowTitleFormat2; }
                set { _windowTitleFormat2 = string.IsNullOrEmpty(value) ? WindowTitleFormat2Default : value; }
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.WindowTitleFormat1 = this.WindowTitleFormat1;
            memento.WindowTitleFormat2 = this.WindowTitleFormat2;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.WindowTitleFormat1 = memento.WindowTitleFormat1;
            this.WindowTitleFormat2 = memento.WindowTitleFormat2;
        }

        #endregion
    }
}
