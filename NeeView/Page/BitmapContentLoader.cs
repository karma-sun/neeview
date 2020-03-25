using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class BitmapContentLoader : IContentLoader, IHasPictureSource
    {
        private BitmapContent _content;
        private object _lock = new object();

        public BitmapContentLoader(BitmapContent content)
        {
            _content = content;
        }


        public event EventHandler Loaded;


        public PictureSource PictureSource => _content.PictureSource;

        public bool IsPictureSourceLocked => _content.IsContentLocked;


        #region IDisposable Support
        private bool _disposedValue = false;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Loaded = null;
                    //// 以下、不要では？
                    ////UnloadContent(); 
                    ////UnloadPictureSource();
                    ////_content.SetPictureInfo(null);
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        protected void RaiseLoaded()
        {
            Loaded?.Invoke(this, null);
        }

        /// <summary>
        /// PictureSource初期化
        /// </summary>
        private PictureSource LoadPictureSource(CancellationToken token)
        {
            lock (_lock)
            {
                var source = _content.PictureSource;
                if (source == null)
                {
                    source = PictureSourceFactory.Create(_content.Entry, _content.PictureInfo, PictureSourceCreateOptions.None, token);
                    _content.SetPictureInfo(MemoryControl.Current.RetryFuncWithMemoryCleanup(() => source.CreatePictureInfo(token)));
                    _content.SetPictureSource(source);

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
                _content.SetPictureSource(null);
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
                if (!Config.Current.ImageResizeFilter.IsEnabled)
                {
                    picture.CreateImageSource(Size.Empty, token);
                }

                return picture;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // TODO: これはLoaderの役割
                _content.SetPageMessage(ex);
                throw;
            }
        }

        public void UnloadPicture()
        {
            _content.SetPicture(null);
        }

        /// <summary>
        /// Pictureの標準画像生成
        /// </summary>
        protected void PictureCreateBitmapSource(CancellationToken token)
        {
            try
            {
                _content.Picture?.CreateImageSource(Size.Empty, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _content.SetPageMessage(ex);
                throw;
            }
        }

        /// <summary>
        /// コンテンツロード (Template)
        /// </summary>
        protected virtual async Task LoadContentAsyncTemplate(Action append, CancellationToken token)
        {
            if (_content.IsLoaded) return;

            _content.SetPicture(LoadPicture(_content.Entry, token));

            append?.Invoke();

            RaiseLoaded();
            _content.UpdateDevStatus();

            await Task.CompletedTask;
        }

        /// <summary>
        /// コンテンツロード
        /// </summary>
        public virtual async Task LoadContentAsync(CancellationToken token)
        {
            await LoadContentAsyncTemplate(() =>
            {
                // NOTE: リサイズフィルター有効の場合はBitmapSourceの生成をサイズ確定まで遅延させる
                if (!Config.Current.ImageResizeFilter.IsEnabled)
                {
                    PictureCreateBitmapSource(token);
                }
            },
            token);
        }

        /// <summary>
        /// コンテンツ開放
        /// </summary>
        public void UnloadContent()
        {
            _content.SetPageMessage((PageMessage)null);
            UnloadPicture();
            _content.UpdateDevStatus();

            MemoryControl.Current.GarbageCollect();
        }

        /// <summary>
        /// サムネイルロード
        /// </summary>
        public virtual async Task LoadThumbnailAsync(CancellationToken token)
        {
            _content.Thumbnail.Initialize(_content.Entry, null);

            if (_content.Thumbnail.IsValid) return;
            token.ThrowIfCancellationRequested();

            var source = LoadPictureSource(token);

            byte[] thumbnailRaw = null;

            if (_content.PageMessage != null)
            {
                thumbnailRaw = null;
            }
            else
            {
                thumbnailRaw = MemoryControl.Current.RetryFuncWithMemoryCleanup(() => source.CreateThumbnail(ThumbnailProfile.Current, token));
            }

            token.ThrowIfCancellationRequested();
            _content.Thumbnail.Initialize(thumbnailRaw);

            await Task.CompletedTask;
        }
    }

}
