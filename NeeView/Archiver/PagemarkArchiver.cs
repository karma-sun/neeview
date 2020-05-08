using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：ページマーク
    /// ページマークフォルダーをアーカイブとみなしてアクセスする
    /// </summary>
    public class PagemarkArchiver : Archiver
    {
        public PagemarkArchiver(string path, ArchiveEntry source) : base(path, source)
        {
        }


        public override bool IsFileSystem { get; } = false;


        public override string ToString()
        {
            return "PagemarkFolder";
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

            var placeQuery = new QueryPath(Path);
            if (placeQuery.Scheme != QueryScheme.Pagemark) throw new ApplicationException("Archive is not pagemark.");

            var node = PagemarkCollection.Current.FindNode(placeQuery);
            if (!(node.Value is PagemarkFolder))
            {
                Debug.WriteLine($"No pagemark folder.");
                return new List<ArchiveEntry>();
            }

            var list = new List<ArchiveEntry>();
            foreach (var pagemark in node.Select(e => e.Value).OfType<Pagemark>())
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    var entry = await CreateEntryAsync(pagemark, list.Count, token);
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

        private async Task<ArchiveEntry> CreateEntryAsync(Pagemark pagemark, int id, CancellationToken token)
        {
            var innerEntry = await ArchiveEntryUtility.CreateAsync(pagemark.FullName, token);

            return new ArchiveEntry()
            {
                IsValid = true,
                Archiver = this,
                Id = id,
                RawEntryName = LoosePath.Combine(LoosePath.GetFileName(pagemark.Path), pagemark.DispName),
                Link = pagemark.FullName,
                Instance = innerEntry,
                Length = innerEntry.Length,
                LastWriteTime = innerEntry.LastWriteTime,
            };
        }

        // ストリームを開く
        protected override Stream OpenStreamInner(ArchiveEntry entry)
        {
            return ((ArchiveEntry)entry.Instance).OpenEntry();
        }

        // 実在ファイルパス取得
        public override string GetFileSystemPath(ArchiveEntry entry)
        {
            return ((ArchiveEntry)entry.Instance).GetFileSystemPath();
        }

        // ファイルパス取得
        // HACK: async化
        protected override void ExtractToFileInner(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            ((ArchiveEntry)entry.Instance).ExtractToFile(exportFileName, isOverwrite);
        }

        // エントリー実体のファイルシステム判定
        public override bool IsFileSystemEntry(ArchiveEntry entry)
        {
            return ((ArchiveEntry)entry.Instance).IsFileSystem;
        }
    }
}
