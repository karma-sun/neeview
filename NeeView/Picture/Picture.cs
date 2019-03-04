using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    //
    public class Picture : BindableBase
    {
        #region Fields

        private ArchiveEntry _archiveEntry;

        private int _resizeHashCode;

        private object _lock = new object();

        #endregion

        #region Constructors

        //
        public Picture(ArchiveEntry entry)
        {
            _archiveEntry = entry;
            _resizeHashCode = GetEnvironmentoHashCode();

            this.PictureInfo = new PictureInfo(entry);
        }

        #endregion

        #region Properties

        //
        public PictureInfo PictureInfo { get; set; }

        //
        public byte[] RawData { get; set; }

        /// <summary>
        /// BitmapSource property.
        /// </summary>
        private BitmapSource _bitmapSource;
        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
            set { if (_bitmapSource != value) { _bitmapSource = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Thumbnail property.
        /// </summary>
        private byte[] _thumbnail;
        public byte[] Thumbnail
        {
            get { return _thumbnail; }
            set { if (_thumbnail != value) { _thumbnail = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Methods

        // 画像生成に影響する設定のハッシュ値取得
        private int GetEnvironmentoHashCode()
        {
            return ImageFilter.Current.GetHashCode() ^ PictureProfile.Current.CustomSize.GetHashCodde();
        }

        // Bitmapが同じサイズであるか判定
        private bool IsEqualBitmapSizeMaybe(Size size, bool keepAspectRatio)
        {
            if (this.BitmapSource == null) return false;

            size = size.IsEmpty ? this.PictureInfo.Size : size;

            const double margin = 1.1;
            if (keepAspectRatio)
            {
                // アスペクト比固定のため、PixelHeightのみで判定
                return Math.Abs(size.Height - this.BitmapSource.PixelHeight) < margin;
            }
            else
            {
                return Math.Abs(size.Height - this.BitmapSource.PixelHeight) < margin && Math.Abs(size.Width - this.BitmapSource.PixelWidth) < margin;
            }
        }

        // リサイズ
        public bool Resize(Size size)
        {
            if (this.BitmapSource == null) return false;

            size = size.IsEmpty ? this.PictureInfo.Size : size;

            if (_archiveEntry.Archiver is PdfArchiver)
            {
                size = PdfArchiverProfile.Current.CreateFixedSize(size);
            }
            else
            {
                var maxWixth = Math.Max(this.PictureInfo.Size.Width, PictureProfile.Current.MaximumSize.Width);
                var maxHeight = Math.Max(this.PictureInfo.Size.Height, PictureProfile.Current.MaximumSize.Height);
                var maxSize = new Size(maxWixth, maxHeight);
                size = size.Limit(maxSize);
            }

            // 規定サイズ判定
            if (!this.PictureInfo.IsLimited && size.IsEqualMaybe(this.PictureInfo.Size))
            {
                size = Size.Empty;
            }

            // アスペクト比固定?
            var cutomSize = PictureProfile.Current.CustomSize;
            var keepAspectRatio = size.IsEmpty || !cutomSize.IsEnabled || cutomSize.IsUniformed;

            int filterHashCode = GetEnvironmentoHashCode();
            bool isDartyResizeParameter = _resizeHashCode != filterHashCode;
            if (!isDartyResizeParameter && IsEqualBitmapSizeMaybe(size, keepAspectRatio)) return false;

            ////var nowSize = new Size(this.BitmapSource.PixelWidth, this.BitmapSource.PixelHeight);
            ////Debug.WriteLine($"Resize: {isDartyResizeParameter}: {nowSize.Truncate()} -> {size.Truncate()}");

            var bitmap = PictureFactory.Current.CreateBitmapSource(_archiveEntry, this.RawData, size, keepAspectRatio, CancellationToken.None);

            lock (_lock)
            {
                _resizeHashCode = filterHashCode;
                this.BitmapSource = bitmap;
            }

            return true;
        }

        // サムネイル生成
        public byte[] CreateThumbnail()
        {
            if (this.Thumbnail != null) return this.Thumbnail;

            if (this.BitmapSource == null) return null;

            var thumbnailSize = ThumbnailProfile.Current.GetThumbnailSize(this.PictureInfo.Size);
            this.Thumbnail = PictureFactory.Current.CreateThumbnail(_archiveEntry, this.RawData, thumbnailSize, this.BitmapSource);

            return this.Thumbnail;
        }

        #endregion
    }

}
