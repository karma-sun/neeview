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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 画像コンテンツ
    /// </summary>
    public class BitmapContent : PageContent
    {
        // picture
        public Picture Picture { get; protected set; }

        // bitmap source
        public BitmapSource BitmapSource => Picture?.BitmapSource;

        // bitmap color
        public Color Color => Picture != null ? Picture.PictureInfo.Color : Colors.Black;

        /// <summary>
        /// BitmapSourceがあればコンテンツ有効
        /// </summary>
        public override bool IsLoaded => BitmapSource != null || PageMessage != null;

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
        protected async Task<Picture> LoadPictureAsync(ArchiveEntry entry, CancellationToken token)
        {
            try
            {
                var picture = new Picture(entry);
                await picture.LoadAsync();

                this.Size = picture.PictureInfo.Size;
                ////this.BitmapInfo = new BitmapInfo(); // ##

                await picture.CreateBitmapAsync(Size.Empty);

                //
                ////var bitmapLoader = new BitmapLoader(entry, BookProfile.Current.IsEnableExif);
                ////var bitmap = await bitmapLoader.LoadAsync(token);
                ////if (bitmap == null) throw new ApplicationException("画像の読み込みに失敗しました。");

                ////Size = new Size(bitmap.Source.PixelWidth, bitmap.Source.PixelHeight);
                ////BitmapInfo = bitmap.Info;

                /*
                try
                {
                    // 基本色
                    BitmapInfo.Color = bitmap.GetOneColor();

                    // ピクセル深度
                    BitmapInfo.BitsPerPixel = bitmap.GetSourceBitsPerPixel();
                }
                catch (Exception e)
                {
                    // ここの例外はスルー
                    Debug.WriteLine(e.Message);
                }
                */


                return picture;
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

            var picture = await LoadPictureAsync(Entry, token);

            if (!token.IsCancellationRequested)
            {
                this.Picture = picture;
                RaiseLoaded();
                RaiseChanged();
            }

            if (Thumbnail.IsValid) return;
            Thumbnail.Initialize(picture.BitmapSource);
        }

        /// <summary>
        /// コンテンツ開放
        /// </summary>
        public override void Unload()
        {
            this.PageMessage = null;
            this.Picture = null;
            RaiseChanged();

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

            var bitmapSource = BitmapSource ?? (await LoadPictureAsync(Entry, token))?.BitmapSource;
            Thumbnail.Initialize(bitmapSource);
        }
    }
}
