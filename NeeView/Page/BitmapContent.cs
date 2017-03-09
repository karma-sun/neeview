// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    /// 画像コンテンツ
    /// </summary>
    public class BitmapContent : PageContent
    {
        // property.
        public static bool IsEnableExif { get; set; } = true;

        // bitmap source
        public BitmapSource BitmapSource { get; protected set; }

        // bitmap info
        public BitmapInfo BitmapInfo { get; protected set; }

        /// <summary>
        /// BitmapSourceがあればコンテンツ有効
        /// </summary>
        public override bool IsLoaded => BitmapSource != null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entry"></param>
        public BitmapContent(ArchiveEntry entry) : base(entry)
        {
        }

        /// <summary>
        /// 画像読込
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected async Task<BitmapSource> LoadBitmapAsync(ArchiveEntry entry, CancellationToken token)
        {
            try
            {
                var bitmapLoader = new BitmapLoader(entry, IsEnableExif);
                var bitmap = await bitmapLoader.LoadAsync(token);
                if (bitmap == null) throw new ApplicationException("画像の読み込みに失敗しました。");

                Size = new Size(bitmap.Source.PixelWidth, bitmap.Source.PixelHeight);
                BitmapInfo = bitmap.Info;

                try
                {
                    // 基本色
                    BitmapInfo.Color = bitmap.Source.GetOneColor();

                    // ピクセル深度
                    BitmapInfo.BitsPerPixel = bitmap.Source.GetSourceBitsPerPixel();
                }
                catch (Exception e)
                {
                    // ここの例外はスルー
                    Debug.WriteLine(e.Message);
                }

                return bitmap.Source;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                PageMessage = new PageMessage()
                {
                    Icon = FilePageIcon.Alart,
                    Message = e.Message
                };
                return null;
            }
        }


        /// <summary>
        /// コンテンツロード
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task LoadAsync(CancellationToken token)
        {
            if (IsLoaded) return;

            var bitmap = await LoadBitmapAsync(Entry, token);

            if (!token.IsCancellationRequested)
            {
                BitmapSource = bitmap;
                RaiseLoaded();
            }

            if (Thumbnail.IsValid) return;
            Thumbnail.Initialize(bitmap);
        }

        /// <summary>
        /// コンテンツ開放
        /// </summary>
        public override void Unload()
        {
            PageMessage = null;
            BitmapSource = null;

            MemoryControl.Current.GarbageCollect();
        }

        /// <summary>
        /// サムネイルロード
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task LoadThumbnailAsync(CancellationToken token)
        {
            if (Thumbnail.IsValid) return;

            // TODO: コンテンツ読み込み要求が有効な場合の処理

            var bitmapSource = BitmapSource ?? await LoadBitmapAsync(Entry, token);
            Thumbnail.Initialize(bitmapSource);
        }
    }
}
