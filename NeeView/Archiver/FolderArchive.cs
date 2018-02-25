// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        public override string ToString()
        {
            return "フォルダー";
        }

        //
        public override bool IsFileSystem { get; } = true;

        //
        private bool _isDisposed;

        // コンストラクタ
        public FolderArchive(string path, ArchiveEntry source, bool isRoot) : base(path, source, isRoot)
        {
        }

        //
        public override bool IsDisposed => _isDisposed;

        //
        public override void Dispose()
        {
            _isDisposed = true;
            base.Dispose();
        }


        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // リスト取得
        public override List<ArchiveEntry> GetEntries(CancellationToken token)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

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
                    RawEntryName = name,
                    Length = (info is FileInfo fileInfo) ? fileInfo.Length : -1,
                    LastWriteTime = info.LastWriteTime,
                });
            }

            return list;
        }


        // ストリームを開く
        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

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
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            File.Copy(GetFileSystemPath(entry), exportFileName, isOverwrite);
        }
    }

}
