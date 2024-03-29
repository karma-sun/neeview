﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    [Serializable]
    public class NotSupportedFileTypeException : Exception
    {
        public NotSupportedFileTypeException() { }

        public NotSupportedFileTypeException(string extension) : base(string.Format(Properties.Resources.Notice_NotSupportedFileType, extension))
        {
            Extension = extension;
        }

        public NotSupportedFileTypeException(string extension, string message) : base(message)
        {
            Extension = extension;
        }

        public NotSupportedFileTypeException(string extension, string message, Exception inner) : base(message)
        {
            Extension = extension;
        }

        public string Extension { get; set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("NotSupportedFileTypeException.Extension", this.Extension);
        }
    }

    /// <summary>
    /// アーカイバーマネージャ
    /// </summary>
    public class ArchiverManager : BindableBase, IDisposable
    {
        static ArchiverManager() => Current = new ArchiverManager();
        public static ArchiverManager Current { get; }


        #region Fields

        /// <summary>
        /// アーカイバのサポート拡張子
        /// </summary>
        private Dictionary<ArchiverType, FileTypeCollection> _supprtedFileTypes = new Dictionary<ArchiverType, FileTypeCollection>()
        {
            [ArchiverType.SevenZipArchiver] = Config.Current.Archive.SevenZip.SupportFileTypes,
            [ArchiverType.ZipArchiver] = Config.Current.Archive.Zip.SupportFileTypes,
            [ArchiverType.PdfArchiver] = Config.Current.Archive.Pdf.SupportFileTypes,
            [ArchiverType.MediaArchiver] = Config.Current.Archive.Media.SupportFileTypes,
            [ArchiverType.SusieArchiver] = SusiePluginManager.Current.ArchiveExtensions,
            [ArchiverType.PlaylistArchiver] = new FileTypeCollection(PlaylistArchive.Extension),
        };

        // アーカイバの適用順
        private List<ArchiverType> _orderList;
        private bool _isDartyOrderList = true;

        private ArchiverCache _cache = new ArchiverCache();

        #endregion

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        private ArchiverManager()
        {
            Config.Current.Archive.Zip.AddPropertyChanged(nameof(ZipArchiveConfig.IsEnabled),
                (s, e) => UpdateOrderList());
            Config.Current.Archive.SevenZip.AddPropertyChanged(nameof(SevenZipArchiveConfig.IsEnabled),
                (s, e) => UpdateOrderList());
            Config.Current.Archive.Pdf.AddPropertyChanged(nameof(PdfArchiveConfig.IsEnabled),
                (s, e) => UpdateOrderList());
            Config.Current.Archive.Media.AddPropertyChanged(nameof(MediaArchiveConfig.IsEnabled),
                (s, e) => UpdateOrderList());
            Config.Current.Susie.AddPropertyChanged(nameof(SusieConfig.IsEnabled),
                (s, e) => UpdateOrderList());
            Config.Current.Susie.AddPropertyChanged(nameof(SusieConfig.IsFirstOrderSusieArchive),
                (s, e) => UpdateOrderList());

            // 検索順初期化
            var tmp = OrderList;

            ApplicationDisposer.Current.Add(this);
        }

        #endregion

        #region Properties

        // 対応アーカイブ検索用リスト
        private List<ArchiverType> OrderList
        {
            get
            {
                if (_isDartyOrderList)
                {
                    _orderList = CreateOrderList();
                    _isDartyOrderList = false;
                }

                return _orderList;
            }
        }

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cache.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        #region Methods

        //
        private void UpdateOrderList()
        {
            _isDartyOrderList = true;
        }

        // 検索順を更新
        private List<ArchiverType> CreateOrderList()
        {
            var order = new List<ArchiverType>();

            order.Add(ArchiverType.PlaylistArchiver);

            if (Config.Current.Archive.Zip.IsEnabled)
            {
                order.Add(ArchiverType.ZipArchiver);
            }

            if (Config.Current.Archive.SevenZip.IsEnabled)
            {
                order.Add(ArchiverType.SevenZipArchiver);
            }

            if (Config.Current.Archive.Pdf.IsEnabled)
            {
                order.Add(ArchiverType.PdfArchiver);
            }

            if (Config.Current.Archive.Media.IsEnabled)
            {
                order.Add(ArchiverType.MediaArchiver);
            }

            if (Config.Current.Susie.IsEnabled)
            {
                if (Config.Current.Susie.IsFirstOrderSusieArchive)
                {
                    order.Insert(0, ArchiverType.SusieArchiver);
                }
                else
                {
                    order.Add(ArchiverType.SusieArchiver);
                }
            }

            return order;
        }


        // サポートしているアーカイバーがあるか判定
        public bool IsSupported(string fileName, bool isAllowFileSystem = true, bool isAllowMedia = true)
        {
            return GetSupportedType(fileName, isAllowFileSystem, isAllowMedia) != ArchiverType.None;
        }


        // サポートしているアーカイバーを取得
        public ArchiverType GetSupportedType(string fileName, bool isArrowFileSystem = true, bool isAllowMedia = true)
        {
            var query = new QueryPath(fileName);

            if (isArrowFileSystem && (fileName.Last() == '\\' || fileName.Last() == '/'))
            {
                return ArchiverType.FolderArchive;
            }

            string ext = LoosePath.GetExtension(fileName);

            foreach (var type in this.OrderList)
            {
                if (_supprtedFileTypes[type].Contains(ext))
                {
                    return (isAllowMedia || type != ArchiverType.MediaArchiver) ? type : ArchiverType.None;
                }
            }

            return ArchiverType.None;
        }

        /// <summary>
        /// 除外フォルダー判定
        /// </summary>
        /// <param name="path">判定するパス</param>
        /// <returns></returns>
        public bool IsExcludedFolder(string path)
        {
            return Config.Current.Book.Excludes.Contains(LoosePath.GetFileName(path));
        }


        /// <summary>
        /// アーカイバー作成
        /// stream に null 以外を指定すると、そのストリームを使用してアーカイブを開きます。
        /// この stream はアーカイブ廃棄時に Dispose されます。
        /// </summary>
        /// <param name="type">アーカイブの種類</param>
        /// <param name="path">アーカイブファイルのパス</param>
        /// <param name="source">元となったアーカイブエントリ</param>
        /// <param name="isRoot">ルートアーカイブとする</param>
        /// <returns>作成されたアーカイバー</returns>
        private Archiver CreateArchiver(ArchiverType type, string path, ArchiveEntry source)
        {
            Archiver archiver;

            switch (type)
            {
                case ArchiverType.FolderArchive:
                    archiver = new FolderArchive(path, source);
                    break;
                case ArchiverType.ZipArchiver:
                    archiver = new ZipArchiver(path, source);
                    break;
                case ArchiverType.SevenZipArchiver:
                    archiver = new SevenZipArchiver(path, source);
                    break;
                case ArchiverType.PdfArchiver:
                    archiver = new PdfArchiver(path, source);
                    break;
                case ArchiverType.MediaArchiver:
                    archiver = new MediaArchiver(path, source);
                    break;
                case ArchiverType.SusieArchiver:
                    archiver = new SusieArchiver(path, source);
                    break;
                case ArchiverType.PlaylistArchiver:
                    archiver = new PlaylistArchive(path, source);
                    break;
                default:
                    ////throw new ArgumentException("Not support archive type.");
                    string extension = LoosePath.GetExtension(path);
                    throw new NotSupportedFileTypeException(extension);
            }

            _cache.Add(archiver);

            return archiver;
        }

        // アーカイバー作成
        private Archiver CreateArchiver(string path, ArchiveEntry source)
        {
            if (Directory.Exists(path))
            {
                return CreateArchiver(ArchiverType.FolderArchive, path, source);
            }
            else
            {
                return CreateArchiver(GetSupportedType(path), path, source);
            }
        }

        /// <summary>
        /// アーカイバ作成。
        /// テンポラリファイルへの展開が必要になることもあるので非同期
        /// </summary>
        /// <param name="source">ArchiveEntry</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Archiver> CreateArchiverAsync(ArchiveEntry source, bool ignoreCache, CancellationToken token)
        {
            // キャッシュがあればそれを返す。
            var systemPath = source.SystemPath;
            if (!ignoreCache && _cache.TryGetValue(systemPath, out var archiver))
            {
                // 更新日、サイズを比較して再利用するかを判定
                if (archiver.LastWriteTime == source.LastWriteTime && archiver.Length == source.Length)
                {
                    ////Debug.WriteLine($"Archiver: Find cache: {systemPath}");
                    return archiver;
                }
                else
                {
                   //// Debug.WriteLine($"Archiver: Old cache: {systemPath}");
                }
            }
            else
            {
                if (ignoreCache)
                {
                    ////Debug.WriteLine($"Archiver: Ignore cache: {systemPath}");
                }
                else
                {
                    ////Debug.WriteLine($"Archiver: Cache not found: {systemPath}");
                }
            }

            if (source.IsFileSystem)
            {
                return CreateArchiver(source.SystemPath, null);
            }
            else
            {
                // TODO: テンポラリファイルの指定方法をスマートに。
                var tempFile = await ArchiveEntryExtractorService.Current.ExtractAsync(source, token);
                var archiverTemp = CreateArchiver(tempFile.Path, source);
                ////Debug.WriteLine($"Archiver: {archiverTemp.SystemPath} => {tempFile.Path}");
                Debug.Assert(archiverTemp.TempFile == null);
                archiverTemp.TempFile = tempFile;
                return archiverTemp;
            }
        }


        /// <summary>
        /// パスが実在するアーカイブであるかを判定
        /// </summary>
        /// 
        public bool Exists(string path, bool isAllowFileSystem)
        {
            if (isAllowFileSystem)
            {
                return Directory.Exists(path) || (File.Exists(path) && IsSupported(path, true));
            }
            else
            {
                return File.Exists(path) && IsSupported(path, false);
            }
        }

        /// <summary>
        /// アーカイブパスからファイルシステムに実在するアーカイブファイルのパスを取得
        /// ex: C:\hoge.zip\sub\test.txt -> C:\hoge.zip
        /// </summary>
        /// <param name="path">アーカイブパス</param>
        /// <returns>実在するアーカイブファイルのパス。見つからなかった場合はnull</returns>
        public string GetExistPathName(string path)
        {
            if (Exists(path, true))
            {
                return path;
            }

            while (true)
            {
                path = LoosePath.GetDirectoryName(path);
                if (string.IsNullOrEmpty(path) || Directory.Exists(path))
                {
                    break;
                }

                if (Exists(path, false))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// アーカイブパス表現を解析
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public void AnalyzeInnerArchivePath(string path)
        {

        }


        public ArchiverType GetArchiverType(Archiver archiver)
        {
            switch (archiver)
            {
                case FolderArchive folderArchive:
                    return ArchiverType.FolderArchive;
                case ZipArchiver zipArchiver:
                    return ArchiverType.ZipArchiver;
                case SevenZipArchiver sevenZipArchiver:
                    return ArchiverType.SevenZipArchiver;
                case PdfArchiver pdfArchiver:
                    return ArchiverType.PdfArchiver;
                case MediaArchiver mediaArchiver:
                    return ArchiverType.MediaArchiver;
                case SusieArchiver susieArchiver:
                    return ArchiverType.SusieArchiver;
                case PlaylistArchive playlistArchvier:
                    return ArchiverType.PlaylistArchiver;
                default:
                    return ArchiverType.None;
            }
        }

        /// <summary>
        /// すべてのアーカイブのファイルロック解除
        /// </summary>
        public async Task UnlockAllArchivesAsync()
        {
            // NOTE: MTAスレッドで実行。SevenZipSharpのCOM例外対策
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                await Task.Run(() => _cache.Unlock());
            }
            else
            {
                _cache.Unlock();
            }
        }

#endregion

#region Debug

        [Conditional("DEBUG")]
        public void DumpCache()
        {
            _cache.CleanUp();
            _cache.Dump();
        }

#endregion
    }
}
