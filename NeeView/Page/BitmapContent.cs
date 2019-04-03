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


        public BitmapContent(ArchiveEntry entry) : base(entry)
        {
        }


        // picture source
        public PictureSource PictureSource { get; protected set; }

        // picture info
        public virtual PictureInfo PictureInfo => _pictureInfo;

        // picture
        public Picture Picture { get; protected set; }

        // bitmap source
        public BitmapSource BitmapSource => Picture?.BitmapSource;

        // bitmap color
        public Color Color => PictureInfo != null ? PictureInfo.Color : Colors.Black;

        // content size
        public override Size Size => PictureInfo != null ? PictureInfo.Size : SizeExtensions.Zero;

        /// <summary>
        /// BitmapSourceがあればコンテンツ有効
        /// </summary>
        public override bool IsLoaded => Picture != null || PageMessage != null;

        public override bool IsAllLoaded => BitmapSource != null || PageMessage != null;

        /// <summary>
        /// PictureSourceのロック
        /// </summary>
        public bool IsPictureSourceLocked => State != PageContentState.None;

        public override bool CanResize => true;


        public virtual Size GetRenderSize(Size size)
        {
            return CanResize &&  PictureProfile.Current.IsResizeFilterEnabled ? size : Size.Empty;
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
                    _pictureInfo = MemoryControl.Current.RetryFuncWithMemoryCleanup(() => source.CreatePictureInfo(token));
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
            lock (_lock)
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

                // NOTE: リサイズフィルター有効の場合はBitmapSourceの生成をサイズ確定まで遅延させる
                if (!PictureProfile.Current.IsResizeFilterEnabled)
                {
                    picture.CreateBitmapSource(Size.Empty, token);
                }

                return picture;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                SetExceptionMessage(ex);
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
            UpdateDevStatus();

            await Task.CompletedTask;
        }

        /// <summary>
        /// コンテンツ開放
        /// </summary>
        public override void UnloadContent()
        {
            this.PageMessage = null;
            UnloadPicture();
            UpdateDevStatus();

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
                thumbnailRaw = MemoryControl.Current.RetryFuncWithMemoryCleanup(() => source.CreateThumbnail(ThumbnailProfile.Current, token));
            }

            token.ThrowIfCancellationRequested();
            Thumbnail.Initialize(thumbnailRaw);

            await Task.CompletedTask;
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    UnloadContent();
                    UnloadPictureSource();
                    _pictureInfo = null;
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion IDisposable Support
    }
}
