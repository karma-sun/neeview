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
        /// <summary>
        /// 所属アーカイバー.
        /// nullの場合、このエントリはファイルパスを示す
        /// </summary>
        public Archiver Archiver { get; set; }

        /// <summary>
        /// アーカイブ内登録番号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// エントリ情報
        /// アーカイバーで識別子として使用される
        /// </summary>
        public object Instance { get; set; }

        /// <summary>
        /// 有効判定
        /// </summary>
        public bool IsValid { get; set; } = true;

        // 例：
        // a.zip 
        // +- b.zip
        //      +- c\001.jpg <- this!

        /// <summary>
        /// エントリ名(重複有)
        /// </summary>
        /// c\001.jpg
        public string EntryName { get; set; }

        /// <summary>
        /// エントリ名のファイル名
        /// </summary>
        /// 001.jpg
        public string EntryLastName => LoosePath.GetFileName(EntryName);

        /// <summary>
        /// ルートアーカイバー
        /// </summary>
        /// a.zip
        public Archiver RootArchiver => Archiver?.RootArchiver;

        /// <summary>
        /// 所属名
        /// </summary>
        public string RootArchiverName => RootArchiver?.EntryName ?? LoosePath.GetFileName(LoosePath.GetDirectoryName(EntryName));


        /// <summary>
        /// ルートアーカイバーからのエントリ名
        /// </summary>
        ///b.zip\c\001.jpg
        public string EntryFullName => LoosePath.Combine(Archiver?.EntryFullName, EntryName);

        /// <summary>
        /// ルートアーカイバーを含むエントリ名
        /// </summary>
        /// a.zip\b.zip\c\001.jpg
        public string FullName => LoosePath.Combine(RootArchiver?.FullName, EntryFullName);


        /// <summary>
        /// 識別名
        /// アーカイブ内では重複名があるので登録番号を含めたユニークな名前にする
        /// </summary>
        public string Ident => LoosePath.Combine(Archiver.Ident, $"{Id}.{EntryName}");

        /// <summary>
        /// ファイルサイズ。
        /// -1 はディレクトリ
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// ファイル更新日
        /// </summary>
        public DateTime? LastWriteTime { get; set; }


        /// <summary>
        /// ファイルシステム所属判定
        /// </summary>
        public bool IsFileSystem => Archiver == null || Archiver.IsFileSystem;

        /// <summary>
        /// ファイルシステムでのパスを返す
        /// </summary>
        /// <returns>パス。圧縮ファイルの場合はnull</returns>
        public string GetFileSystemPath()
        {
            return Archiver != null
                ? Archiver.GetFileSystemPath(this)
                : EntryName;
        }




        /// <summary>
        /// ストリームを開く
        /// </summary>
        /// <returns>Stream</returns>
        public Stream OpenEntry()
        {
            return Archiver != null
                ? Archiver.OpenStream(this)
                : new FileStream(EntryName, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="exportFileName">出力ファイル名</param>
        /// <param name="isOverwrite">上書き許可フラグ</param>
        public void ExtractToFile(string exportFileName, bool isOverwrite)
        {
            if (Archiver != null)
            {
                Archiver.ExtractToFile(this, exportFileName, isOverwrite);
            }
            else
            {
                File.Copy(EntryName, exportFileName, isOverwrite);
            }
        }


        /// <summary>
        /// テンポラリにアーカイブを解凍する
        /// このテンポラリは自動的に削除される
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="isKeepFileName">エントリー名をファイル名にする</param>
        public FileProxy ExtractToTemp(bool isKeepFileName = false)
        {
            if (IsFileSystem)
            {
                return new FileProxy(GetFileSystemPath());
            }
            else
            {
                string tempFileName = isKeepFileName
                    ? Temporary.CreateTempFileName(LoosePath.GetFileName(EntryName))
                    : Temporary.CreateCountedTempFileName("entry", System.IO.Path.GetExtension(EntryName));
                ExtractToFile(tempFileName, false);
                return new TempFile(tempFileName);
            }
        }



        /// <summary>
        /// このエントリがアーカイブであるかを拡張子から判定
        /// </summary>
        /// <returns></returns>
        public bool IsArchive()
        {
            return ModelContext.ArchiverManager.IsSupported(EntryName);
        }

        /// <summary>
        /// このエントリが画像であるか拡張子から判定
        /// </summary>
        /// <returns></returns>
        public bool IsImage()
        {
            return ModelContext.BitmapLoaderManager.IsSupported(EntryName);
        }


        /// <summary>
        /// パスからエントリ作成
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ArchiveEntry Create(string path)
        {
            var entry = new ArchiveEntry();

            entry.EntryName = path;

            var directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists)
            {
                entry.Length = -1;
                entry.LastWriteTime = directoryInfo.LastWriteTime;
                return entry;
            }

            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                entry.Length = fileInfo.Length;
                entry.LastWriteTime = fileInfo.LastWriteTime;
                return entry;
            }

            entry.IsValid = false;
            return entry;
        }
    }
}

