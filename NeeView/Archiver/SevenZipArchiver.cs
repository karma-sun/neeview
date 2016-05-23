// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using SevenZip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class SevenZipSource : IDisposable
    {
        public static bool IsFileLocked { get; set; }

        private SevenZipExtractor _Extractor;

        private string _FileName;
        private Stream _Stream;

        public bool IsStream => _Stream != null;

        private object _Lock;

        //
        public SevenZipSource(string fileName, object lockObject)
        {
            _FileName = fileName;
            _Lock = lockObject ?? new object();
        }

        //
        public SevenZipSource(Stream stream, object lockObject)
        {
            _Stream = stream;
            _Lock = lockObject ?? new object();
        }

        //
        public SevenZipExtractor Open()
        {
            if (_Extractor == null)
            {
                _Extractor = IsStream ? new SevenZipExtractor(_Stream) : new SevenZipExtractor(_FileName);
            }

            return _Extractor;
        }

        //
        public void Close(bool isForce = false)
        {
            if (_Extractor != null && (isForce || !IsFileLocked))
            {
                _Extractor.Dispose();
                _Extractor = null;
            }
        }

        public void Dispose()
        {
            lock (_Lock)
            {
                Close(true);
                _Stream = null;
            }
        }
    }

    public class SevenZipDescriptor : IDisposable
    {
        private SevenZipSource _Source;

        private SevenZipExtractor _Extractor;

        public SevenZipDescriptor(SevenZipSource source)
        {
            _Source = source;
            _Extractor = _Source.Open();
        }

        public ReadOnlyCollection<ArchiveFileInfo> ArchiveFileData
        {
            get { return _Extractor.ArchiveFileData; }
        }

        public void ExtractFile(int index, Stream extractStream)
        {
            _Extractor.ExtractFile(index, extractStream);
        }

        public void Dispose()
        {

            _Source.Close();
            _Extractor = null;
        }
    }


    /// <summary>
    /// アーカイバ：7z.dll
    /// </summary>
    public class SevenZipArchiver : Archiver
    {
        public override string ToString()
        {
            return "7zip.dll";
        }

        private static object _Lock = new object();

        //
        static SevenZipArchiver()
        {
            SevenZipExtractor.SetLibraryPath("7z.dll");
            //var features = SevenZip.SevenZipExtractor.CurrentLibraryFeatures;
            //Console.WriteLine(((uint)features).ToString("X6"));
        }

        private SevenZipSource _Source;
        private Stream _Stream;

        private bool _IsDisposed;


        //
        public SevenZipArchiver(string archiveFileName, Stream stream)
        {
            FileName = archiveFileName;
            _Stream = stream;

            _Source = _Stream != null ? new SevenZipSource(_Stream, _Lock) : new SevenZipSource(FileName, _Lock);
        }


        // 廃棄処理
        public override void Dispose()
        {
            _IsDisposed = true;

            _Source?.Dispose();
            _Source = null;
            _Stream?.Dispose();
            _Stream = null;
            base.Dispose();
        }


        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries()
        {
            if (_IsDisposed) throw new ApplicationException("Archive already colosed.");

            var list = new List<ArchiveEntry>();

            lock (_Lock)
            {
                using (var extractor = new SevenZipDescriptor(_Source))
                {
                    for (int id = 0; id < extractor.ArchiveFileData.Count; ++id)
                    {
                        var entry = extractor.ArchiveFileData[id];
                        if (!entry.IsDirectory)
                        {
                            list.Add(new ArchiveEntry()
                            {
                                Archiver = this,
                                Id = id,
                                EntryName = entry.FileName,
                                FileSize = (long)entry.Size,
                                LastWriteTime = entry.LastWriteTime,
                            });
                        }
                    }
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
                using (var extractor = new SevenZipDescriptor(_Source))
                {
                    var archiveEntry = extractor.ArchiveFileData[entry.Id];
                    if (archiveEntry.FileName != entry.EntryName)
                    {
                        throw new ApplicationException("ページデータの不整合");
                    }

                    var ms = new MemoryStream();
                    extractor.ExtractFile(entry.Id, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms;
                }
            }
        }


        // ファイルに出力
        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (_IsDisposed) throw new ApplicationException("Archive already colosed.");

            lock (_Lock)
            {
                using (var extractor = new SevenZipExtractor(FileName)) // 専用extractor
                using (Stream fs = new FileStream(exportFileName, FileMode.Create, FileAccess.Write))
                {
                    extractor.ExtractFile(entry.Id, fs);
                }
            }
        }

    }
}
