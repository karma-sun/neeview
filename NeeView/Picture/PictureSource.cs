using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Pictureの元データ管理
    /// </summary>
    public abstract class PictureSource
    {
        protected PictureSourceCreateOptions _createOptions;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entry">エントリ</param>
        /// <param name="pictureInfo">画像情報。nullの場合は元データから新しく生成される。</param>
        /// <param name="createOptions"></param>
        public PictureSource(ArchiveEntry entry, PictureInfo pictureInfo, PictureSourceCreateOptions createOptions)
        {
            ArchiveEntry = entry;
            PictureInfo = pictureInfo;
            _createOptions = createOptions;
        }

        public ArchiveEntry ArchiveEntry { get; }
        public PictureInfo PictureInfo { get; protected set; }

        /// <summary>
        /// メモリ使用量取得
        /// </summary>
        public virtual long GetMemorySize() => 0;

        /// <summary>
        /// PictureInfo作成
        /// </summary>
        public abstract PictureInfo CreatePictureInfo(CancellationToken token);

        /// <summary>
        /// ImageSource作成。メインコンテンツ用
        /// </summary>
        public abstract ImageSource CreateImageSource(Size size, BitmapCreateSetting setting, CancellationToken token);

        /// <summary>
        /// 画像データ作成
        /// </summary>
        public abstract byte[] CreateImage(Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token);

        /// <summary>
        /// サムネイル画像データ作成
        /// </summary>
        public abstract byte[] CreateThumbnail(ThumbnailProfile profile, CancellationToken token);

        /// <summary>
        /// サイズ補正。各プロファイルに即してサイズを制限する
        /// </summary>
        public abstract Size FixedSize(Size size);
    }

    [Flags]
    public enum PictureSourceCreateOptions
    {
        None = 0,
        ////IgnoreImageCache = 0x0001,
        IgnoreCompress = 0x0002,
    }


    public static class PictureSourceFactory
    {
        public static PictureSource Create(ArchiveEntry entry, PictureInfo pictureInfo, PictureSourceCreateOptions createOptions, CancellationToken token)
        {
            if (entry.Archiver is PdfArchiver)
            {
                return new PdfPictureSource(entry, pictureInfo, createOptions);
            }
            else
            {
                return new DefaultPictureSource(entry, pictureInfo, createOptions);
            }
        }
    }

}
