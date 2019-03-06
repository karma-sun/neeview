using PhotoSauce.MagicScaler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Picture生成オプション
    /// </summary>
    [Flags]
    public enum PictureCreateOptions
    {
        None = 0x0000,
        CreateBitmap = 0x0001,
        CreateThumbnail = 0x0002,
        IgnoreImageCache = 0x0004,
    }

    /// <summary>
    /// Bitmap生成パラメータ
    /// </summary>
    public class BitmapCreateSetting
    {
        /// <summary>
        /// Bitmap生成モード
        /// </summary>
        public BitmapCreateMode Mode { get; set; }

        /// <summary>
        /// リサイズパラメータ
        /// </summary>
        public ProcessImageSettings ProcessImageSettings { get; set; }

        /// <summary>
        /// 生成元として使用可能なBitmap。
        /// 指定されない場合もある
        /// </summary>
        public BitmapSource Source { get; set; }
    }


    /// <summary>
    /// Picture Factory
    /// </summary>
    public class PictureFactory : IPictureFactory
    {
        private static PictureFactory _current;
        public static PictureFactory Current = _current = _current ?? new PictureFactory();

        private DefaultPictureFactory _defaultFactory = new DefaultPictureFactory();
        private PdfPictureFactory _pdfFactory = new PdfPictureFactory();

        /// <summary>
        /// OutOfMemory時にはリトライする処理(async)
        /// </summary>
        private async Task<TResult> RetryWhenOutOfMemoryAsync<TResult>(Task<TResult> task)
        {
            int retry = 0;
            RETRY:

            try
            {
                return await task;
            }
            catch (OutOfMemoryException) when (retry == 0)
            {
                Debug.WriteLine("Retry...");
                retry++;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                goto RETRY;
            }
        }

        /// <summary>
        /// OutOfMemory時にはリトライする処理
        /// </summary>
        private TResult RetryWhenOutOfMemory<TResult>(Func<TResult> func)
        {
            int retry = 0;
            RETRY:

            try
            {
                return func();
            }
            catch (OutOfMemoryException) when (retry == 0)
            {
                Debug.WriteLine("Retry...");
                retry++;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                goto RETRY;
            }
        }

        /// <summary>
        /// Picture作成
        /// </summary>
        /// <param name="entry">ソースとなるエントリ</param>
        /// <param name="options">Picture生成オプション</param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>キャンセルされたときには null を返す</returns>
        public async Task<Picture> CreateAsync(ArchiveEntry entry, PictureCreateOptions options, CancellationToken token)
        {
            if (entry.Archiver is PdfArchiver)
            {
                return await RetryWhenOutOfMemoryAsync(_pdfFactory.CreateAsync(entry, options, token));
            }
            else
            {
                return await RetryWhenOutOfMemoryAsync(_defaultFactory.CreateAsync(entry, options, token));
            }
        }

        /// <summary>
        /// 画像データからBitmapSource生成
        /// </summary>
        /// <param name="entry">ソースとなるエントリ</param>
        /// <param name="raw">ソース画像データ</param>
        /// <param name="size">指定サイズ</param>
        /// <param name="keepAspectRatio">アスペクト比固定？</param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>生成したBitmapSource。キャンセルされたときにはnullを返す</returns>
        public BitmapSource CreateBitmapSource(ArchiveEntry entry, byte[] raw, Size size, bool keepAspectRatio, CancellationToken token)
        {
            ////Debug.WriteLine($"Create: {entry.EntryLastName} ({size.Truncate()})");
            if (entry.Archiver is PdfArchiver)
            {
                return RetryWhenOutOfMemory(() => _pdfFactory.CreateBitmapSource(entry, raw, size, keepAspectRatio, token));
            }
            else
            {
                return RetryWhenOutOfMemory(() => _defaultFactory.CreateBitmapSource(entry, raw, size, keepAspectRatio, token));
            }
        }


        /// <summary>
        /// 画像ファイルデータから指定サイズの画像ファイルを作成
        /// </summary>
        /// <param name="entry">ソースのエントリ</param>
        /// <param name="raw">ソースとなる画像ファイルデータ</param>
        /// <param name="size">指定サイズ</param>
        /// <param name="format">出力画像フォーマット</param>
        /// <param name="quality">出力画像の品質。JPEG用</param>
        /// <param name="setting">画像生成設定</param>
        /// <returns>画像ファイルデータ</returns>
        public byte[] CreateImage(ArchiveEntry entry, byte[] raw, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting)
        {
            ////Debug.WriteLine($"CreateThumnbnail: {entry.EntryLastName} ({size.Truncate()})");
            if (entry.Archiver is PdfArchiver)
            {
                return RetryWhenOutOfMemory(() => _pdfFactory.CreateImage(entry, raw, size, format, quality, setting));
            }
            else
            {
                return RetryWhenOutOfMemory(() => _defaultFactory.CreateImage(entry, raw, size, format, quality, setting));
            }
        }

        /// <summary>
        /// 画像ファイルデータから指定サイズの画像ファイルを作成。サムネイル用
        /// </summary>
        /// <param name="entry">ソースのエントリ</param>
        /// <param name="raw">ソースとなる画像ファイルデータ</param>
        /// <param name="size">指定サイズ</param>
        /// <param name="source">入力ビットマップ。null出ない場合rawでなくこちらをソースとする</param>
        /// <returns>画像ファイルデータ</returns>
        public byte[] CreateThumbnail(ArchiveEntry entry, byte[] raw, Size size, BitmapSource source)
        {
            var createSetting = ThumbnailProfile.Current.CreateBitmapCreateSetting();
            createSetting.Source = source;

            return CreateImage(entry, raw, size, ThumbnailProfile.Current.Format, ThumbnailProfile.Current.Quality, createSetting);
        }
    }
}
