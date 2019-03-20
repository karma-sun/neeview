using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 画像。
    /// エントリに対応する表示画像、サムネイル画像を管理する。
    /// PictureFactoryで生成される。
    /// </summary>
    public class Picture : BindableBase
    {
        #region Fields

        /// <summary>
        /// ソースのエントリ
        /// </summary>
        private ArchiveEntry _archiveEntry;

        /// <summary>
        /// リサイズパラメータのハッシュ。
        /// リサイズが必要かの判定に使用される
        /// </summary>
        private int _resizeHashCode;

        /// <summary>
        /// このPictureが使用されなくなったときのキャンセル通知
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private object _lock = new object();

        #endregion

        #region Constructors

        public Picture(ArchiveEntry entry)
        {
            _archiveEntry = entry;
            _resizeHashCode = GetEnvironmentoHashCode();
            _cancellationTokenSource = new CancellationTokenSource();

            this.PictureInfo = new PictureInfo(entry);
        }

        #endregion

        #region Properties

        /// <summary>
        /// 画像情報
        /// </summary>
        public PictureInfo PictureInfo { get; set; }

        /// <summary>
        /// ソースとなる 画像ファイルデータ。
        /// エントリからの呼び出し負荷を軽減するためのキャッシュ
        /// </summary>
        public byte[] RawData { get; set; }

        /// <summary>
        /// 表示する画像
        /// </summary>
        private BitmapSource _bitmapSource;
        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
            set { if (_bitmapSource != value) { _bitmapSource = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// サムネイル画像
        /// </summary>
        private byte[] _thumbnail;
        public byte[] Thumbnail
        {
            get { return _thumbnail; }
            set { if (_thumbnail != value) { _thumbnail = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Methods

        /// <summary>
        /// このPictureの使用停止
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

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

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return false;
            }

            try
            {
                var bitmap = PictureFactory.Current.CreateBitmapSource(_archiveEntry, this.RawData, size, keepAspectRatio, _cancellationTokenSource.Token);
                if (bitmap == null)
                {
                    return false;
                }

                lock (_lock)
                {
                    _resizeHashCode = filterHashCode;
                    this.BitmapSource = bitmap;
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }

            return true;
        }

        // サムネイル生成
        // TODO: メインコンテンツの状態に依存しないように
        public byte[] CreateThumbnail(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (this.Thumbnail != null)
            {
                return this.Thumbnail;
            }

            if (this.BitmapSource == null) 
            {
                Debug.WriteLine("Warning!: It's wrong operation");
                return null;
            }

            var thumbnailSize = ThumbnailProfile.Current.GetThumbnailSize(this.PictureInfo.Size);
            this.Thumbnail = PictureFactory.Current.CreateThumbnail(_archiveEntry, this.RawData, thumbnailSize, this.BitmapSource, token);

            return this.Thumbnail;
        }

        #endregion
    }

}
