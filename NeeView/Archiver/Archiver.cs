// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// アーカイバの種類
    /// </summary>
    public enum ArchiverType
    {
        None,

        FolderFiles,
        ZipArchiver,
        ZipArchiverKeepOpened, // 未使用
        SevenZipArchiver,
        SusieArchiver,

        DefaultArchiver = ZipArchiver
    }
    
    /// <summary>
    /// アーカイブエントリ
    /// </summary>
    public class ArchiveEntry
    {
        public string FileName { get; set; }
        public DateTime UpdateTime { get; set; }
    }

    /// <summary>
    /// アーカイバ基底クラス
    /// </summary>
    public abstract class Archiver : IDisposable
    {
        // アーカイブのパス
        public abstract string FileName { get; }

        // 親アーカイブ
        public Archiver Parent { get; set; }

        // エントリリストを取得
        public abstract List<ArchiveEntry> GetEntries();

        // エントリのストリームを取得
        public abstract Stream OpenEntry(string entryName);

        // エントリをファイルとして出力
        public abstract void ExtractToFile(string entryName, string exportFileName);

        public string GetPlace()
        {
            if (Parent == null || Parent is FolderFiles)
            {
                return FileName;
            }
            else
            {
                return Parent.GetPlace();
            }
        }

        // 廃棄用ゴミ箱
        public TrashBox TrashBox { get; private set; } = new TrashBox();

        // 廃棄処理
        public virtual void Dispose()
        {
            TrashBox.Dispose();
        }
    }
}
