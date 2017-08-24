// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバーマネージャ
    /// </summary>
    public class ArchiverManager
    {
        public static ArchiverManager Current { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        public ArchiverManager()
        {
            Current = this;

            SusieContext.Current.AddPropertyChanged(nameof(SusieContext.IsEnabled),
                (s, e) => UpdateOrderList());
            SusieContext.Current.AddPropertyChanged(nameof(SusieContext.IsFirstOrderSusieArchive),
                (s, e) => UpdateOrderList());

            UpdateOrderList();
        }

        // サポート拡張子
        private Dictionary<ArchiverType, FileTypeCollection> _supprtedFileTypes = new Dictionary<ArchiverType, FileTypeCollection>()
        {
            [ArchiverType.SevenZipArchiver] = SevenZipArchiverProfile.Current.SupportFileTypes,
            [ArchiverType.ZipArchiver] = new FileTypeCollection(".zip"),
            [ArchiverType.PdfArchiver] = new FileTypeCollection(".pdf"),
            [ArchiverType.SusieArchiver] = SusieContext.Current.ArchiveExtensions,
        };


        private List<ArchiverType> _orderList;

        // アーカイバー有効/無効
        public bool IsEnabled { get; set; } = true;


        // 検索順を更新
        private void UpdateOrderList()
        {
            var order = new List<ArchiverType>()
            {
                ArchiverType.SevenZipArchiver,
                ArchiverType.ZipArchiver,
                ArchiverType.PdfArchiver,
            };

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
            if (isArrowFileSystem && (fileName.Last() == '\\' || fileName.Last() == '/'))
            {
                return ArchiverType.FolderArchive;
            }

            if (IsEnabled)
            {
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
        /// <param name="stream">アーカイブストリーム。ファイルから開く場合はnull</param>
        /// <param name="source">元となったアーカイブエントリ</param>
        /// <param name="isAll">全て展開を前提とする</param>
        /// <returns>作成されたアーカイバー</returns>
        public Archiver CreateArchiver(ArchiverType type, string path, Stream stream, ArchiveEntry source, bool isAll)
        {
            // streamは未使用
            Debug.Assert(stream == null);

            switch (type)
            {
                case ArchiverType.FolderArchive:
                    return new FolderArchive(path, source);
                case ArchiverType.ZipArchiver:
                    return new ZipArchiver(path, source);
                case ArchiverType.SevenZipArchiver:
                    return new SevenZipArchiverProxy(path, source, isAll);
                case ArchiverType.PdfArchiver:
                    return new PdfArchiver(path, source);
                case ArchiverType.SusieArchiver:
                    return new SusieArchiver(path, source);
                default:
                    throw new ArgumentException("no support ArchvierType.", nameof(type));
            }
        }

        // アーカイバー作成
        public Archiver CreateArchiver(string path, ArchiveEntry source, bool isAll)
        {
            if (Directory.Exists(path))
            {
                return CreateArchiver(ArchiverType.FolderArchive, path, null, source, isAll);
            }
            else
            {
                return CreateArchiver(GetSupportedType(path), path, null, source, isAll);
            }
        }

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsEnabled { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsEnabled = this.IsEnabled;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsEnabled = memento.IsEnabled;
        }
        #endregion

    }
}
