// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
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
    public class ImageContent : PageContent
    {
        // property.
        public static bool IsEnableExif { get; set; } = true;

        // bitmap source
        public BitmapSource BitmapSource { get; protected set; }

        // bitmap info
        public FileBasicInfo Info { get; protected set; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entry"></param>
        public ImageContent(ArchiveEntry entry) : base(entry)
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
                Color = bitmap.Source.GetOneColor();
                Info = bitmap.Info;

                token.ThrowIfCancellationRequested();

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

        //
        private object _lock = new object();

        /// <summary>
        /// コンテンツロード
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task LoadAsync(CancellationToken token)
        {
            if (IsLoaded) return;

            var bitmap = await LoadBitmapAsync(Entry, token);

            lock (_lock)
            {
                BitmapSource = bitmap;
                IsLoaded = true;
            }

            if (Thumbnail.IsValid) return;
            Thumbnail.Initialize(bitmap);
        }

        /// <summary>
        /// コンテンツ開放
        /// </summary>
        public override void Unload()
        {
            bool unloaded = false;

            lock (_lock)
            {
                if (IsLoaded)
                {
                    PageMessage = null;
                    IsLoaded = false;
                    BitmapSource = null;
                    unloaded = true;
                }
            }

            if (unloaded)
            {
                MemoryControl.Current.GarbageCollect();
            }
        }

        /// <summary>
        /// サムネイルロード
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task LoadThumbnailAsync(CancellationToken token)
        {
            if (Thumbnail.IsValid) return;

            var bitmapSource = BitmapSource ?? await LoadBitmapAsync(Entry, token);
            Thumbnail.Initialize(bitmapSource);
        }
    }
}
