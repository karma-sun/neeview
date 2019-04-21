using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ArchiveContentLoader : BitmapContentLoader
    {
        private ArchiveContent _content;

        public ArchiveContentLoader(ArchiveContent content) : base(content)
        {
            _content = content;
        }

        private async Task InitializeEntryAsync(CancellationToken token)
        {
            if (_content.Entry == null)
            {
                var query = new QueryPath(_content.SourcePath);
                query = query.ToEntityPath();
                try
                {
                    _content.SetEntry(await ArchiveEntryUtility.CreateAsync(query.SimplePath, token));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ArchiveContent.Entry: {ex.Message}");
                    _content.SetEntry(ArchiveEntry.Create(query.SimplePath));
                    _content.Thumbnail.Initialize(null);
                }
            }
        }

        /// <summary>
        /// コンテンツロード
        /// </summary>
        public override async Task LoadContentAsync(CancellationToken token)
        {
            await InitializeEntryAsync(token);

            if (_content.IsLoaded) return;

            await LoadThumbnailAsync(token);

            RaiseLoaded();
            _content.UpdateDevStatus();
        }


        /// <summary>
        /// サムネイルロード
        /// </summary>
        public override async Task LoadThumbnailAsync(CancellationToken token)
        {
            await InitializeEntryAsync(token);

            _content.Thumbnail.Initialize(_content.Entry, null);
            if (_content.Thumbnail.IsValid) return;

            if (!_content.Entry.IsValid && !_content.Entry.IsArchivePath)
            {
                _content.Thumbnail.Initialize(null);
                return;
            }

            try
            {
                var picture = await LoadPictureAsync(token);
                token.ThrowIfCancellationRequested();
                if (picture == null)
                {
                    _content.Thumbnail.Initialize(null);
                }
                else if (picture.Type == ThumbnailType.Unique)
                {
                    _content.Thumbnail.Initialize(picture.RawData);
                }
                else
                {
                    _content.Thumbnail.Initialize(picture.Type);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                // 例外無効
                Debug.WriteLine($"LoadThumbnail: {e.Message}");
                _content.Thumbnail.Initialize(null);
            }
        }


        /// <summary>
        /// エントリに対応するサムネイル画像生成
        /// </summary>
        private async Task<ThumbnailPicture> LoadPictureAsync(CancellationToken token)
        {
            if (_content.Entry.Archiver != null && _content.Entry.Archiver is MediaArchiver)
            {
                return new ThumbnailPicture(ThumbnailType.Media);
            }
            if (_content.Entry.IsArchivePath)
            {
                var entry = await ArchiveEntryUtility.CreateAsync(_content.Entry.SystemPath, token);
                if (entry.IsBook())
                {
                    return await LoadArchivePictureAsync(entry, token);
                }
                else
                {
                    return new ThumbnailPicture(CreateThumbnail(entry, token));
                }
            }
            else
            {
                return await LoadArchivePictureAsync(_content.Entry, token);
            }
        }

        /// <summary>
        /// アーカイブサムネイル読込
        /// 名前順で先頭のページ
        /// </summary>
        private async Task<ThumbnailPicture> LoadArchivePictureAsync(ArchiveEntry entry, CancellationToken token)
        {
            // ブックサムネイル検索範囲
            const int searchRange = 2;

            if (System.IO.Directory.Exists(entry.SystemPath) || entry.IsBook())
            {
                if (ArchiverManager.Current.GetSupportedType(entry.SystemPath) == ArchiverType.MediaArchiver)
                {
                    return new ThumbnailPicture(ThumbnailType.Media);
                }

                var select = await ArchiveEntryUtility.CreateFirstImageArchiveEntryAsync(entry, searchRange, token);
                if (select != null)
                {
                    return new ThumbnailPicture(CreateThumbnail(select, token));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new ThumbnailPicture(CreateThumbnail(entry, token));
            }
        }

        private byte[] CreateThumbnail(ArchiveEntry entry, CancellationToken token)
        {
            var source = PictureSourceFactory.Create(entry, null, PictureSourceCreateOptions.IgnoreCompress, token);
            return MemoryControl.Current.RetryFuncWithMemoryCleanup(() => source.CreateThumbnail(ThumbnailProfile.Current, token));
        }

        /// <summary>
        /// 画像、もしくはサムネイルタイプを指定するもの
        /// </summary>
        class ThumbnailPicture
        {
            public ThumbnailType Type { get; set; }
            public byte[] RawData { get; set; }

            public ThumbnailPicture(ThumbnailType type)
            {
                Type = type;
            }

            public ThumbnailPicture(byte[] rawData)
            {
                Type = ThumbnailType.Unique;
                RawData = rawData;
            }
        }
    }

}
