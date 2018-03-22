using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：通常ファイル
    /// ディレクトリをアーカイブとみなしてアクセスする
    /// </summary>
    public class FolderArchive : Archiver
    {
        #region Constructors

        public FolderArchive(string path, ArchiveEntry source, bool isRoot) : base(path, source, isRoot)
        {
        }

        #endregion

        #region Properties

        public override bool IsFileSystem { get; } = true;

        #endregion

        #region Methods

        public override string ToString()
        {
            return "Folder";
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

            int prefixLen = Path.Length;
            var list = new List<ArchiveEntry>();

            var directory = new DirectoryInfo(Path);
            foreach (var info in directory.EnumerateFileSystemInfos())
            {
                token.ThrowIfCancellationRequested();

                if ((info.Attributes & FileAttributes.Hidden) != 0)
                {
                    continue;
                }

                var name = info.FullName.Substring(prefixLen).TrimStart('\\', '/');
                list.Add(new ArchiveEntry()
                {
                    Archiver = this,
                    Id = list.Count,
                    RawEntryName = name + (info.Attributes.HasFlag(FileAttributes.Directory) ? "\\" : ""),
                    Length = (info is FileInfo fileInfo) ? fileInfo.Length : -1,
                    LastWriteTime = info.LastWriteTime,
                });
            }

            return list;
        }


        // ストリームを開く
        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            return new FileStream(GetFileSystemPath(entry), FileMode.Open, FileAccess.Read);
        }

        // ファイルパス取得
        public override string GetFileSystemPath(ArchiveEntry entry)
        {
            return System.IO.Path.Combine(Path, entry.EntryName);
        }

        // ファイルパス取得
        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            File.Copy(GetFileSystemPath(entry), exportFileName, isOverwrite);
        }

        #endregion
    }
}
