using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Pictureの元データ管理
    /// </summary>
    public abstract class PictureSource
    {
        protected PictureSourceCreateOptions _createOptions;

        public PictureSource(ArchiveEntry entry, PictureSourceCreateOptions createOptions)
        {
            ArchiveEntry = entry;
            _createOptions = createOptions;
        }

        public ArchiveEntry ArchiveEntry { get; }
        public PictureInfo PictureInfo { get; protected set; }

        /// <summary>
        /// メモリ使用量取得
        /// </summary>
        public virtual long GetMemorySize() => 0;

        /// <summary>
        /// PictureInfo初期化
        /// </summary>
        public abstract void InitializePictureInfo(CancellationToken token);

        /// <summary>
        /// BitmaSource作成。メインコンテンツ用
        /// </summary>
        public abstract BitmapSource CreateBitmapSource(Size size, BitmapCreateSetting setting, CancellationToken token);

        /// <summary>
        /// 画像データ作成
        /// </summary>
        public abstract byte[] CreateImage(Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token);

        /// <summary>
        /// サムネイル画像データ作成
        /// </summary>
        public abstract byte[] CreateThumbnail(ThumbnailProfile profile, CancellationToken token);
    }

    [Flags]
    public enum PictureSourceCreateOptions
    {
        None = 0,
        ////IgnoreImageCache = 0x0001,
    }


    public static class PictureSourceFactory
    {
        public static PictureSource Create(ArchiveEntry entry, PictureSourceCreateOptions createOptions, CancellationToken token)
        {
            if (entry.Archiver is PdfArchiver)
            {
                return new PdfPictureSource(entry, createOptions);
            }
            else
            {
                return new DefaultPictureSource(entry, createOptions);
            }
        }
    }

}
