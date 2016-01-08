// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Susie
{
    /// <summary>
    /// Susie アーカイブエントリ
    /// </summary>
    public class ArchiveEntry
    {
        // 対応するプラグイン
        private SusiePlugin _Spi;

        // エントリ情報(RAW)
        private ArchiveFileInfoRaw _Info;

        // アーカイブのショートファイル名
        // UNICODE対応のため、ショートファイル名でアクセスします
        public string ArchiveShortFileName { get; private set; }

        // エントリのパス名
        public string Path => _Info.path;

        // エントリ名
        public string FileName => _Info.filename;

        // 展開後ファイルサイズ
        public uint FileSize => _Info.filesize;

        // タイムスタンプ
        public DateTime TimeStamp => Time_T2DateTime(_Info.timestamp);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ArchiveEntry(SusiePlugin spi, string archiveFileName, ArchiveFileInfoRaw info)
        {
            _Spi = spi;
            ArchiveShortFileName = archiveFileName;
            _Info = info;
        }


        /// メモリに展開
        public byte[] Load()
        {
            lock (_Spi.Lock)
            {
                using (var api = _Spi.Open())
                {
                    return api.GetFile(ArchiveShortFileName, _Info);
                }
            }
        }


        /// ファイルに出力
        public void ExtractToFolder(string extractFolder)
        {
            lock (_Spi.Lock)
            {
                using (var api = _Spi.Open())
                {
                    int ret = api.GetFile(ArchiveShortFileName, _Info, extractFolder);
                    if (ret != 0) throw new System.IO.IOException("抽出に失敗しました");
                }
            }
        }

        // UNIX時間をDateTimeに変換
        private static DateTime Time_T2DateTime(uint time_t)
        {
            long win32FileTime = 10000000 * (long)time_t + 116444736000000000;
            return DateTime.FromFileTimeUtc(win32FileTime);
        }
    }


    /// <summary>
    /// Susie アーカイブエントリ リスト
    /// </summary>
    public class ArchiveEntryCollection : List<ArchiveEntry>
    {
        public ArchiveEntryCollection(SusiePlugin spi, string archiveFileName, List<ArchiveFileInfoRaw> entries)
        {
            string shortPath = Win32Api.GetShortPathName(archiveFileName);
            foreach (var entry in entries)
            {
                this.Add(new ArchiveEntry(spi, shortPath, entry));
            }
        }
    }

}
