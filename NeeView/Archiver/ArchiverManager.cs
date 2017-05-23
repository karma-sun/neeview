// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        }

        // サポート拡張子
        private Dictionary<ArchiverType, string[]> _supprtedFileTypes = new Dictionary<ArchiverType, string[]>()
        {
            [ArchiverType.SevenZipArchiver] = new string[] { ".7z", ".rar", ".lzh" },
            [ArchiverType.ZipArchiver] = new string[] { ".zip" },
            [ArchiverType.SusieArchiver] = new string[] { }
        };

        // アーカイバー優先度リスト
        private Dictionary<ArchiverType, List<ArchiverType>> _orderList = new Dictionary<ArchiverType, List<ArchiverType>>()
        {
            [ArchiverType.DefaultArchiver] = new List<ArchiverType>()
            {
                ArchiverType.SevenZipArchiver,
                ArchiverType.ZipArchiver,
                ArchiverType.SusieArchiver
            },
            [ArchiverType.SusieArchiver] = new List<ArchiverType>()
            {
                ArchiverType.SusieArchiver,
                ArchiverType.SevenZipArchiver,
                ArchiverType.ZipArchiver,
            },
        };

        // アーカイバー有効/無効
        public bool IsEnabled { get; set; } = true;

        // アーカイバー優先度リストの種類
        public ArchiverType OrderType { set; get; } = ArchiverType.DefaultArchiver;

        // サポートしているアーカイバーがあるか判定
        public bool IsSupported(string fileName)
        {
            return GetSupportedType(fileName) != ArchiverType.None;
        }


        // サポートしているアーカイバーを取得
        public ArchiverType GetSupportedType(string fileName, bool isArrowFileSystem = true)
        {
            if (isArrowFileSystem && fileName.Last() == '\\')
            {
                return ArchiverType.FolderArchive;
            }

            if (IsEnabled)
            {
                string ext = LoosePath.GetExtension(fileName);

                foreach (var type in _orderList[OrderType])
                {
                    if (type == ArchiverType.SusieArchiver && !SusieArchiver.IsEnable)
                    {
                        continue;
                    }

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
            return BitmapLoaderManager.Current.Excludes.Contains(LoosePath.GetFileName(path));
        }

        // SevenZipアーカイバーのサポート拡張子を更新
        public void UpdateSevenZipSupprtedFileTypes(string exts)
        {
            if (exts == null) return;

            var list = new List<string>();
            foreach (var token in exts.Split(';'))
            {
                var ext = token.Trim().TrimStart('.').ToLower();
                if (!string.IsNullOrWhiteSpace(ext)) list.Add("." + ext);
            }
            _supprtedFileTypes[ArchiverType.SevenZipArchiver] = list.ToArray();
            Debug.WriteLine("7z.dll Support: " + string.Join(" ", _supprtedFileTypes[ArchiverType.SevenZipArchiver]));
        }

        // Susieアーカイバーのサポート拡張子を更新
        public void UpdateSusieSupprtedFileTypes(Susie.Susie susie)
        {
            var list = new List<string>();
            foreach (var plugin in susie.AMPlgunList)
            {
                if (plugin.IsEnable)
                {
                    list.AddRange(plugin.Extensions);
                }
            }
            _supprtedFileTypes[ArchiverType.SusieArchiver] = list.Distinct().ToArray();
            Debug.WriteLine("SusieAM Support: " + string.Join(" ", _supprtedFileTypes[ArchiverType.SusieArchiver]));
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
        /// <returns>作成されたアーカイバー</returns>
        public Archiver CreateArchiver(ArchiverType type, string path, Stream stream, ArchiveEntry source)
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
                    return new SevenZipArchiver(path, source);
                case ArchiverType.SusieArchiver:
                    return new SusieArchiver(path, source);
                default:
                    throw new ArgumentException("no support ArchvierType.", nameof(type));
            }
        }

        // アーカイバー作成
        public Archiver CreateArchiver(string path, ArchiveEntry source)
        {
            if (Directory.Exists(path))
            {
                return CreateArchiver(ArchiverType.FolderArchive, path, null, source);
            }

            return CreateArchiver(GetSupportedType(path), path, null, source);
        }
    }
}
