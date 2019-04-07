using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 画像コンテンツ
    /// </summary>
    public class BitmapContent : PageContent
    {
        private PictureInfo _pictureInfo;


        public BitmapContent(ArchiveEntry entry) : base(entry)
        {
        }


        // picture source
        public PictureSource PictureSource { get; protected set; }

        // picture info
        public virtual PictureInfo PictureInfo => _pictureInfo;

        // picture
        public Picture Picture { get; protected set; }

        // bitmap source
        public BitmapSource BitmapSource => Picture?.BitmapSource;

        // bitmap color
        public Color Color => PictureInfo != null ? PictureInfo.Color : Colors.Black;

        // content size
        public override Size Size => PictureInfo != null ? PictureInfo.Size : SizeExtensions.Zero;

        /// <summary>
        /// BitmapSourceがあればコンテンツ有効
        /// </summary>
        public override bool IsLoaded => Picture != null || PageMessage != null;

        public override bool IsViewReady => BitmapSource != null || PageMessage != null;

        public override bool CanResize => true;


        public virtual Size GetRenderSize(Size size)
        {
            return CanResize && PictureProfile.Current.IsResizeFilterEnabled ? size : Size.Empty;
        }

        /// <summary>
        /// 使用メモリサイズ (Picture)
        /// </summary>
        public override long GetContentMemorySize()
        {
            var pictre = Picture;
            return pictre != null ? pictre.GetMemorySize() : 0;
        }

        /// <summary>
        /// 使用メモリサイズ (PictureSource)
        /// </summary>
        public override long GetPictureSourceMemorySize()
        {
            var source = PictureSource;
            return source != null ? source.GetMemorySize() : 0;
        }


        public void SetPictureInfo(PictureInfo pictureInfo)
        {
            _pictureInfo = pictureInfo;
        }

        public void SetPictureSource(PictureSource pictureSource)
        {
            PictureSource = pictureSource;
        }

        public void SetPicture(Picture picture)
        {
            Picture = picture;
        }

        public override IContentLoader CreateContentLoader()
        {
            return new BitmapContentLoader(this);
        }
    }

}
