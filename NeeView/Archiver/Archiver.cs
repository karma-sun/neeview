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
        public long FileSize { get; set; }
        public DateTime LastWriteTime { get; set; }
        public object Instance { get; set; }
    }


    /// <summary>
    /// アーカイバ基底クラス
    /// </summary>
    public abstract class Archiver : IDisposable
    {
        // エントリリスト
        public Dictionary<string, ArchiveEntry> Entries { get; private set; } = new Dictionary<string, ArchiveEntry>();

        // アーカイブのパス
        public abstract string FileName { get; }

        // ファイルシステムの場合はtrue
        public virtual bool IsFileSystem { get; } = false;

        // ファイルシステムでのパスを取得
        public virtual string GetFileSystemPath(string entryName)
        {
            throw new NotImplementedException();
        }

        // 対応判定
        public abstract bool IsSupported();

        // 親アーカイブ
        public Archiver Parent { get; set; }

        // エントリリストを取得
        public abstract Dictionary<string, ArchiveEntry> GetEntries();

        // エントリのストリームを取得
        public abstract Stream OpenEntry(string entryName);

        // エントリをファイルとして出力
        public abstract void ExtractToFile(string entryName, string exportFileName, bool isOverwrite);

        //
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
