// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 画像ページ
    /// </summary>
    public class BitmapPage : Page
    {
        // アーカイバ
        private Archiver _Archiver;

        // GIFアニメーション用に展開したテンポラリファイル
        private Uri _GifFileUri;


        // ToString
        public override string ToString()
        {
            return LoosePath.GetFileName(FileName);
        }

        // コンストラクタ
        public BitmapPage(Archiver archiver, ArchiveEntry entry, string place)
        {
            Place = place;
            FileName = entry.FileName;
            UpdateTime = entry.UpdateTime;

            _Archiver = archiver;
        }

        // Bitmapロード
        private BitmapSource LoadBitmap()
        {
            using (var stream = _Archiver.OpenEntry(FileName))
            {
                foreach (var loaderType in ModelContext.BitmapLoaderManager.OrderList)
                {
                    try
                    {
                        var bitmapLoader = BitmapLoaderManager.Create(loaderType);
                        stream.Seek(0, SeekOrigin.Begin);
                        var bmp = bitmapLoader.Load(stream, FileName);
                        if (bmp != null) return bmp;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"{e.Message}\nat '{FileName}' by {loaderType}");
                    }
                }

                throw new ApplicationException("画像の読み込みに失敗しました");
            }
        }


        // 開発用遅延
        [Conditional("DEBUG")]
        private void __Delay(int ms)
        {
            Thread.Sleep(ms);
        }


        // コンテンツをロードする
        protected override object LoadContent()
        {
            //__Delay(200);

            try
            {
                var bitmapSource = LoadBitmap();
                if (bitmapSource == null) throw new ApplicationException("cannot load by BitmapImge.");
                Width = bitmapSource.PixelWidth;
                Height = bitmapSource.PixelHeight;
                Color = bitmapSource.GetOneColor();

                // GIFアニメ用にファイル展開
                if (IsEnableAnimatedGif && LoosePath.GetExtension(FileName) == ".gif")
                {
                    if (_GifFileUri == null)
                    {
                        if (_Archiver is FolderFiles)
                        {
                            _GifFileUri = new Uri(((FolderFiles)_Archiver).GetFullPath(FileName));
                        }
                        else
                        {
                            var tempFile = Temporary.CreateCountedTempFileName("img", ".gif");
                            _Archiver.ExtractToFile(FileName, tempFile);
                            _Archiver.TrashBox.Add(new TrashFile(tempFile));
                            _GifFileUri = new Uri(tempFile);
                        }
                    }
                    return _GifFileUri;
                }
                else
                {
                    return bitmapSource;
                }
            }
            catch (Exception e)
            {
                Message = "Exception: " + e.Message;
                Width = 320;
                Height = 320 * 1.25;
                Color = Colors.Black;

                return new FilePageContext()
                {
                    Icon = FilePageIcon.Alart,
                    FileName = FileName,
                    Message = e.Message
                };
            }
        }
    }

}
