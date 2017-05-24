using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // ウィンドウタイトル更新項目
    [Flags]
    public enum UpdateWindowTitleMask
    {
        None = 0,
        Book = (1 << 0),
        Page = (1 << 1),
        View = (1 << 2),
        All = 0xFFFF
    }

    //
    public class WindowTitle : BindableBase
    {
        public static WindowTitle Current { get; private set; }

        // 標準ウィンドウタイトル
        private string _defaultWindowTitle;

        // ウィンドウタイトル
        private string _title = "";
        public string Title
        {
            get { return _title; }
            private set { _title = value; RaisePropertyChanged(); }
        }

        private ContentCanvas _contentCanvas;
        private ContentCanvasTransform _contentCanvasTransform;

        public WindowTitle(ContentCanvas contentCanvas, ContentCanvasTransform contentCanvasTransform)
        {
            Current = this;

            _contentCanvas = contentCanvas;
            _contentCanvas.ContentChanged += ContentCanvas_ContentChanged;

            _contentCanvasTransform = contentCanvasTransform;
            _contentCanvasTransform.TransformChanged += ContentCanvasTransform_TransformChanged;

            // Window title
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = FileVersionInfo.GetVersionInfo(assembly.Location);
            _defaultWindowTitle = $"{assembly.GetName().Name} {ver.FileMajorPart}.{ver.FileMinorPart}";
            if (ver.FileBuildPart > 0) _defaultWindowTitle += $".{ver.FileBuildPart}";
#if DEBUG
            _defaultWindowTitle += " [Debug]";
#endif

            BookHub.Current.Loading +=
                (s, e) => this.LoadingPath = e;

            //
            UpdateWindowTitle(UpdateWindowTitleMask.All);
        }

        private void ContentCanvasTransform_TransformChanged(object sender, TransformEventArgs e)
        {
            if (e.ChangeType == TransformChangeType.Scale)
            {
                UpdateWindowTitle(UpdateWindowTitleMask.View);
            }
        }

        //
        private void ContentCanvas_ContentChanged(object sender, EventArgs e)
        {
            UpdateWindowTitle(UpdateWindowTitleMask.All);
        }



        // ウィンドウタイトル更新
        public void UpdateWindowTitle(UpdateWindowTitleMask mask)
        {
            var place = BookOperation.Current.Book?.Place;

            if (LoadingPath != null)
                Title = LoosePath.GetFileName(LoadingPath) + " (読込中)";

            else if (place == null)
                Title = _defaultWindowTitle;

            else if (_contentCanvas.MainContent == null)
                Title = NVUtility.PlaceToTitle(place);

            else
                Title = CreateWindowTitle(mask);
        }

        // ウィンドウタイトル用キーワード置換
        private ReplaceString _windowTitleFormatter = new ReplaceString();

        public const string WindowTitleFormat1Default = "$Book($Page/$PageMax) - $FullName";
        public const string WindowTitleFormat2Default = "$Book($Page/$PageMax) - $FullNameL | $NameR";

        // ウィンドウタイトルフォーマット
        private string _windowTitleFormat1 = WindowTitleFormat1Default;
        public string WindowTitleFormat1
        {
            get { return _windowTitleFormat1; }
            set { _windowTitleFormat1 = value; _windowTitleFormatter.SetFilter(_windowTitleFormat1 + " " + _windowTitleFormat2); }
        }

        private string _windowTitleFormat2 = WindowTitleFormat2Default;
        public string WindowTitleFormat2
        {
            get { return _windowTitleFormat2; }
            set { _windowTitleFormat2 = value; _windowTitleFormatter.SetFilter(_windowTitleFormat1 + " " + _windowTitleFormat2); }
        }

        // ウィンドウタイトル作成
        private string CreateWindowTitle(UpdateWindowTitleMask mask)
        {
            var MainContent = _contentCanvas.MainContent;
            var Contents = _contentCanvas.Contents;
            var _viewScale = _contentCanvasTransform.ViewScale;

            string format = Contents[1].IsValid ? WindowTitleFormat2 : WindowTitleFormat1;

            bool isMainContent0 = MainContent == Contents[0];

            if ((mask & UpdateWindowTitleMask.Book) != 0)
            {
                string bookName = NVUtility.PlaceToTitle(BookOperation.Current.Book?.Place);
                _windowTitleFormatter.Set("$Book", bookName);
            }

            if ((mask & UpdateWindowTitleMask.Page) != 0)
            {
                string pageNum = (MainContent.Source.PartSize == 2)
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

                string size0 = bitmapContent0?.BitmapInfo != null ? $"{bitmapContent0.Size.Width}×{bitmapContent0.Size.Height}" : "";
                string size1 = bitmapContent1?.BitmapInfo != null ? $"{bitmapContent1.Size.Width}×{bitmapContent1.Size.Height}" : "";
                _windowTitleFormatter.Set("$Size", isMainContent0 ? size0 : size1);
                _windowTitleFormatter.Set("$SizeL", size1);
                _windowTitleFormatter.Set("$SizeR", size0);

                string bpp0 = bitmapContent0?.BitmapInfo != null ? size0 + "×" + bitmapContent0.BitmapInfo.BitsPerPixel.ToString() : "";
                string bpp1 = bitmapContent1?.BitmapInfo != null ? size1 + "×" + bitmapContent1.BitmapInfo.BitsPerPixel.ToString() : "";
                _windowTitleFormatter.Set("$SizeEx", isMainContent0 ? bpp0 : bpp1);
                _windowTitleFormatter.Set("$SizeExL", bpp1);
                _windowTitleFormatter.Set("$SizeExR", bpp0);
            }

            if ((mask & UpdateWindowTitleMask.View) != 0)
            {
                _windowTitleFormatter.Set("$ViewScale", $"{(int)(_viewScale * 100 + 0.1)}%");
            }

            if ((mask & (UpdateWindowTitleMask.Page | UpdateWindowTitleMask.View)) != 0)
            {
                var _Dpi = App.Config.Dpi;

                string scale0 = Contents[0].IsValid ? $"{(int)(_viewScale * Contents[0].Scale * _Dpi.DpiScaleX * 100 + 0.1)}%" : "";
                string scale1 = Contents[1].IsValid ? $"{(int)(_viewScale * Contents[1].Scale * _Dpi.DpiScaleX * 100 + 0.1)}%" : "";
                _windowTitleFormatter.Set("$Scale", isMainContent0 ? scale0 : scale1);
                _windowTitleFormatter.Set("$ScaleL", scale1);
                _windowTitleFormatter.Set("$ScaleR", scale0);
            }

            return _windowTitleFormatter.Replace(format);
        }


        // ロード中パス
        // TODO : 定義位置ここか？
        private string _loadingPath;
        public string LoadingPath
        {
            get { return _loadingPath; }
            set { _loadingPath = value; UpdateWindowTitle(UpdateWindowTitleMask.All); }
        }



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
