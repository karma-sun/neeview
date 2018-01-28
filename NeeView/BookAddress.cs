// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイブパスに対応したブックアドレス
    /// </summary>
    public class BookAddress : IDisposable
    {
        #region Properties

        /// <summary>
        /// ブックのアーカイバ
        /// </summary>
        public Archiver Archiver { get; private set; }

        /// <summary>
        /// 開始ページ名
        /// </summary>
        public string EntryName { get; set; }

        /// <summary>
        /// ブックの場所
        /// </summary>
        public string Place => Archiver.FullPath;

        /// <summary>
        /// ページを含めたアーカイブパス
        /// </summary>
        public string FullPath => LoosePath.Combine(Place, EntryName);

        #endregion

        #region Methods

        /// <summary>
        /// 初期化(必須)。
        /// アーカイブ展開等を含むため、非同期処理。
        /// TODO: 自動再帰の場合のEntryName
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task InitializeAsync(string path, CancellationToken token)
        {
            var entry = await ArchiveFileSystem.CreateArchiveEntry(path, token);
            if (ArchiverManager.Current.IsSupported(entry.FullPath))
            {
                this.Archiver = await ArchiverManager.Current.CreateArchiverAsync(entry, false, token);
                this.EntryName = null;
            }
            else
            {
                this.Archiver = entry.Archiver ?? new FolderArchive(Path.GetDirectoryName(entry.FullPath), null);
                this.EntryName = entry.EntryName;
            }

            // このアーカイブをROOTとする
            this.Archiver.RootFlag = true;
        }

        /// <summary>
        /// 使用しているアーカイバを破棄
        /// </summary>
        private void Terminate()
        {
            var archiver = this.Archiver;
            while(archiver != null)
            {
                var parent = archiver.Parent;
                archiver.Dispose();
                archiver = parent;
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // マネージ状態を破棄します (マネージ オブジェクト)。
                    Terminate();
                }

                _disposedValue = true;
            }
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
        }
        #endregion

        #endregion
    }

}
