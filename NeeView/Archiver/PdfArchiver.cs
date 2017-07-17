// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        #region Fields

        private bool _isDisposed;

        #endregion

        #region Constructors

        // コンストラクタ
        public PdfArchiver(string path, ArchiveEntry source) : base(path, source)
        {
        }

        #endregion

        #region Properties

        //
        public override bool IsDisposed => _isDisposed;

        #endregion

        #region Methods

        public override string ToString()
        {
            return "Pdfium";
        }

        // Dispose
        public override void Dispose()
        {
            _isDisposed = true;
            base.Dispose();
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries(CancellationToken token)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

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
                        EntryName = $"{id + 1:000}.png",
                        Length = 0,
                        LastWriteTime = information.ModificationDate,
                    });
                }
            }

            return list;
        }

        // エントリーのストリームを得る
        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            using (var pdfDocument = PdfDocument.Load(Path))
            {
                var size = GetRenderSize(pdfDocument, entry.Id);
                var image = pdfDocument.Render(entry.Id, (int)size.Width, (int)size.Height, 96, 96, false);

                var ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
        }

        // 規定内に収まるサイズ取得
        private Size GetRenderSize(PdfDocument pdfDocument, int page)
        {
            return GetRenderSize(pdfDocument, page, new Size(PdfArchiverProfile.Current.RenderSize.Width, PdfArchiverProfile.Current.RenderSize.Height));
        }

        // 指定サイズ内に収まるサイズ取得
        private Size GetRenderSize(PdfDocument pdfDocument, int page, Size size)
        {
            var documentSize = pdfDocument.PageSizes[page];

            var rateX = size.Width / documentSize.Width;
            var rateY = size.Height / documentSize.Height;
            var rate = Math.Min(rateX, rateY);

            return new Size(documentSize.Width * rate, documentSize.Height * rate);
        }

        // ファイルとして出力
        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            using (var pdfDocument = PdfDocument.Load(Path))
            {
                var size = GetRenderSize(pdfDocument, entry.Id);
                var image = pdfDocument.Render(entry.Id, (int)size.Width, (int)size.Height, 96, 96, false);

                image.Save(exportFileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        
        // サイズを指定して画像を取得する
        public BitmapSource CraeteBitmapSource(ArchiveEntry entry, Size maxSize)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            using (var pdfDocument = PdfDocument.Load(Path))
            {
                var size = GetRenderSize(pdfDocument, entry.Id, maxSize);
                var image = pdfDocument.Render(entry.Id, (int)size.Width, (int)size.Height, 96, 96, false);
                return Utility.NVGraphics.ToBitmapSource(image);
            }
        }

        #endregion
    }
}
