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
        protected Picture LoadPicture(ArchiveEntry entry, PictureCreateOptions options, CancellationToken token)
        {
            try
            {
                var picture = PictureFactory.Current.Create(entry, options, token);
                this.Size = picture.PictureInfo.Size;
                return picture;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // 画像ではない
                PageMessage = new PageMessage()
                {
                    Icon = FilePageIcon.Alart,
                    Message = ex.Message
                };
                throw;
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

            var picture = LoadPicture(Entry, PictureCreateOptions.CreateBitmap, token);
            this.Picture = picture;
            RaiseLoaded();
            RaiseChanged();


            // TODO: サムネイル自動生成、ここでおこなうのはよろしくない？
#if false 
            if (Thumbnail.IsValid || picture == null) return;
            Thumbnail.Initialize(picture.CreateThumbnail(token));
#endif
            await Task.CompletedTask;
        }

        /// <summary>
        /// コンテンツ開放
        /// </summary>
        public override void Unload()
        {
            this.PageMessage = null;
            this.Picture?.Cancel();
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

            byte[] thumbnailRaw = null;

            if (this.Picture != null)
            {
                thumbnailRaw = this.Picture.CreateThumbnail(token);
            }
            else if (this.PageMessage != null)
            {
                thumbnailRaw = null;
            }
            else
            {
                var picture = LoadPicture(Entry, PictureCreateOptions.CreateThumbnail, token);
                thumbnailRaw = picture?.CreateThumbnail(token);
            }

            token.ThrowIfCancellationRequested();
            Thumbnail.Initialize(thumbnailRaw);

            await Task.CompletedTask;
        }
    }
}
