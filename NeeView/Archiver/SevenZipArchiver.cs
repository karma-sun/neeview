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

        private SevenZipExtractor _extractor;

        private string _fileName;
        private Stream _stream;

        public bool IsStream => _stream != null;

        private object _lock;

        //
        public SevenZipSource(string fileName, object lockObject)
        {
            _fileName = fileName;
            _lock = lockObject ?? new object();
        }

        //
        public SevenZipSource(Stream stream, object lockObject)
        {
            _stream = stream;
            _lock = lockObject ?? new object();
        }

        //
        public SevenZipExtractor Open()
        {
            if (_extractor == null)
            {
                _extractor = IsStream ? new SevenZipExtractor(_stream) : new SevenZipExtractor(_fileName);
            }

            return _extractor;
        }

        //
        public void Close(bool isForce = false)
        {
            if (_extractor != null && (isForce || !IsFileLocked))
            {
                _extractor.Dispose();
                _extractor = null;
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                Close(true);
                _stream = null;
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



        public static string DllPath { get; set; }

        private static bool _isLibraryInitialized;

        //
        private static void InitializeLibrary()
        {
            if (_isLibraryInitialized) return;

            var dllPath = string.IsNullOrWhiteSpace(DllPath) ? Path.Combine(App.Config.LibrariesPath, "7z.dll") : DllPath;

#if DEBUG
            // 開発中はLibrariesパスが存在しないので、カレントに設定しなおす
            if (!File.Exists(dllPath))
            {
                dllPath = Path.Combine(App.Config.AssemblyLocation, "7z.dll");
            }
#endif

            SevenZipExtractor.SetLibraryPath(dllPath);

            FileVersionInfo dllVersionInfo = FileVersionInfo.GetVersionInfo(dllPath);
            Debug.WriteLine("7z.dll: ver" + dllVersionInfo?.FileVersion);

            _isLibraryInitialized = true;
        }


        private static object _lock = new object();



        private SevenZipSource _Source;
        private Stream _Stream;

        private bool _IsDisposed;


        //
        public SevenZipArchiver(string archiveFileName, Stream stream)
        {
            InitializeLibrary();

            FileName = archiveFileName;
            _Stream = stream;

            _Source = _Stream != null ? new SevenZipSource(_Stream, _lock) : new SevenZipSource(FileName, _lock);
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

            lock (_lock)
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

            lock (_lock)
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

            lock (_lock)
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
