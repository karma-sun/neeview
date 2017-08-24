// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using PhotoSauce.MagicScaler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    [Flags]
    public enum PictureCreateOptions
    {
        None,
        CreateBitmap,
        CreateThumbnail,
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
        Picture Create(ArchiveEntry entry, PictureCreateOptions options);

        BitmapSource CreateBitmapSource(ArchiveEntry entry, byte[] raw, Size size);
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
            catch (Exception)
            {
                throw;
            }
        }

        //
        public Picture Create(ArchiveEntry entry, PictureCreateOptions options)
        {
            return RetryWhenOutOfMemory(
                () =>
                {
                    if (entry.Archiver is PdfArchiver)
                    {
                        return _pdfFactory.Create(entry, options);
                    }
                    else
                    {
                        return _defaultFactory.Create(entry, options);
                    }
                });
        }

        //
        public BitmapSource CreateBitmapSource(ArchiveEntry entry, byte[] raw, Size size)
        {
            ////Debug.WriteLine($"Create: {entry.EntryLastName} ({size.Truncate()})");

            return RetryWhenOutOfMemory(
                () =>
                {
                    if (entry.Archiver is PdfArchiver)
                    {
                        return _pdfFactory.CreateBitmapSource(entry, raw, size);
                    }
                    else
                    {
                        return _defaultFactory.CreateBitmapSource(entry, raw, size);
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
