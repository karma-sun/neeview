// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
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
    public class ArchiveEntry : IDisposable
    {
        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        public ArchiveEntry()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="path">初期パス</param>
        public ArchiveEntry(string path)
        {
            this.RawEntryName = path;

            try
            {
                var directoryInfo = new DirectoryInfo(path);
                if (directoryInfo.Exists)
                {
                    this.Length = -1;
                    this.LastWriteTime = directoryInfo.LastWriteTime;
                    return;
                }

                var fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    this.Length = fileInfo.Length;
                    this.LastWriteTime = fileInfo.LastWriteTime;
                    return;
                }
            }
            catch
            {
                // 不正なパスが含まれていると通常のファイルシステムでは対応できない。
                // アーカイブパスの可能性がある。
            }

            // 実在するパスではない
            this.IsValid = false;

            // アーカイブパスの場合、ファイル情報は親アーカイブのものにする
            var parent = ArchiverManager.Current.GetExistPathName(path);
            if (parent != null)
            {
                var parentFileInfo = new FileInfo(parent);
                this.Length = parentFileInfo.Length;
                this.LastWriteTime = parentFileInfo.LastWriteTime;
                this.IsArchivePath = true;
            }
        }

        #endregion

        #region Properties

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
        /// パスが有効であるか
        /// 無効である場合はアーカイブパスである可能性あり
        /// </summary>
        public bool IsValid { get; private set; } = true;

        /// <summary>
        /// アーカイブパスであるか
        /// </summary>
        public bool IsArchivePath { get; private set; }

        // 例：
        // a.zip 
        // +- b.zip
        //      +- c\001.jpg <- this!

        /// <summary>
        /// エントリ名(重複有)
        /// </summary>
        /// c\001.jpg
        private string _rawEntryName;
        public string RawEntryName
        {
            get { return _rawEntryName; }
            set
            {
                if (_rawEntryName != value)
                {
                    _rawEntryName = value;
                    this.EntryName = LoosePath.NormalizeSeparator(_rawEntryName);
                }
            }
        }

        /// <summary>
        /// エントリ名(重複有、正規化)
        /// </summary>
        /// c/001.jpg => c\001.jpg
        public string EntryName { get; private set; }

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
        /// エクスプローラーから指定可能なパス
        /// </summary>
        public string FullPath => LoosePath.Combine(RootArchiver?.FullPath, EntryFullName);

        /// <summary>
        /// 識別名
        /// アーカイブ内では重複名があるので登録番号を含めたユニークな名前にする
        /// </summary>
        public string Ident => LoosePath.Combine(Archiver?.Ident, $"{Id}.{EntryName}");

        /// <summary>
        /// ファイルサイズ。
        /// -1 はディレクトリ
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// ディレクトリ？
        /// </summary>
        public bool IsDirectory => Length == -1;

        /// <summary>
        /// ファイル更新日
        /// </summary>
        public DateTime? LastWriteTime { get; set; }
        
        /// <summary>
        /// ファイルシステム所属判定
        /// </summary>
        public bool IsFileSystem => Archiver == null || Archiver.IsFileSystem;


        #endregion Properties

        #region Methods

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
        /// 本来のファイルシステムでのパスを返す
        /// </summary>
        /// <returns></returns>
        public string GetSourceFileSystemPath()
        {
            if (this.Archiver == null)
            {
                return this.EntryName;
            }

            // 最初のアーカイブパスを取得
            return this.Archiver.GetSourceFileSystemPath();
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
            if (this.Archiver is FolderArchive)
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
        /// このエントリがアーカイブであるかを拡張子から判定。
        /// ファイルシステムを含む
        /// </summary>
        /// <returns></returns>
        public bool IsArchive()
        {
            bool isAllowFileSystem = this.Archiver == null || this.Archiver.IsFileSystem;
            return ArchiverManager.Current.IsSupported(EntryName, isAllowFileSystem);
        }

        /// <summary>
        /// このエントリがアーカイブであるかを拡張子から判定
        /// ファイルシステムは除外
        /// </summary>
        /// <returns></returns>
        public bool IsArchiveFile()
        {
            bool isAllowFileSystem = this.Archiver == null;
            return ArchiverManager.Current.IsSupported(EntryName, isAllowFileSystem);
        }

        /// <summary>
        /// このエントリが画像であるか拡張子から判定
        /// </summary>
        /// <returns></returns>
        public bool IsImage()
        {
            return PictureProfile.Current.IsSupported(this.EntryName);
        }

        /// <summary>
        /// 関連するArchiverをDisposeする
        /// </summary>
        private void DisporeArchivers()
        {
            for (var archiver = this.Archiver; archiver != null; archiver = archiver.Parent)
            {
                archiver.Dispose();
            }
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // マネージ状態を破棄します (マネージ オブジェクト)。
                    DisporeArchivers();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~ArchiveEntry() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion

    }


    /// <summary>
    /// ArchiveEntryコレクション拡張
    /// </summary>
    public static class ArchiveEntryCollectionExtensions
    {
        /// <summary>
        /// ArchiveEntryコレクションから指定のArchiveEntryを取得する
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ArchiveEntry GetEntry(this IEnumerable<ArchiveEntry> entries, string path)
        {
            path = LoosePath.NormalizeSeparator(path);
            return entries.FirstOrDefault(e => e.EntryName == path);
        }
    }

}

