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
    public class SpiException : ApplicationException
    {
        public SpiException(string msg, SusiePlugin spi) : base($"[{System.IO.Path.GetFileName(spi.FileName)}] {msg}")
        {
        }
    }


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
                    var buff = api.GetFile(ArchiveShortFileName, _Info);
                    if (buff == null) throw new SpiException("抽出に失敗しました(M)", _Spi);
                    return buff;
                }
            }
        }

        /// フォルダに出力。ファイル名は変更しない
        public void ExtractToFolder(string extractFolder)
        {
            lock (_Spi.Lock)
            {
                using (var api = _Spi.Open())
                {
                    int ret = api.GetFile(ArchiveShortFileName, _Info, extractFolder);
                    if (ret != 0) throw new SpiException($"抽出に失敗しました(F)", _Spi);
                }
            }
        }

        /// ファイルに出力
        public void ExtractToFile(string extract)
        {
            using (var ms = new System.IO.MemoryStream(Load(), false))
            using (var stream = new System.IO.FileStream(extract, System.IO.FileMode.Create))
            {
                ms.WriteTo(stream);
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
        public SusiePlugin SusiePlugin { get; set; }

        public ArchiveEntryCollection(SusiePlugin spi, string archiveFileName, List<ArchiveFileInfoRaw> entries)
        {
            SusiePlugin = spi;

            string shortPath = Win32Api.GetShortPathName(archiveFileName);
            foreach (var entry in entries)
            {
                this.Add(new ArchiveEntry(spi, shortPath, entry));
            }
        }
    }

}
