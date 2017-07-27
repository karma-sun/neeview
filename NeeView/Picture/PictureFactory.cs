// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    /// <summary>
    /// Picture Factory interface.
    /// </summary>
    public interface IPictureFactory
    {
        Picture Create(ArchiveEntry entry);
        BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size);
        Size CreateFixedSize(ArchiveEntry entry, Size size);

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
            catch(Exception)
            {
                throw;
            }
        }

        //
        public Picture Create(ArchiveEntry entry)
        {
            return RetryWhenOutOfMemory(
                () =>
                {
                    if (entry.Archiver is PdfArchiver)
                    {
                        return _pdfFactory.Create(entry);
                    }
                    else
                    {
                        return _defaultFactory.Create(entry);
                    }
                });
        }

        //
        public BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size)
        {
            Debug.WriteLine($"Create: {entry.EntryLastName} ({size.Truncate()})");

            return RetryWhenOutOfMemory(
                () =>
                {
                    if (entry.Archiver is PdfArchiver)
                    {
                        return _pdfFactory.CreateBitmapSource(entry, size);
                    }
                    else
                    {
                        return _defaultFactory.CreateBitmapSource(entry, size);
                    }
                });
        }

        public Size CreateFixedSize(ArchiveEntry entry, Size size)
        {
            if (entry.Archiver is PdfArchiver)
            {
                return _pdfFactory.CreateFixedSize(entry, size);
            }
            else
            {
                return _defaultFactory.CreateFixedSize(entry, size);
            }
        }
    }
}
