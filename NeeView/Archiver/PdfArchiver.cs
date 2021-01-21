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
    /// </summary>
    public class PdfArchiver : Archiver
    {
        #region Constructors

        public PdfArchiver(string path, ArchiveEntry source) : base(path, source)
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
        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            var list = new List<ArchiveEntry>();

            // TODO: ウィンドウが非アクティブになるまではインスタンスを持ちづつけるようにする？要速度調査

            using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var pdfDocument = PdfDocument.Load(stream))
            {
                var information = pdfDocument.GetInformation();

                for (int id = 0; id < pdfDocument.PageCount; ++id)
                {
                    token.ThrowIfCancellationRequested();

                    list.Add(new ArchiveEntry()
                    {
                        IsValid = true,
                        Archiver = this,
                        Id = id,
                        Instance = null,
                        RawEntryName = $"{id + 1:000}.png",
                        Length = 0,
                        LastWriteTime = information.ModificationDate ?? default
                    });
                }
            }

            await Task.CompletedTask;
            return list;
        }

        // エントリーのストリームを得る
        // PDFは画像化したものをストリームにして返す
        protected override Stream OpenStreamInner(ArchiveEntry entry)
        {
            using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var pdfDocument = PdfDocument.Load(stream))
            {
                var size = GetRenderSize(pdfDocument, entry.Id);
                var image = pdfDocument.Render(entry.Id, (int)size.Width, (int)size.Height, 96, 96, false);

                var ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
        }

        // サイズ取得
        public Size GetSourceSize(ArchiveEntry entry)
        {
            using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var pdfDocument = PdfDocument.Load(stream))
            {
                return GetSourceSize(pdfDocument, entry.Id);
            }
        }

        // サイズ取得
        private Size GetSourceSize(PdfDocument pdfDocument, int page)
        {
            return SizeExtensions.FromDrawingSize(pdfDocument.PageSizes[page]);
        }

        // 標準サイズで取得
        public Size GetRenderSize(ArchiveEntry entry)
        {
            using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var pdfDocument = PdfDocument.Load(stream))
            {
                return GetRenderSize(pdfDocument, entry.Id);
            }
        }

        // 標準サイズで取得
        private Size GetRenderSize(PdfDocument pdfDocument, int page)
        {
            var size = SizeExtensions.FromDrawingSize(pdfDocument.PageSizes[page]);
            if (PdfArchiverProfile.Current.SizeLimitedRenderSize.IsContains(size))
            {
                size = size.Uniformed(PdfArchiverProfile.Current.SizeLimitedRenderSize);
            }
            return size;
        }


        // ファイルとして出力
        protected override void ExtractToFileInner(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var pdfDocument = PdfDocument.Load(stream))
            {
                var size = GetRenderSize(pdfDocument, entry.Id);
                var image = pdfDocument.Render(entry.Id, (int)size.Width, (int)size.Height, 96, 96, false);

                image.Save(exportFileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        // サイズを指定して画像を取得する
        public System.Drawing.Image CraeteBitmapSource(ArchiveEntry entry, Size size)
        {
            using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var pdfDocument = PdfDocument.Load(stream))
            {
                return pdfDocument.Render(entry.Id, (int)size.Width, (int)size.Height, 96, 96, false);
            }
        }

        #endregion
    }
}
