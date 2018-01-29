// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイブ内パスを含むパス記述に対応した処理
    /// </summary>
    public static class ArchiveFileSystem
    {
        /// <summary>
        /// パスからArcvhiveEntryを作成
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<ArchiveEntry> CreateArchiveEntry(string path, CancellationToken token)
        {
            // システムパスはそのまま
            if (File.Exists(path) || Directory.Exists(path))
            {
                return new ArchiveEntry(path);
            }
            // アーカイブパスはそのエントリーを返す
            else
            {
                try
                {
                    var archivePath = ArchiverManager.Current.GetExistPathName(path) ?? throw new FileNotFoundException();
                    var entryName = path.Substring(archivePath.Length + 1);
                    var archiver = ArchiverManager.Current.CreateArchiver(archivePath, false);
                    ////Debug.WriteLine($"Create Archiver: {archiver.FullPath}");
                    return await CreateInnerArchiveEntry(archiver, entryName, token);
                }
                catch (FileNotFoundException)
                {
                    throw new FileNotFoundException($"\"{path}\" が見つかりません");
                }
            }
        }

        /// <summary>
        /// アーカイブ内のエントリーを返す。
        /// 入れ子になったアーカイブの場合、再帰処理する。
        /// </summary>
        /// <param name="archiver"></param>
        /// <param name="entryName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task<ArchiveEntry> CreateInnerArchiveEntry(Archiver archiver, string entryName, CancellationToken token)
        {
            var entries = await archiver.GetEntriesAsync(token);

            var entry = entries.GetEntry(entryName);
            if (entry != null) return entry;

            // 書庫内書庫の検証
            var path = entryName;

            while (true)
            {
                path = LoosePath.GetDirectoryName(path);
                if (string.IsNullOrEmpty(path)) throw new FileNotFoundException();

                entry = entries.GetEntry(path);
                if (entry != null)
                {
                    var subArchiver = await ArchiverManager.Current.CreateArchiverAsync(entry, false, token);
                    ////Debug.WriteLine($"Create Archiver: {subArchiver.FullPath}");
                    var subEntryName = entryName.Substring(entry.RawEntryName.Length + 1);
                    return await CreateInnerArchiveEntry(subArchiver, subEntryName, token);
                }
            }
        }

        /// <summary>
        /// アーカイブパスの存在チェック
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> ExistsAsync(string path, CancellationToken token)
        {
            try
            {
                using (var entry = await CreateArchiveEntry(path, token))
                {
                    return entry != null;
                }
            }
            catch(FileNotFoundException)
            {
                return false;
            }
        }


        /// <summary>
        /// 実在するディレクトリまで遡る
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetExistDirectoryName(string path)
        {
            if (Directory.Exists(path))
            {
                return path;
            }

            while (path != null)
            {
                path = LoosePath.GetDirectoryName(path);
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

    }

}
