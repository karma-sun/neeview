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
            Thumbnail.Initialize(Entry, null);
        }

        /// <summary>
        /// サムネイルロード
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task LoadThumbnailAsync(CancellationToken token)
        {
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
            using (var archiver = await ArchiverManager.Current.CreateArchiverAsync(entry, false, token))
            {
                archiver.RootFlag = true;
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
    }

}
