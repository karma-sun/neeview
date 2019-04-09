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
        #region Fields

        private Susie.SusiePlugin _susiePlugin;
        private object _lock = new object();

        #endregion

        #region Constructors

        public SusieArchiver(string path, ArchiveEntry source) : base(path, source)
        {
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return _susiePlugin.Name ?? "(none)";
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
        protected override List<ArchiveEntry> GetEntriesInner(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var plugin = GetPlugin();
            if (plugin == null) throw new NotSupportedException($"not archive: {Path}");

            var infoCollection = plugin.GetArchiveInfo(Path);
            if (infoCollection == null) throw new NotSupportedException();

            var list = new List<ArchiveEntry>();
            var directories = new List<ArchiveEntry>();

            for (int id = 0; id < infoCollection.Count; ++id)
            {
                token.ThrowIfCancellationRequested();

                var entry = infoCollection[id];

                var archiveEntry = new ArchiveEntry()
                {
                    IsValid = true,
                    Archiver = this,
                    Id = id,
                    RawEntryName = (entry.Path.TrimEnd('\\', '/') + "\\" + entry.FileName).TrimStart('\\', '/'),
                    Length = entry.FileSize,
                    LastWriteTime = entry.TimeStamp,
                    Instance = entry,
                };

                if (!entry.IsDirectory)
                {
                    list.Add(archiveEntry);
                }
                else
                {
                    archiveEntry.Length = -1;
                    directories.Add(archiveEntry);
                }
            }

            // NOTE: サイズ0であり、他のエントリ名のパスを含む場合はディレクトリとみなし除外する。
            list = list.Where(entry => entry.Length > 0 || list.All(e => e == entry || !e.EntryName.StartsWith(LoosePath.TrimDirectoryEnd(entry.EntryName)))).ToList();

            // ディレクトリエントリを追加
            list.AddRange(CreateDirectoryEntries(list.Concat(directories)));

            return list;
        }


        // エントリーのストリームを得る
        protected override Stream OpenStreamInner(ArchiveEntry entry)
        {
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            lock (_lock)
            {
                var info = (Susie.ArchiveEntry)entry.Instance;
                byte[] buffer = info.Load();
                return new MemoryStream(buffer, 0, buffer.Length, false, true);
            }
        }


        // ファイルに出力する
        protected override void ExtractToFileInner(ArchiveEntry entry, string extractFileName, bool isOverwrite)
        {
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            var info = (Susie.ArchiveEntry)entry.Instance;

            // 16MB以上のエントリは直接ファイル出力を試みる
            if (entry.Length > 16 * 1024 * 1024)
            {
                string tempDirectory = Temporary.Current.CreateCountedTempFileName("susie", "");

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

                    return;
                }

                // 失敗したら：メモリ展開からのファイル保存を行う
                catch (Susie.SpiException e)
                {
                    Debug.WriteLine(e.Message);
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

            // メモリ展開からのファイル保存
            info.ExtractToFile(extractFileName);
        }

        /// <summary>
        /// 事前展開？
        /// </summary>
        public override bool CanPreExtract(CancellationToken token)
        {
            // NOTE: Susieプラグインの場合、サイズに関係なくプラグインに設定されたフラグでのみ判定
            var spi = GetPlugin();
            return spi != null ? spi.IsPreExtract : false;
        }

        #endregion
    }
}
