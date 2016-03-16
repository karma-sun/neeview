// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバ：Susieアーカイバ
    /// </summary>
    public class SusieArchiver : Archiver
    {
        public override string ToString()
        {
            return _SusiePlugin.Name ?? "(none)";
        }

        public static bool IsEnable { get; set; }

        private string _ArchiveFileName;
        public override string FileName => _ArchiveFileName;

        private Susie.SusiePlugin _SusiePlugin;
        Dictionary<string, Susie.ArchiveEntry> _ArchiveFileInfoDictionary;

        private object _Lock = new object();


        // コンストラクタ
        public SusieArchiver(string archiveFileName)
        {
            _ArchiveFileName = archiveFileName;
        }

        // サポート判定
        public override bool IsSupported()
        {
            return GetPlugin() != null;
        }

        // 対応プラグイン取得
        public Susie.SusiePlugin GetPlugin()
        {
            if (_SusiePlugin == null)
            {
                _SusiePlugin = ModelContext.Susie?.GetArchivePlugin(_ArchiveFileName);
            }
            return _SusiePlugin;
        }

        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries()
        {
            var plugin = GetPlugin();
            if (plugin == null) throw new NotSupportedException();

            var infoCollection = plugin.GetArchiveInfo(_ArchiveFileName);
            if (infoCollection == null) throw new NotSupportedException();

            _ArchiveFileInfoDictionary = new Dictionary<string, Susie.ArchiveEntry>();
            List<ArchiveEntry> entries = new List<ArchiveEntry>();
            foreach (var entry in infoCollection)
            {
                try
                {
                    string name = (entry.Path.TrimEnd('\\', '/') + "\\" + entry.FileName).TrimStart('\\', '/');

                    entries.Add(new ArchiveEntry()
                    {
                        FileName = name, //  entry.FileName,
                        UpdateTime = entry.TimeStamp,
                    });

                    _ArchiveFileInfoDictionary.Add(name, entry);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

            }

            return entries;
        }


        // エントリーのストリームを得る
        public override Stream OpenEntry(string entryName)
        {
            lock (_Lock)
            {
                var info = _ArchiveFileInfoDictionary[entryName];
                byte[] buffer = info.Load();
                return new MemoryStream(buffer, 0, buffer.Length, false, true);
            }
        }


        // ファイルに出力する
        public override void ExtractToFile(string entryName, string extractFileName, bool isOverwrite)
        {
            var info = _ArchiveFileInfoDictionary[entryName];

            string tempDirectory = Temporary.CreateCountedTempFileName("Susie", "");

            try
            {
                // susieプラグインでは出力ファイル名を指定できないので、
                // テンポラリフォルダに出力してから移動する
                Directory.CreateDirectory(tempDirectory);

                // 注意：失敗することがよくある
                info.ExtractToFolder(tempDirectory);

                // 上書き時は移動前に削除
                if (isOverwrite && File.Exists(extractFileName))
                {
                    File.Delete(extractFileName);
                }

                var files = Directory.GetFiles(tempDirectory);
                File.Move(files[0], extractFileName);
                Directory.Delete(tempDirectory, true);
            }

            // 失敗したら：メモリ展開からのファイル保存を行う
            catch (Susie.SpiException e)
            {
                Debug.WriteLine(e.Message);
                info.ExtractToFile(extractFileName);
                return;
            }

            // 後始末
            finally
            {
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }
    }
}
