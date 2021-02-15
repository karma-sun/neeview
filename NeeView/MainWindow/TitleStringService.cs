using System;
using System.Collections.Generic;
using NeeView.Text;

namespace NeeView
{
    public class TitleStringService
    {
        private static TitleStringService _default;
        public static TitleStringService Default => _default = _default ?? new TitleStringService(MainViewComponent.Current);


        private ReplaceString _replaceString = new ReplaceString();
        private MainViewComponent _mainViewComponent;
        private int _changedCount;


        public TitleStringService(MainViewComponent mainViewComponent)
        {
            _mainViewComponent = mainViewComponent;

            _mainViewComponent.ContentCanvas.ContentChanged += (s, e) =>
            {
                Update();
            };

            _mainViewComponent.DragTransform.AddPropertyChanged(nameof(DragTransform.Scale), (s, e) =>
            {
                Update();
            });

            _replaceString.Changed += ReplaceString_Changed;
        }


        public event EventHandler Changed;


        private void ReplaceString_Changed(object sender, ReplaceStringChangedEventArgs e)
        {
            _changedCount++;
        }

        public string Replace(string src, IEnumerable<string> keys)
        {
            return _replaceString.Replace(src, keys);
        }

        private void Update()
        {
            _changedCount = 0;

            var contents = _mainViewComponent.ContentCanvas.CloneContents;
            var mainContent = _mainViewComponent.ContentCanvas.MainContent;
            var viewScale = _mainViewComponent.DragTransform.Scale;

            bool isMainContent0 = mainContent == contents[0];

            // book
            string bookName = LoosePath.GetDispName(BookOperation.Current.Book?.Address);
            _replaceString.Set("$Book", bookName);

            // page
            _replaceString.Set("$PageMax", (BookOperation.Current.GetMaxPageIndex() + 1).ToString());

            string pageNum0 = GetPageNum(contents[0]);
            string pageNum1 = GetPageNum(contents[1]);
            _replaceString.Set("$Page", isMainContent0 ? pageNum0 : pageNum1);
            _replaceString.Set("$PageL", pageNum1);
            _replaceString.Set("$PageR", pageNum0);

            string GetPageNum(ViewContent content)
            {
                return content.IsValid ? (content.Source.PagePart.PartSize == 2) ? (content.Position.Index + 1).ToString() : (content.Position.Index + 1).ToString() + (content.Position.Part == 1 ? ".5" : ".0") : "";
            }

            string path0 = GetFullName(contents[0]);
            string path1 = GetFullName(contents[1]);
            _replaceString.Set("$FullName", isMainContent0 ? path0 : path1);
            _replaceString.Set("$FullNameL", path1);
            _replaceString.Set("$FullNameR", path0);

            string GetFullName(ViewContent content)
            {
                return content.IsValid ? content.FullPath.Replace("/", " > ").Replace("\\", " > ") + content.GetPartString() : "";
            }

            string name0 = GetName(contents[0]);
            string name1 = GetName(contents[1]);
            _replaceString.Set("$Name", isMainContent0 ? name0 : name1);
            _replaceString.Set("$NameL", name1);
            _replaceString.Set("$NameR", name0);

            string GetName(ViewContent content)
            {
                return content.IsValid ? LoosePath.GetFileName(content.FullPath) + content.GetPartString() : "";
            }

            var bitmapContent0 = contents[0].Content as BitmapContent;
            var bitmapContent1 = contents[1].Content as BitmapContent;
            var pictureInfo0 = bitmapContent0?.PictureInfo;
            var pictureInfo1 = bitmapContent1?.PictureInfo;
            string bpp0 = GetSizeEx(pictureInfo0);
            string bpp1 = GetSizeEx(pictureInfo1);
            _replaceString.Set("$SizeEx", isMainContent0 ? bpp0 : bpp1);
            _replaceString.Set("$SizeExL", bpp1);
            _replaceString.Set("$SizeExR", bpp0);

            string GetSizeEx(PictureInfo pictureInfo)
            {
                return pictureInfo != null ? GetSize(pictureInfo) + "×" + pictureInfo.BitsPerPixel.ToString() : "";
            }

            string size0 = GetSize(pictureInfo0);
            string size1 = GetSize(pictureInfo1);
            _replaceString.Set("$Size", isMainContent0 ? size0 : size1);
            _replaceString.Set("$SizeL", size1);
            _replaceString.Set("$SizeR", size0);

            string GetSize(PictureInfo pictureInfo)
            {
                return pictureInfo != null ? $"{pictureInfo.OriginalSize.Width}×{pictureInfo.OriginalSize.Height}" : "";
            }

            // view scale
            _replaceString.Set("$ViewScale", $"{(int)(viewScale * 100 + 0.1)}%");

            // scale
            var _Dpi = _mainViewComponent.ContentCanvas.Dpi;
            string scale0 = contents[0].IsValid ? $"{(int)(viewScale * contents[0].Scale * _Dpi.DpiScaleX * 100 + 0.1)}%" : "";
            string scale1 = contents[1].IsValid ? $"{(int)(viewScale * contents[1].Scale * _Dpi.DpiScaleX * 100 + 0.1)}%" : "";
            _replaceString.Set("$Scale", isMainContent0 ? scale0 : scale1);
            _replaceString.Set("$ScaleL", scale1);
            _replaceString.Set("$ScaleR", scale0);

            if (_changedCount > 0)
            {
                Changed?.Invoke(this, null);
            }
        }
    }

}
