using NeeView;
using NeeView.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：プレイリスト方式
    /// </summary>
    public class PlaylistArchive : Archiver
    {
        public const string Extension = ".nvpls";

        #region Constructors

        public PlaylistArchive(string path, ArchiveEntry source) : base(path, source)
        {
        }

        #endregion

        #region Properties

        public override bool IsFileSystem { get; } = false;

        #endregion

        #region Methods

        public static bool IsSupportExtension(string path)
        {
            return LoosePath.GetExtension(path) == Extension;
        }

        public override string ToString()
        {
            return "Playlist";
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // リスト取得
        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var playlist = PlaylistFile.Load(Path);
            var list = new List<ArchiveEntry>();

            foreach (var item in playlist.Items)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    var entry = await CreateEntryAsync(item, list.Count, token);
                    list.Add(entry);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return list;
        }

        private async Task<ArchiveEntry> CreateEntryAsync(string path, int id, CancellationToken token)
        {
            var innterEntry = await ArchiveEntryUtility.CreateAsync(path, token);

            ArchiveEntry entry = new ArchiveEntry()
            {
                Id = id,
                IsValid = true,
                Archiver = this,
                RawEntryName = LoosePath.GetFileName(path),
                Link = path,
                Instance = innterEntry,
                Length = innterEntry.Length,
                LastWriteTime = innterEntry.LastWriteTime,
            };

            return entry;
        }


        // ストリームを開く
        protected override Stream OpenStreamInner(ArchiveEntry entry)
        {
            return ((ArchiveEntry)entry.Instance).OpenEntry();
        }

        // ファイルパス取得
        public override string GetFileSystemPath(ArchiveEntry entry)
        {
            return ((ArchiveEntry)entry.Instance).GetFileSystemPath();
        }

        // ファイル出力
        protected override void ExtractToFileInner(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            ((ArchiveEntry)entry.Instance).ExtractToFile(exportFileName, isOverwrite);
        }

        #endregion
    }
}

