// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバ：通常ファイル
    /// ディレクトリをアーカイブとみなしてアクセスする
    /// </summary>
    public class FolderFiles : Archiver
    {
        public override string ToString()
        {
            return "フォルダー";
        }

        public override bool IsFileSystem { get; } = true;

        private string _FolderFileName;
        public override string FileName => _FolderFileName;

        // コンストラクタ
        public FolderFiles(string folderFileName)
        {
            _FolderFileName = folderFileName;
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // リスト取得
        public override Dictionary<string, ArchiveEntry> GetEntries()
        {
            int prefixLen = _FolderFileName.Length;
            Entries.Clear();
            foreach (var path in Directory.GetFiles(_FolderFileName))
            {
                var name = path.Substring(prefixLen).TrimStart('\\', '/');
                var fileInfo = new FileInfo(path);
                Entries.Add(name, new ArchiveEntry()
                {
                    FileName = name,
                    FileSize = fileInfo.Length,
                    LastWriteTime = fileInfo.LastWriteTime,
                });
            }
            foreach (var path in Directory.GetDirectories(_FolderFileName))
            {
                var name = path.Substring(prefixLen).TrimStart('\\', '/') + "\\";
                var fileInfo = new DirectoryInfo(path);
                Entries.Add(name, new ArchiveEntry()
                {
                    FileName = name,
                    FileSize = -1,
                    LastWriteTime = fileInfo.LastWriteTime,
                });
            }

            return Entries;
        }


        //
        public override Stream OpenEntry(string entryName)
        {
            return new FileStream(System.IO.Path.Combine(_FolderFileName, entryName), FileMode.Open, FileAccess.Read);
        }


        // ファイルパス取得
        public override string GetFileSystemPath(string entryName)
        {
            return System.IO.Path.Combine(_FolderFileName, entryName);
        }

        //
        public override void ExtractToFile(string entryName, string exportFileName, bool isOverwrite)
        {
            File.Copy(GetFileSystemPath(entryName), exportFileName, isOverwrite);
        }
    }


}
