// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    public class ArchiverManager
    {
        public static ArchiverManager Current { get; private set; }

        #region Fields

        /// <summary>
        /// アーカイバのサポート拡張子
        /// </summary>
        private Dictionary<ArchiverType, FileTypeCollection> _supprtedFileTypes = new Dictionary<ArchiverType, FileTypeCollection>()
        {
            [ArchiverType.SevenZipArchiver] = SevenZipArchiverProfile.Current.SupportFileTypes,
            [ArchiverType.ZipArchiver] = new FileTypeCollection(".zip"),
            [ArchiverType.PdfArchiver] = new FileTypeCollection(".pdf"),
            [ArchiverType.SusieArchiver] = SusieContext.Current.ArchiveExtensions,
        };

        /// <summary>
        /// アーカイバの適用順
        /// </summary>
        private List<ArchiverType> _orderList;

        /// <summary>
        /// デフォルト除外パターン
        /// </summary>
        private const string _defaultExcludePattern = @"\.part([2-9]|\d\d+)\.rar$";

        /// <summary>
        /// 除外パターンの正規表現
        /// </summary>
        private Regex _excludeRegex;

        #endregion

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        public ArchiverManager()
        {
            Current = this;

            this.ExcludePattern = _defaultExcludePattern;

            SusieContext.Current.AddPropertyChanged(nameof(SusieContext.IsEnabled),
                (s, e) => UpdateOrderList());
            SusieContext.Current.AddPropertyChanged(nameof(SusieContext.IsFirstOrderSusieArchive),
                (s, e) => UpdateOrderList());

            UpdateOrderList();
        }

        #endregion

        #region Properties

        // アーカイバー有効/無効
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// IsPdfEnabled property.
        /// </summary>
        private bool _isPdfEnabled = true;
        public bool IsPdfEnabled
        {
            get { return _isPdfEnabled; }
            set { if (_isPdfEnabled != value) { _isPdfEnabled = value; UpdateOrderList(); } }
        }


        /// <summary>
        /// ExcludePattern property.
        /// </summary>
        private string _excludePattern;
        public string ExcludePattern
        {
            get { return _excludePattern; }
            set { if (_excludePattern != value) { _excludePattern = value; UpdateExcludeRegex(); } }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 除外パターンを更新
        /// </summary>
        private void UpdateExcludeRegex()
        {
            try
            {
                _excludeRegex = new Regex(_excludePattern, RegexOptions.IgnoreCase);
            }
            catch
            {
            }
        }


        // 検索順を更新
        private void UpdateOrderList()
        {
            var order = new List<ArchiverType>()
            {
                ArchiverType.SevenZipArchiver,
                ArchiverType.ZipArchiver,
            };

            if (this.IsPdfEnabled)
            {
                order.Add(ArchiverType.PdfArchiver);
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

            _orderList = order;
        }


        // サポートしているアーカイバーがあるか判定
        public bool IsSupported(string fileName, bool isAllowFileSystem = true)
        {
            return GetSupportedType(fileName, isAllowFileSystem) != ArchiverType.None;
        }


        // サポートしているアーカイバーを取得
        public ArchiverType GetSupportedType(string fileName, bool isArrowFileSystem = true)
        {
            if (isArrowFileSystem && (fileName.Last() == '\\' || fileName.Last() == '/' || Directory.Exists(fileName)))
            {
                return ArchiverType.FolderArchive;
            }

            if (IsEnabled)
            {
                // 除外判定
                if (_excludeRegex != null && _excludeRegex.IsMatch(fileName))
                {
                    return ArchiverType.None;
                }

                string ext = LoosePath.GetExtension(fileName);

                foreach (var type in _orderList)
                {
                    if (_supprtedFileTypes[type].Contains(ext))
                    {
                        return type;
                    }
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
                case ArchiverType.SusieArchiver:
                    return new SusieArchiver(path, source, isRoot);
                default:
                    throw new ArgumentException("no support ArchvierType.", nameof(type));
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
            public bool IsEnabled { get; set; }

            [DataMember, DefaultValue(true)]
            [PropertyMember("PDF対応")]
            public bool IsPdfEnabled { get; set; }

            [DataMember, DefaultValue(_defaultExcludePattern)]
            [PropertyMember("除外する圧縮ファイルのパターン", Tips = ".NETの正規表現で指定します")]
            public string ExcludePattern { get; set; }

            #region Constructors

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext context)
            {
                Constructor();
            }

            private void Constructor()
            {
                this.IsEnabled = true;
                this.IsPdfEnabled = true;
                this.ExcludePattern = _defaultExcludePattern;
            }

            #endregion
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsEnabled = this.IsEnabled;
            memento.IsPdfEnabled = this.IsPdfEnabled;
            memento.ExcludePattern = this.ExcludePattern;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsEnabled = memento.IsEnabled;
            this.IsPdfEnabled = memento.IsPdfEnabled;
            this.ExcludePattern = memento.ExcludePattern ?? _defaultExcludePattern;
        }
        #endregion

    }
}
