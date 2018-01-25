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
    public class BookAddress
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
        }

        #endregion
    }

}
