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
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：Susieアーカイバー
    /// </summary>
    public class SusieArchiver : Archiver
    {
        public override string ToString()
        {
            return _susiePlugin.Name ?? "(none)";
        }

        public static bool IsEnable { get; set; }

        private Susie.SusiePlugin _susiePlugin;

        private object _lock = new object();

        private bool _isDisposed;

        // コンストラクタ
        public SusieArchiver(string path, ArchiveEntry source) : base(path, source)
        {
        }

        //
        public override bool IsDisposed => _isDisposed;

        //
        public override void Dispose()
        {
            _isDisposed = true;
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
            if (_susiePlugin == null)
            {
                _susiePlugin = SusieContext.Current.Susie?.GetArchivePlugin(Path, true);
            }
            return _susiePlugin;
        }

        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries(CancellationToken token)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            token.ThrowIfCancellationRequested();

            var plugin = GetPlugin();
            if (plugin == null) throw new NotSupportedException($"not archive: {Path}");

            var infoCollection = plugin.GetArchiveInfo(Path);
            if (infoCollection == null) throw new NotSupportedException();

            var list = new List<ArchiveEntry>();
            for (int id = 0; id < infoCollection.Count; ++id)
            {
                token.ThrowIfCancellationRequested();

                var entry = infoCollection[id];
                if (entry.FileSize > 0)
                {
                    string name = (entry.Path.TrimEnd('\\', '/') + "\\" + entry.FileName).TrimStart('\\', '/');
                    list.Add(new ArchiveEntry()
                    {
                        Archiver = this,
                        Id = id,
                        EntryName = name,
                        Length = entry.FileSize,
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
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            lock (_lock)
            {
                var info = (Susie.ArchiveEntry)entry.Instance;
                byte[] buffer = info.Load();
                return new MemoryStream(buffer, 0, buffer.Length, false, true);
            }
        }


        // ファイルに出力する
        public override void ExtractToFile(ArchiveEntry entry, string extractFileName, bool isOverwrite)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            var info = (Susie.ArchiveEntry)entry.Instance;

            string tempDirectory = Temporary.CreateCountedTempFileName("susie", "");

            try
            {
                // susieプラグインでは出力ファイル名を指定できないので、
                // テンポラリフォルダーに出力してから移動する
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
