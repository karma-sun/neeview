using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// TODO: 書庫内書庫 ストリームによる多重展開が可能？

namespace NeeView
{
    /// <summary>
    /// アーカイバー：標準Zipアーカイバー
    /// </summary>
    public class ZipArchiver : Archiver
    {
        #region Constructors

        public ZipArchiver(string path, ArchiveEntry source) : base(path, source)
        {
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return ".NET ZipArchiver";
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        /// <summary>
        /// ZIPヘッダチェック
        /// </summary>
        /// <returns></returns>
        private bool CheckSignature(Stream stream)
        {
            var pos = stream.Position;

            byte[] signature = new byte[4];
            stream.Read(signature, 0, 4);
            stream.Seek(pos, SeekOrigin.Begin);

            return (BitConverter.ToString(signature, 0) == "50-4B-03-04");
        }


        // エントリーリストを得る
        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            var list = new List<ArchiveEntry>();
            var directories = new List<ArchiveEntry>();

            FileStream stream = null;
            try
            {
                stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);

                // ヘッダチェック
                if (!CheckSignature(stream))
                {
                    throw new FormatException(string.Format(Properties.Resources.NotZipException_Message, Path));
                }

                // エントリー取得
                using (var archiver = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    stream = null;

                    for (int id = 0; id < archiver.Entries.Count; ++id)
                    {
                        token.ThrowIfCancellationRequested();

                        var entry = archiver.Entries[id];

                        var archiveEntry = new ArchiveEntry()
                        {
                            IsValid = true,
                            Archiver = this,
                            Id = id,
                            Instance = null,
                            RawEntryName = entry.FullName,
                            Length = entry.Length,
                            LastWriteTime = entry.LastWriteTime.LocalDateTime,
                        };

                        if (!entry.IsDirectory())
                        {
                            list.Add(archiveEntry);
                        }
                        else
                        {
                            archiveEntry.Length = -1;
                            directories.Add(archiveEntry);
                        }
                    }

                    // ディレクトリエントリを追加
                    list.AddRange(CreateDirectoryEntries(list.Concat(directories)));
                }
            }
            finally
            {
                stream?.Dispose();
            }

            await Task.CompletedTask;
            return list;
        }

        // エントリーのストリームを得る
        protected override Stream OpenStreamInner(ArchiveEntry entry)
        {
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            using (var archiver = ZipFile.OpenRead(Path))
            {
                ZipArchiveEntry archiveEntry = archiver.Entries[entry.Id];
                if (archiveEntry.FullName != entry.RawEntryName)
                {
                    throw new ApplicationException(Properties.Resources.InconsistencyException_Message);
                }

                using (var stream = archiveEntry.Open())
                {
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms;
                }
            }
        }

        //
        protected override void ExtractToFileInner(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            using (var archiver = ZipFile.OpenRead(Path))
            {
                ZipArchiveEntry archiveEntry = archiver.Entries[entry.Id];
                archiveEntry.ExtractToFile(exportFileName, isOverwrite);
            }
        }

        #endregion
    }

    //
    public static class ZipArchiveEntryExtension
    {
        public static bool IsDirectory(this ZipArchiveEntry self)
        {
            var last = self.FullName.Last();
            return (self.Name == "" && (last == '\\' || last == '/'));
        }
    }
}
