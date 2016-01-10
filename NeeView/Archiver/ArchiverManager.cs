// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバマネージャ
    /// </summary>
    public class ArchiverManager
    {
        // サポート拡張子
        Dictionary<ArchiverType, string[]> _SupprtedFileTypes = new Dictionary<ArchiverType, string[]>()
        {
            [ArchiverType.ZipArchiver] = new string[] { ".zip" },
            [ArchiverType.SevenZipArchiver] = new string[] { ".7z", ".rar", ".lzh" },
            [ArchiverType.SusieArchiver] = new string[] { }
        };

        // アーカイバ優先度リスト
        Dictionary<ArchiverType, List<ArchiverType>> _OrderList = new Dictionary<ArchiverType, List<ArchiverType>>()
        {
            [ArchiverType.DefaultArchiver] = new List<ArchiverType>()
            {
                ArchiverType.ZipArchiver,
                ArchiverType.SevenZipArchiver,
                ArchiverType.SusieArchiver
            },
            [ArchiverType.SusieArchiver] = new List<ArchiverType>()
            {
                ArchiverType.SusieArchiver,
                ArchiverType.ZipArchiver,
                ArchiverType.SevenZipArchiver,
            },
        };

        // アーカイバ優先度リストの種類
        public ArchiverType OrderType { set; get; } = ArchiverType.DefaultArchiver;

        // サポートしているアーカイバがあるか判定
        public bool IsSupported(string fileName)
        {
            return GetSupportedType(fileName) != ArchiverType.None;
        }

        // サポートしているアーカイバを取得
        public ArchiverType GetSupportedType(string fileName)
        {
            if (fileName.Last() == '\\')
            {
                return ArchiverType.FolderFiles;
            }

            string ext = LoosePath.GetExtension(fileName);

            foreach (var type in _OrderList[OrderType])
            {
                if (_SupprtedFileTypes[type].Contains(ext))
                {
                    return type;
                }
            }
            return ArchiverType.None;
        }

        // Susieアーカイバのサポート拡張子を更新
        public void UpdateSusieSupprtedFileTypes(Susie.Susie susie)
        {
            var list = new List<string>();
            foreach (var plugin in susie.AMPlgunList.Values)
            {
                if (plugin.IsEnable)
                {
                    list.AddRange(plugin.Extensions);
                }
            }
            _SupprtedFileTypes[ArchiverType.SusieArchiver] = list.Distinct().ToArray();
        }

        // アーカイバ作成
        public Archiver CreateArchiver(ArchiverType type, string path)
        {
            switch (type)
            {
                case ArchiverType.FolderFiles:
                    return new FolderFiles(path);
                case ArchiverType.ZipArchiver:
                    return new ZipArchiver(path);
                case ArchiverType.ZipArchiverKeepOpened:
                    return new ZipArchiverKeepOpened(path);
                case ArchiverType.SevenZipArchiver:
                    return new SevenZipArchiver(path);
                case ArchiverType.SusieArchiver:
                    return new SusieArchiver(path);
                default:
                    throw new ArgumentException("no support ArchvierType.", nameof(type));
            }
        }

        // アーカイバ作成
        public Archiver CreateArchiver(string path)
        {
            if (Directory.Exists(path))
            {
                return CreateArchiver(ArchiverType.FolderFiles, path);
            }

            return CreateArchiver(GetSupportedType(path), path);
        }
    }

}
