using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// MediaPlayer コンテンツ
    /// </summary>
    public class MediaContent : BitmapContent
    {
        public MediaContent(ArchiveEntry entry) : base(entry)
        {
            PictureInfo = new PictureInfo(entry);
        }

        public override bool IsLoaded => FileProxy != null;

        public override bool IsViewReady => IsLoaded;

        public override bool CanResize => false;

        public override PictureInfo PictureInfo { get; }

        public bool IsLastStart { get; set; }

        /// <summary>
        /// サイズ設定
        /// </summary>
        public void SetSize(Size size)
        {
            this.PictureInfo.Size = size;
            this.PictureInfo.OriginalSize = size;
            this.PictureInfo.BitsPerPixel = 32;
        }

        public override IContentLoader CreateContentLoader()
        {
            return new MediaContentLoader(this);
        }
    }
}
