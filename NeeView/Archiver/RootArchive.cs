// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/m

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ファイルシステムの共通アーカイブ.
    /// エントリは全てフルパスになります。
    /// </summary>
    public class RootArchive : FolderArchive
    {
        /// <summary>
        /// 現在のシステムオブジェクト
        /// </summary>
        public static RootArchive Current { get; private set; } = new RootArchive();

        //
        public override string ToString()
        {
            return "ルートフォルダー";
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RootArchive() : base("")
        {
        }

        /// <summary>
        /// エントリ作成
        /// </summary>
        /// <param name="path"></param>
        /// <param name="size"></param>
        /// <param name="lastWriteTime"></param>
        /// <returns></returns>
        public ArchiveEntry CreateArchiveEntry(string path, long size, DateTime lastWriteTime)
        {
            var entry = new ArchiveEntry()
            {
                Archiver = Current,
                EntryName = path,
                FileSize = size,
                LastWriteTime = lastWriteTime
            };

            return entry;
        }

        /// <summary>
        /// エントリ作成
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ArchiveEntry CreateArchiveEntry(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists)
            {
                return CreateArchiveEntry(path, -1, directoryInfo.LastWriteTime);
            }

            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                return CreateArchiveEntry(path, fileInfo.Length, fileInfo.LastWriteTime);
            }

            return CreateArchiveEntry(path, -1, default(DateTime));
        }
    }
}
