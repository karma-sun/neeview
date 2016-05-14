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

        private object _Lock;

        //
        public SevenZipSource(string fileName, object lockObject)
        {
            _FileName = fileName;
            _Lock = lockObject ?? new object();
        }

        //
        public SevenZipExtractor Open()
        {
            if (_Extractor == null)
            {
                _Extractor = new SevenZipExtractor(_FileName);
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

        public void ExtractFile(int index, Stream stream)
        {
            _Extractor.ExtractFile(index, stream);
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


        // コンストラクタ
        public SevenZipArchiver(string archiveFileName)
        {
            FileName = archiveFileName;

            //
            _Source = new SevenZipSource(FileName, _Lock);
            this.TrashBox.Add(_Source);
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries()
        {
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
            lock (_Lock)
            {
                using (var extractor = new SevenZipDescriptor(_Source))
                using (Stream fs = new FileStream(exportFileName, FileMode.Create, FileAccess.Write))
                {
                    extractor.ExtractFile(entry.Id, fs);
                }
            }
        }

    }
}
