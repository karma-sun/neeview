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

        private Susie.SusiePlugin _SusiePlugin;

        private object _Lock = new object();

        private bool _IsDisposed;

        // コンストラクタ
        public SusieArchiver(string archiveFileName)
        {
            FileName = archiveFileName;
        }

        //
        public override void Dispose()
        {
            _IsDisposed = true;
            base.Dispose();
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
                _SusiePlugin = ModelContext.Susie?.GetArchivePlugin(FileName, true);
            }
            return _SusiePlugin;
        }

        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries()
        {
            if (_IsDisposed) throw new ApplicationException("Archive already colosed.");

            var plugin = GetPlugin();
            if (plugin == null) throw new NotSupportedException();

            var infoCollection = plugin.GetArchiveInfo(FileName);
            if (infoCollection == null) throw new NotSupportedException();

            var list = new List<ArchiveEntry>();
            for (int id = 0; id < infoCollection.Count; ++id)
            {
                var entry = infoCollection[id];
                if (entry.FileSize > 0)
                {
                    string name = (entry.Path.TrimEnd('\\', '/') + "\\" + entry.FileName).TrimStart('\\', '/');
                    list.Add(new ArchiveEntry()
                    {
                        Archiver = this,
                        Id = id,
                        EntryName = name,
                        FileSize = entry.FileSize,
                        LastWriteTime = entry.TimeStamp,
                        Instance = entry,
                    });
                }
            }

            return list;
        }


        // エントリーのストリームを得る
        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (_IsDisposed) throw new ApplicationException("Archive already colosed.");

            lock (_Lock)
            {
                var info = (Susie.ArchiveEntry)entry.Instance;
                byte[] buffer = info.Load();
                return new MemoryStream(buffer, 0, buffer.Length, false, true);
            }
        }


        // ファイルに出力する
        public override void ExtractToFile(ArchiveEntry entry, string extractFileName, bool isOverwrite)
        {
            if (_IsDisposed) throw new ApplicationException("Archive already colosed.");

            var info = (Susie.ArchiveEntry)entry.Instance;

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
