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
    /// アーカイブエントリ
    /// </summary>
    public class ArchiveEntry
    {
        // 所属アーカイバ
        public Archiver Archiver { get; set; }

        // 登録番号
        public int Id { get; set; }

        // エントリ情報
        public object Instance { get; set; }

        // エントリ名
        public string EntryName { get; set; }

        // ファイルサイズ
        public long FileSize { get; set; }

        // ファイル更新日
        public DateTime? LastWriteTime { get; set; }


        /// <summary>
        /// ファイルシステム所属判定
        /// </summary>
        public bool IsFileSystem => Archiver.IsFileSystem;

        /// <summary>
        /// ファイルシステムでのパスを返す
        /// </summary>
        /// <returns>パス。圧縮ファイルの場合はnull</returns>
        public string GetFileSystemPath()
        {
            return Archiver.GetFileSystemPath(this);
        }

        /// <summary>
        /// ストリームを開く
        /// </summary>
        /// <returns>Stream</returns>
        public Stream OpenEntry()
        {
            return Archiver.OpenStream(this);
        }

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="exportFileName">出力ファイル名</param>
        /// <param name="isOverwrite">上書き許可フラグ</param>
        public void ExtractToFile(string exportFileName, bool isOverwrite)
        {
            Archiver.ExtractToFile(this, exportFileName, isOverwrite);
        }
    }
}
