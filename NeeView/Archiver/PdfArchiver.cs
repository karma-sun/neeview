using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PdfiumViewer;
using System.Windows;
using System.Runtime.Serialization;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：PdfiumViewer によるPDFアーカイバ
    /// エントリーはPNG化したストリームを渡している
    /// </summary>
    public class PdfArchiver : Archiver
    {
        #region Constructors

        public PdfArchiver(string path, ArchiveEntry source, bool isRoot) : base(path, source, isRoot)
        {
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return "Pdfium";
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries(CancellationToken token)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            var list = new List<ArchiveEntry>();

            using (var pdfDocument = PdfDocument.Load(Path))
            {
                var information = pdfDocument.GetInformation();

                for (int id = 0; id < pdfDocument.PageCount; ++id)
                {
                    token.ThrowIfCancellationRequested();

                    list.Add(new ArchiveEntry()
                    {
                        Archiver = this,
                        Id = id,
                        Instance = null,
                        RawEntryName = $"{id + 1:000}.png",
                        Length = 0,
                        LastWriteTime = information.ModificationDate ?? default
                    });
                }
            }

            return list;
        }

        // エントリーのストリームを得る
        // PDFは画像化したものをストリームにして返す
        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            using (var pdfDocument = PdfDocument.Load(Path))
            {
                var size = GetRenderSize(pdfDocument, entry.Id);
                var image = pdfDocument.Render(entry.Id, (int)size.Width, (int)size.Height, 96, 96, false);

                var ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
        }

        // 標準サイズで取得
        public Size GetRenderSize(ArchiveEntry entry)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            using (var pdfDocument = PdfDocument.Load(Path))
            {
                return GetRenderSize(pdfDocument, entry.Id);
            }
        }

        // 標準サイズで取得
        private Size GetRenderSize(PdfDocument pdfDocument, int page)
        {
            var size = SizeExtensions.FromDrawingSize(pdfDocument.PageSizes[page]);
            return size.Uniformed(PdfArchiverProfile.Current.SizeLimitedRenderSize);
        }


        // ファイルとして出力
        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            using (var pdfDocument = PdfDocument.Load(Path))
            {
                var size = GetRenderSize(pdfDocument, entry.Id);
                var image = pdfDocument.Render(entry.Id, (int)size.Width, (int)size.Height, 96, 96, false);

                image.Save(exportFileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        // サイズを指定して画像を取得する
        public System.Drawing.Image CraeteBitmapSource(ArchiveEntry entry, Size size)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            using (var pdfDocument = PdfDocument.Load(Path))
            {
                return pdfDocument.Render(entry.Id, (int)size.Width, (int)size.Height, 96, 96, false);
            }
        }

        #endregion
    }
}
