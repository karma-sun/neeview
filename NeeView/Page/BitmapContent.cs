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
        private object _lock = new object();

        // picture source
        public PictureSource PictureSource { get; protected set; }

        public virtual PictureInfo PictureInfo => PictureSource?.PictureInfo;

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
        /// PictureSource初期化
        /// </summary>
        private void InitialzePictureSource(CancellationToken token)
        {
            lock (_lock)
            {
                if (PictureSource == null)
                {
                    PictureSource = PictureSourceFactory.Create(Entry, false, token);
                }
            }
        }


        /// <summary>
        /// 画像読込
        /// </summary>
        protected Picture LoadPicture(ArchiveEntry entry, CancellationToken token)
        {
            try
            {
                InitialzePictureSource(token);

                var picture = new Picture(PictureSource);
                picture.Initialize(token);
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

            this.Picture = LoadPicture(Entry, token);

            RaiseLoaded();
            RaiseChanged();

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

            this.PictureSource = null;

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

            InitialzePictureSource(token);

            byte[] thumbnailRaw = null;

            if (this.PageMessage != null)
            {
                thumbnailRaw = null;
            }
            else
            {
                thumbnailRaw = PictureSource.CreateThumbnail(token);
            }

            token.ThrowIfCancellationRequested();
            Thumbnail.Initialize(thumbnailRaw);

            await Task.CompletedTask;
        }
    }
}
