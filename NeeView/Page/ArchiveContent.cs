// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
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
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// アーカイブコンテンツ
    /// 対象のサムネイルを作成
    /// </summary>
    public class ArchiveContent : BitmapContent
    {
        private string _path;

        /// <summary>
        /// コンテンツ有効フラグは常にfalse
        /// </summary>
        public override bool IsLoaded => false;

        /// <summary>
        /// コンスラクタ
        /// </summary>
        /// <param name="entry">対象アーカイブもしくはファイルのエントリ</param>
        public ArchiveContent(ArchiveEntry entry) : base(entry)
        {
            _path = entry?.FullPath;

            PageMessage = new PageMessage()
            {
                Icon = FilePageIcon.Alart,
                Message = "このページはサムネイル作成専用です",
            };

            // エントリが有効でない場合の処理
            if (!entry.IsValid && !entry.IsArchivePath)
            {
                Thumbnail.Initialize(null);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path"></param>
        public ArchiveContent(string path) : base(null)
        {
            _path = path;

            PageMessage = new PageMessage()
            {
                Icon = FilePageIcon.Alart,
                Message = "このページはサムネイル作成専用です",
            };
        }

        /// <summary>
        /// コンテンツロードは非サポート
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override Task LoadAsync(CancellationToken token)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// サムネイル初期化
        /// ページ指定があるため特殊
        /// </summary>
        public override void InitializeThumbnail()
        {
            InitializeArchiveEntry();
            Thumbnail.Initialize(Entry, null);
        }

        /// <summary>
        /// エントリー初期化
        /// </summary>
        private void InitializeArchiveEntry()
        {
            if (this.Entry == null)
            {
                this.Entry = new ArchiveEntry(_path);
            }
        }

        /// <summary>
        /// サムネイルロード
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task LoadThumbnailAsync(CancellationToken token)
        {
            InitializeArchiveEntry();

            if (Thumbnail.IsValid) return;

            if (!Entry.IsValid && !Entry.IsArchivePath)
            {
                Thumbnail.Initialize(null);
                return;
            }

            try
            {
                var picture = await LoadPictureAsync(token);
                Thumbnail.Initialize(picture?.CreateThumbnail());
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                // 例外無効
                Debug.WriteLine($"LoadThumbnail: {e.Message}");
                Thumbnail.Initialize(null);
            }

        }

        /// <summary>
        /// エントリに対応するサムネイル画像生成
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<Picture> LoadPictureAsync(CancellationToken token)
        {
            if (this.Entry.Archiver != null && this.Entry.Archiver is MediaArchiver)
            {
                return null;
            }
            if (this.Entry.IsArchivePath)
            {
                using (var entry = await ArchiveFileSystem.CreateArchiveEntry(this.Entry.FullPath, token))
                {
                    if (entry.IsArchive())
                    {
                        return await LoadArchivePictureAsync(entry, token);
                    }
                    else
                    {
                        return await LoadPictureAsync(entry, PictureCreateOptions.CreateThumbnail, token);
                    }
                }
            }
            else
            {
                return await LoadArchivePictureAsync(this.Entry, token);
            }
        }


        /// <summary>
        /// アーカイブサムネイル読込
        /// 名前順で先頭のページ
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="entryName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<Picture> LoadArchivePictureAsync(ArchiveEntry entry, CancellationToken token)
        {
            if (System.IO.Directory.Exists(entry.FullPath) || ArchiverManager.Current.IsSupported(entry.FullPath))
            {
                if (ArchiverManager.Current.GetSupportedType(entry.FullPath) == ArchiverType.MediaArchiver)
                {
                    return null;
                }

                using (var archiver = await ArchiverManager.Current.CreateArchiverAsync(entry, true, false, token))
                {
                    bool isRecursive = !archiver.IsFileSystem && BookHub.Current.IsArchiveRecursive;
                    using (var collector = new EntryCollection(archiver, isRecursive, false))
                    {
                        await collector.FirstOneAsync(token);
                        var select = collector.Collection.FirstOrDefault();

                        if (select != null)
                        {
                            return await LoadPictureAsync(select, PictureCreateOptions.CreateThumbnail, token);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            else
            {
                return await LoadPictureAsync(entry, PictureCreateOptions.CreateThumbnail, token);
            }
        }
    }

}
