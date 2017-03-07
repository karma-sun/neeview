// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
    /// アーカイバ：通常ファイル
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
        public FolderArchive(string path, ArchiveEntry source) : base(path, source)
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
            foreach (var info in directory.EnumerateFiles())
            {
                token.ThrowIfCancellationRequested();

                var name = info.FullName.Substring(prefixLen).TrimStart('\\', '/');
                list.Add(new ArchiveEntry()
                {
                    Archiver = this,
                    Id = list.Count,
                    EntryName = name,
                    Length = info.Length,
                    LastWriteTime = info.LastWriteTime,
                });
            }
            foreach (var info in directory.EnumerateDirectories())
            {
                token.ThrowIfCancellationRequested();

                var name = info.FullName.Substring(prefixLen).TrimStart('\\', '/') + "\\";
                list.Add(new ArchiveEntry()
                {
                    Archiver = this,
                    Id = list.Count,
                    EntryName = name,
                    Length = -1,
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
