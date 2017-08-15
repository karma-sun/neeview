// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using SevenZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 事前にテンポラリフォルダーに展開してアクセスするアーカイバー
    /// </summary>
    public class SevenZipExtractArchiver : Archiver
    {
        #region Fields

        private ArchiveEntry _source;
        private string _temp;

        #endregion

        #region Constructors

        //
        public SevenZipExtractArchiver(string path, ArchiveEntry source) : base(path, source)
        {
            SevenZipArchiver.InitializeLibrary();
            _source = source ?? ArchiveEntry.Create(path);
        }

        #endregion

        #region Properties

        public override string ToString() => "7zip.dll extractor";

        //
        public override bool IsFileSystem { get; } = true;

        #endregion

        #region Methods

        // リスト取得
        public override List<ArchiveEntry> GetEntries(CancellationToken token)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            Open(token);

            token.ThrowIfCancellationRequested();

            var list = new List<ArchiveEntry>();
            var directoryEntries = new List<ArchiveEntry>();

            using (var extractor = new SevenZipExtractor(this.Path))
            {
                foreach (var entry in extractor.ArchiveFileData)
                {
                    token.ThrowIfCancellationRequested();

                    var archiveEntry = new ArchiveEntry()
                    {
                        Archiver = this,
                        Id = entry.Index,
                        EntryName = entry.FileName,
                        Length = (long)entry.Size,
                        LastWriteTime = entry.LastWriteTime,
                        Instance = System.IO.Path.Combine(_temp, entry.GetTempFileName())
                    };

                    if (!entry.IsDirectory)
                    {
                        list.Add(archiveEntry);
                    }
                    else
                    {
                        archiveEntry.Length = -1;
                        archiveEntry.Instance = null;
                        directoryEntries.Add(archiveEntry);
                    }
                }

                // 空ディレクトリー追加
                if (BookProfile.Current.IsEnableNoSupportFile)
                {
                    list.AddDirectoryEntries(directoryEntries);
                }
            }

            return list;
        }

        //
        public override bool IsSupported()
        {
            return true;
        }

        // ファイルパス取得
        public override string GetFileSystemPath(ArchiveEntry entry)
        {
            return (string)entry.Instance;
        }

        //
        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            return new FileStream(GetFileSystemPath(entry), FileMode.Open, FileAccess.Read);
        }

        //
        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            File.Copy(GetFileSystemPath(entry), exportFileName, isOverwrite);
        }

        //
        private void Open(CancellationToken token)
        {
            if (IsDisposed || _temp != null) return;

            // make temp folder
            var directory = Temporary.CreateCountedTempFileName("arc", "");

            ////var sw = Stopwatch.StartNew();

            using (var extractor = new SevenZipExtractor(this.Path))
            {
                extractor.ExtractArchiveTemp(directory);
            }

            ////sw.Stop();
            ////Debug.WriteLine($"Extract: {sw.ElapsedMilliseconds}ms");

            _temp = directory;
        }

        //
        private void Close()
        {
            if (_temp == null) return;

            try
            {
                if (Directory.Exists(_temp))
                {
                    Directory.Delete(_temp, true);
                }
            }
            catch
            {
                // nop.
            }

            _temp = null;
        }

        #endregion

        #region IDisposable Support

        private bool _isDisposed = false; // 重複する呼び出しを検出するには

        public override bool IsDisposed => _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // マネージ状態を破棄します (マネージ オブジェクト)。
                }

                // アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // 大きなフィールドを null に設定します。
                Close();

                _isDisposed = true;
            }
        }

        // 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~SevenZipExtractArchiver()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public override void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
