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
    [Flags]
    public enum PictureCreateOptions
    {
        None = 0x0000,
        CreateBitmap = 0x0001,
        CreateThumbnail = 0x0002,
    }

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
    /// Picture Factory interface.
    /// </summary>
    public interface IPictureFactory
    {
        Task<Picture> CreateAsync(ArchiveEntry entry, PictureCreateOptions options, CancellationToken token);

        BitmapSource CreateBitmapSource(ArchiveEntry entry, byte[] raw, Size size, bool keepAspectRatio, CancellationToken token);
        byte[] CreateImage(ArchiveEntry entry, byte[] raw, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting);
    }


    /// <summary>
    /// Picture Factory
    /// </summary>
    public class PictureFactory : IPictureFactory
    {
        //
        private static PictureFactory _current;
        public static PictureFactory Current = _current = _current ?? new PictureFactory();

        //
        private DefaultPictureFactory _defaultFactory = new DefaultPictureFactory();

        private PdfPictureFactory _pdfFactory = new PdfPictureFactory();


        //
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

        //
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

        //
        public BitmapSource CreateBitmapSource(ArchiveEntry entry, byte[] raw, Size size, bool keepAspectRatio, CancellationToken token)
        {
            ////Debug.WriteLine($"Create: {entry.EntryLastName} ({size.Truncate()})");

            return RetryWhenOutOfMemory(
            () =>
            {
                if (entry.Archiver is PdfArchiver)
                {
                    return _pdfFactory.CreateBitmapSource(entry, raw, size, keepAspectRatio, token);
                }
                else
                {
                    return _defaultFactory.CreateBitmapSource(entry, raw, size, keepAspectRatio, token);
                }
            });
        }


        //
        public byte[] CreateImage(ArchiveEntry entry, byte[] raw, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting)
        {
            ////Debug.WriteLine($"CreateThumnbnail: {entry.EntryLastName} ({size.Truncate()})");

            return RetryWhenOutOfMemory(
                () =>
                {
                    if (entry.Archiver is PdfArchiver)
                    {
                        return _pdfFactory.CreateImage(entry, raw, size, format, quality, setting);
                    }
                    else
                    {
                        return _defaultFactory.CreateImage(entry, raw, size, format, quality, setting);
                    }
                });
        }

        //
        public byte[] CreateThumbnail(ArchiveEntry entry, byte[] raw, Size size, BitmapSource source)
        {
            var createSetting = ThumbnailProfile.Current.CreateBitmapCreateSetting();
            createSetting.Source = source;

            return CreateImage(entry, raw, size, ThumbnailProfile.Current.Format, ThumbnailProfile.Current.Quality, createSetting);
        }
    }
}
