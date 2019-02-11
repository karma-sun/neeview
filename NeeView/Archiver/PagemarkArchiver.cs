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
        public PagemarkArchiver(string path, ArchiveEntry source, bool isRoot) : base(path, source, isRoot)
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
        public override List<ArchiveEntry> GetEntries(CancellationToken token)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            token.ThrowIfCancellationRequested();

            var placeQuery = new QueryPath(Path);
            if (placeQuery.Scheme != QueryScheme.Pagemark) throw new ApplicationException("Archive is not pagemark.");

            var node = PagemarkCollection.Current.FindNode(placeQuery);
            if (!(node.Value is PagemarkFolder))
            {
                Debug.WriteLine($"No pagemark folder.");
                return new List<ArchiveEntry>();
            }

            int prefixLen = Path.Length;
            var list = new List<ArchiveEntry>();

            foreach (var child in node.Where(e => e.Value is Pagemark))
            {
                var pagemark = (Pagemark)child.Value;

                list.Add(new ArchiveEntry()
                {
                    Archiver = this,
                    Id = list.Count,
                    Instance = child,
                    RawEntryName = LoosePath.Combine(LoosePath.GetFileName(pagemark.Place), pagemark.DispName),
                    Length = 0,
                    LastWriteTime = default,
                });
            }

            return list;
        }


        // ストリームを開く
        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            if (entry.Instance is TreeListNode<IPagemarkEntry> node && node.Value is Pagemark pagemark)
            {
                // NOTE: 非同期関数をResult待ちしているので要注意
                using (var entry_ = ArchiveFileSystem.CreateArchiveEntry(pagemark.FullName, CancellationToken.None).Result)
                {
                    var mem = new MemoryStream();
                    entry_.OpenEntry().CopyTo(mem);
                    mem.Seek(0, SeekOrigin.Begin);
                    return mem;
                }
            }

            return null;
        }

        // 実在ファイルパス取得
        public override string GetFileSystemPath(ArchiveEntry entry)
        {
            if (entry.Instance is TreeListNode<IPagemarkEntry> node && node.Value is Pagemark pagemark)
            {
                return ArchiveFileSystem.GetExistEntryName(pagemark.FullName);
            }

            return null;
        }

        // ファイルパス取得
        // HACK: async化
        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            if (entry.Instance is TreeListNode<IPagemarkEntry> node && node.Value is Pagemark pagemark)
            {
                using (var entry_ = Task.Run(() => ArchiveFileSystem.CreateArchiveEntry(pagemark.FullName, CancellationToken.None).Result).Result)
                {
                    entry_.ExtractToFile(exportFileName, isOverwrite);
                }
            }
        }
    }
}
