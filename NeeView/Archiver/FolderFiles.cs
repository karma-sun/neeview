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
            return "FileSystem";
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
        public override List<ArchiveEntry> GetEntries()
        {
            int prefixLen = _FolderFileName.Length;
            var entries = new List<ArchiveEntry>();
            foreach (var path in Directory.GetFiles(_FolderFileName))
            {
                entries.Add(new ArchiveEntry()
                {
                    FileName = path.Substring(prefixLen).TrimStart('\\', '/'),
                    UpdateTime = File.GetLastWriteTime(path),
                });
            }
            foreach (var path in Directory.GetDirectories(_FolderFileName))
            {
                entries.Add(new ArchiveEntry()
                {
                    FileName = path.Substring(prefixLen).TrimStart('\\', '/') + "\\",
                    UpdateTime = File.GetLastWriteTime(path),
                });
            }

            return entries;
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
