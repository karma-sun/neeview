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
    public class BitmapContent : PageContent, IHasPictureSource
    {
        private PictureInfo _pictureInfo;
        private object _lock = new object();

        // picture source
        public PictureSource PictureSource { get; protected set; }

        public virtual PictureInfo PictureInfo => _pictureInfo;

        // picture
        public Picture Picture { get; protected set; }

        // bitmap source
        public BitmapSource BitmapSource => Picture?.BitmapSource;

        // bitmap color
        public Color Color => PictureInfo != null ? PictureInfo.Color : Colors.Black;

        /// <summary>
        /// BitmapSourceがあればコンテンツ有効
        /// </summary>
        public override bool IsLoaded => BitmapSource != null || PageMessage != null;


        /// <summary>
        /// PictureSourceのロック
        /// </summary>
        public bool IsPictureSourceLocked => State != PageContentState.None;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entry"></param>
        public BitmapContent(ArchiveEntry entry) : base(entry)
        {
        }

        /// <summary>
        /// 使用メモリサイズ (Picture)
        /// </summary>
        public override long GetContentMemorySize()
        {
            var pictre = Picture;
            return pictre != null ? pictre.GetMemorySize() : 0;
        }

        /// <summary>
        /// 使用メモリサイズ (PictureSource)
        /// </summary>
        public override long GetPictureSourceMemorySize()
        {
            var source = PictureSource;
            return source != null ? source.GetMemorySize() : 0;
        }

        /// <summary>
        /// PictureSource初期化
        /// </summary>
        private PictureSource LoadPictureSource(CancellationToken token)
        {
            lock (_lock)
            {
                var source = PictureSource;
                if (source == null)
                {
                    source = PictureSourceFactory.Create(Entry, _pictureInfo, PictureSourceCreateOptions.None, token);
                    _pictureInfo = source.CreatePictureInfo(token);
                    this.PictureSource = source;

                    Book.Default?.BookMemoryService.AddPictureSource(this);
                }

                return source;
            }
        }

        /// <summary>
        /// PictureSource開放
        /// </summary>
        public void UnloadPictureSource()
        {
            lock(_lock)
            {
                PictureSource = null;
            }
        }



        /// <summary>
        /// 画像読込
        /// </summary>
        protected Picture LoadPicture(ArchiveEntry entry, CancellationToken token)
        {
            try
            {
                var source = LoadPictureSource(token);

                var picture = new Picture(source);
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

        public void UnloadPicture()
        {
            this.Picture?.Cancel();
            this.Picture = null;
        }


        /// <summary>
        /// コンテンツロード
        /// </summary>
        public override async Task LoadContentAsync(CancellationToken token)
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
        public override void UnloadContent()
        {
            this.PageMessage = null;

            UnloadPicture();

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

            var source = LoadPictureSource(token);

            byte[] thumbnailRaw = null;

            if (this.PageMessage != null)
            {
                thumbnailRaw = null;
            }
            else
            {
                thumbnailRaw = source.CreateThumbnail(ThumbnailProfile.Current, token);
            }

            token.ThrowIfCancellationRequested();
            Thumbnail.Initialize(thumbnailRaw);

            await Task.CompletedTask;
        }
    }
}
