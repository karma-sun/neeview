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
        // ToString
        public override string ToString()
        {
            return LoosePath.GetFileName(FileName);
        }

        // コンストラクタ
        public BitmapPage(Archiver archiver, ArchiveEntry entry, string place)
        {
            Place = place;
            Entry = entry;
        }

        //
        public override Page TinyClone()
        {
            return new BitmapPage(Entry.Archiver, Entry, Place);
        }


        // Bitmapロード
        private BitmapContent LoadBitmap()
        {
            var loader = new BitmapLoader(Entry, IsEnableExif);
            return loader.Load();
        }


        // 開発用遅延
        [Conditional("DEBUG")]
        private void __Delay(int ms)
        {
            Thread.Sleep(ms);
        }


        /// <summary>
        /// 画像ロード処理
        /// </summary>
        /// <param name="isUseMediaPlayer">メディアプレイヤーで再生させる</param>
        /// <returns>ページコンテンツ</returns>
        protected object LoadContent(bool isUseMediaPlayer)
        {
            try
            {
                var bitmapLoader = new BitmapLoader(Entry, IsEnableExif);
                var bitmapContent = bitmapLoader.Load();
                if (bitmapContent == null) throw new ApplicationException("画像の読み込みに失敗しました。");
                var bitmapSource = bitmapContent.Source;
                Width = bitmapSource.PixelWidth;
                Height = bitmapSource.PixelHeight;
                Color = bitmapSource.GetOneColor();

                // GIFアニメ用にファイル展開
                if (isUseMediaPlayer)
                {
                    return new AnimatedGifContent()
                    {
                        FileProxy = CreateTempFile(false),
                        BitmapContent = bitmapContent
                    };
                }
                else
                {
                    return bitmapContent;
                }
            }
            catch (Exception e)
            {
                Message = "Exception: " + e.Message;
                Width = 320;
                Height = 320; // * 1.25;
                Color = Colors.Black;

                return new FilePageContent()
                {
                    Icon = FilePageIcon.Alart,
                    FileName = FileName,
                    Message = e.Message,

                    Info = new FileBasicInfo()
                };
            }
        }


        // コンテンツをロードする
        protected override object LoadContent()
        {
            return LoadContent(IsEnableAnimatedGif && LoosePath.GetExtension(FileName) == ".gif");
        }

        // ファイルの出力
        public override void Export(string path)
        {
            Entry.ExtractToFile(path, true);
        }
    }

    // アニメーションGIF用リソース
    public class AnimatedGifContent
    {
        public FileProxy FileProxy { get; set; }
        public BitmapContent BitmapContent { get; set; }
    }
}
