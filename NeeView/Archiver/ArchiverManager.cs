// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバーマネージャ
    /// </summary>
    public class ArchiverManager : BindableBase
    {
        public static ArchiverManager Current { get; private set; }

        #region Fields

        /// <summary>
        /// アーカイバのサポート拡張子
        /// </summary>
        private Dictionary<ArchiverType, FileTypeCollection> _supprtedFileTypes = new Dictionary<ArchiverType, FileTypeCollection>()
        {
            [ArchiverType.SevenZipArchiver] = SevenZipArchiverProfile.Current.SupportFileTypes,
            [ArchiverType.ZipArchiver] = ZipArchiverProfile.Current.SupportFileTypes,
            [ArchiverType.PdfArchiver] = new FileTypeCollection(".pdf"),
            [ArchiverType.MediaArchiver] = MediaArchiverProfile.Current.SupportFileTypes,
            [ArchiverType.SusieArchiver] = SusieContext.Current.ArchiveExtensions,
        };

        // アーカイバの適用順
        private List<ArchiverType> _orderList;
        private bool _isDartyOrderList = true;

        #endregion

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        public ArchiverManager()
        {
            Current = this;

            ZipArchiverProfile.Current.AddPropertyChanged(nameof(ZipArchiverProfile.IsEnabled),
                (s, e) => UpdateOrderList());
            SevenZipArchiverProfile.Current.AddPropertyChanged(nameof(MediaArchiverProfile.IsEnabled),
                (s, e) => UpdateOrderList());
            PdfArchiverProfile.Current.AddPropertyChanged(nameof(PdfArchiverProfile.IsEnabled),
                (s, e) => UpdateOrderList());
            MediaArchiverProfile.Current.AddPropertyChanged(nameof(MediaArchiverProfile.IsEnabled),
                (s, e) => UpdateOrderList());
            SusieContext.Current.AddPropertyChanged(nameof(SusieContext.IsEnabled),
                (s, e) => UpdateOrderList());
            SusieContext.Current.AddPropertyChanged(nameof(SusieContext.IsFirstOrderSusieArchive),
                (s, e) => UpdateOrderList());

            // 検索順初期化
            var tmp = OrderList;
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

            if (ZipArchiverProfile.Current.IsEnabled)
            {
                order.Add(ArchiverType.ZipArchiver);
            }

            if (SevenZipArchiverProfile.Current.IsEnabled)
            {
                order.Add(ArchiverType.SevenZipArchiver);
            }

            if (PdfArchiverProfile.Current.IsEnabled)
            {
                order.Add(ArchiverType.PdfArchiver);
            }

            if (MediaArchiverProfile.Current.IsEnabled)
            {
                order.Add(ArchiverType.MediaArchiver);
            }

            if (SusieContext.Current.IsEnabled)
            {
                if (SusieContext.Current.IsFirstOrderSusieArchive)
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
            return BookProfile.Current.Excludes.Contains(LoosePath.GetFileName(path));
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
        /// <param name="isAll">全て展開を前提とする</param>
        /// <returns>作成されたアーカイバー</returns>
        public Archiver CreateArchiver(ArchiverType type, string path, ArchiveEntry source, bool isRoot, bool isAll)
        {
            switch (type)
            {
                case ArchiverType.FolderArchive:
                    return new FolderArchive(path, source, isRoot);
                case ArchiverType.ZipArchiver:
                    return new ZipArchiver(path, source, isRoot);
                case ArchiverType.SevenZipArchiver:
                    return new SevenZipArchiverProxy(path, source, isRoot, isAll);
                case ArchiverType.PdfArchiver:
                    return new PdfArchiver(path, source, isRoot);
                case ArchiverType.MediaArchiver:
                    return new MediaArchiver(path, source, isRoot);
                case ArchiverType.SusieArchiver:
                    return new SusieArchiver(path, source, isRoot);
                default:
                    throw new ArgumentException("対応するアーカイバーがありません。");
            }
        }

        // アーカイバー作成
        public Archiver CreateArchiver(string path, ArchiveEntry source, bool isRoot, bool isAll)
        {
            if (Directory.Exists(path))
            {
                return CreateArchiver(ArchiverType.FolderArchive, path, source, isRoot, isAll);
            }
            else
            {
                return CreateArchiver(GetSupportedType(path), path, source, isRoot, isAll);
            }
        }

        /// <summary>
        /// アーカイバ作成
        /// </summary>
        /// <param name="path">パス</param>
        /// <param name="isAll"></param>
        /// <returns></returns>
        public Archiver CreateArchiver(string path, bool isRoot, bool isAll)
        {
            return CreateArchiver(path, null, isRoot, isAll);
        }

        /// <summary>
        /// アーカイバ作成。
        /// テンポラリファイルへの展開が必要になることもあるので非同期
        /// </summary>
        /// <param name="source">ArchiveEntry</param>
        /// <param name="isAll"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Archiver> CreateArchiverAsync(ArchiveEntry source, bool isRoot, bool isAll, CancellationToken token)
        {
            if (source.IsFileSystem)
            {
                return CreateArchiver(source.FullPath, null, isRoot, isAll);
            }
            else
            {
                // TODO: テンポラリファイルの指定方法をスマートに。
                var tempFile = await ArchivenEntryExtractorService.Current.ExtractAsync(source, token);
                var archiver = CreateArchiver(tempFile.Path, source, isRoot, isAll);
                archiver.TempFile = tempFile;
                return archiver;
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


        #endregion

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsEnabled { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext context)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        //
        public Memento CreateMemento()
        {
            return null;
            ////var memento = new Memento();
            ////return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

#pragma warning disable CS0612

            // compatible before ver.29
            if (memento._Version < Config.GenerateProductVersionNumber(1, 29, 0))
            {
                if (!memento.IsEnabled)
                {
                    ZipArchiverProfile.Current.IsEnabled = false;
                    SevenZipArchiverProfile.Current.IsEnabled = false;
                    PdfArchiverProfile.Current.IsEnabled = false;
                    MediaArchiverProfile.Current.IsEnabled = false;
                    SusieContext.Current.IsEnableSusie = false;
                }
            }

#pragma warning restore CS0612
        }

        #endregion
    }
}
