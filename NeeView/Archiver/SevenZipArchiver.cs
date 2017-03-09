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
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class SevenZipSource : IDisposable
    {
        public static double LockTime { get; set; } = -1.0;

        private SevenZipExtractor _extractor;

        private string _fileName;
        private Stream _stream;

        public bool IsStream => _stream != null;

        private object _lock;

        private DelayAction _delayClose;

        //
        public SevenZipSource(string fileName, object lockObject)
        {
            _fileName = fileName;
            _lock = lockObject ?? new object();
            Initialize();
        }

        //
        public SevenZipSource(Stream stream, object lockObject)
        {
            _stream = stream;
            _lock = lockObject ?? new object();
            Initialize();
        }

        //
        private void Initialize()
        {
            if (LockTime > 0)
            {
                _delayClose = new DelayAction(App.Current.Dispatcher, TimeSpan.FromSeconds(0.5), DelayClose, TimeSpan.FromSeconds(LockTime));
            }
        }

        //
        private void DelayClose()
        {
            lock (_lock)
            {
                Close(true);
            }
        }


        //
        public SevenZipExtractor Open()
        {
            _delayClose?.Cancel();

            if (_extractor == null)
            {
                _extractor = IsStream ? new SevenZipExtractor(_stream) : new SevenZipExtractor(_fileName);
            }

            return _extractor;
        }

        //
        public void Close(bool isForce = false)
        {
            if (_extractor != null)
            {
                if (isForce || LockTime == 0)
                {
                    _extractor.Dispose();
                    _extractor = null;
                }
                else if (_delayClose != null)
                {
                    _delayClose.Request();
                }
            }
        }

        //
        private bool _isDisposed;

        //
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _delayClose?.Cancel();

            // 投げっぱなし
            Task.Run(() =>
            {
                lock (_lock)
                {
                    Close(true);
                    _stream = null;
                }
            });
        }
    }



    public class SevenZipDescriptor : IDisposable
    {
        private SevenZipSource _source;

        private SevenZipExtractor _extractor;

        public SevenZipDescriptor(SevenZipSource source)
        {
            _source = source;
            _extractor = _source.Open();
        }

        public ReadOnlyCollection<ArchiveFileInfo> ArchiveFileData
        {
            get { return _extractor.ArchiveFileData; }
        }

        public void ExtractFile(int index, Stream extractStream)
        {
            _extractor.ExtractFile(index, extractStream);
        }

        public void Dispose()
        {
            _source.Close();
            _extractor = null;
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

        private static bool s_isLibraryInitialized;

        //
        private static void InitializeLibrary()
        {
            if (s_isLibraryInitialized) return;

            var dllPath = string.IsNullOrWhiteSpace(DllPath) ? System.IO.Path.Combine(App.Config.LibrariesPath, "7z.dll") : DllPath;

#if DEBUG
            // 開発中はLibrariesパスが存在しないので、カレントに設定しなおす
            if (!File.Exists(dllPath))
            {
                dllPath = System.IO.Path.Combine(App.Config.AssemblyLocation, "7z.dll");
            }
#endif

            SevenZipExtractor.SetLibraryPath(dllPath);

            FileVersionInfo dllVersionInfo = FileVersionInfo.GetVersionInfo(dllPath);
            Debug.WriteLine("7z.dll: ver" + dllVersionInfo?.FileVersion);

            s_isLibraryInitialized = true;
        }


        private static object s_lock = new object();



        private SevenZipSource _source;
        private Stream _stream;

        private bool _isDisposed;


        public SevenZipArchiver(string path, ArchiveEntry source) : base(path, source)
        {
            InitializeLibrary();

            _source = new SevenZipSource(Path, s_lock);
        }


        //
        public override bool IsDisposed => _isDisposed;

        // 廃棄処理
        public override void Dispose()
        {
            _isDisposed = true;

            lock (s_lock)
            {
                _source?.Dispose();
                _source = null;
                _stream?.Dispose();
                _stream = null;
            }

            base.Dispose();
        }


        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries(CancellationToken token)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            token.ThrowIfCancellationRequested();

            var list = new List<ArchiveEntry>();

            lock (s_lock)
            {
                if (_source == null) throw new ApplicationException("Archive already colosed.");

                using (var extractor = new SevenZipDescriptor(_source))
                {
                    for (int id = 0; id < extractor.ArchiveFileData.Count; ++id)
                    {
                        token.ThrowIfCancellationRequested();

                        var entry = extractor.ArchiveFileData[id];
                        if (!entry.IsDirectory)
                        {
                            list.Add(new ArchiveEntry()
                            {
                                Archiver = this,
                                Id = id,
                                EntryName = entry.FileName,
                                Length = (long)entry.Size,
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
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            lock (s_lock)
            {
                if (_source == null) throw new ApplicationException("Archive already colosed.");

                using (var extractor = new SevenZipDescriptor(_source))
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
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            lock (s_lock)
            {
                using (var extractor = new SevenZipExtractor(Path)) // 専用extractor
                using (Stream fs = new FileStream(exportFileName, FileMode.Create, FileAccess.Write))
                {
                    extractor.ExtractFile(entry.Id, fs);
                }
            }
        }
    }
}
