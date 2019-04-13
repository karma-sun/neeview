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
        protected override List<ArchiveEntry> GetEntriesInner(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var playlist = PlaylistFile.Load(Path);
            var list = new List<ArchiveEntry>();

            foreach (var item in playlist.Items)
            {
                var entry = CreateEntry(item, list.Count);
                list.Add(entry);
            }

            return list;
        }

        private ArchiveEntry CreateEntry(string path, int id)
        {
            ArchiveEntry entry = new ArchiveEntry()
            {
                Id = id,
                IsValid = true,
                Archiver = this,
                RawEntryName = LoosePath.GetFileName(path),
                Link = path,
            };

            try
            {
                var directoryInfo = new DirectoryInfo(path);
                if (directoryInfo.Exists)
                {
                    entry.Length = -1;
                    entry.LastWriteTime = directoryInfo.LastWriteTime;
                    entry.IsValid = true;
                }
                else
                {
                    var fileInfo = new FileInfo(path);
                    if (fileInfo.Exists)
                    {
                        entry.Length = fileInfo.Length;
                        entry.LastWriteTime = fileInfo.LastWriteTime;
                        entry.IsValid = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // 有効でないエントリ。無効な項目として生成
                Debug.WriteLine(ex.Message);
            }

            return entry;
        }


        // ストリームを開く
        protected override Stream OpenStreamInner(ArchiveEntry entry)
        {
            Debug.Assert(entry.Link != null);
            return new FileStream(entry.Link, FileMode.Open, FileAccess.Read);
        }

        // ファイルパス取得
        public override string GetFileSystemPath(ArchiveEntry entry)
        {
            Debug.Assert(entry.Link != null);
            return entry.Link;
        }

        // ファイル出力
        protected override void ExtractToFileInner(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            Debug.Assert(entry.Link != null);
            File.Copy(entry.Link, exportFileName, isOverwrite);
        }

        #endregion
    }
}

