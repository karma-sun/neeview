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
    /// アーカイバマネージャ
    /// </summary>
    public class ArchiverManager
    {
        // サポート拡張子
        Dictionary<ArchiverType, string[]> _SupprtedFileTypes = new Dictionary<ArchiverType, string[]>()
        {
            [ArchiverType.SevenZipArchiver] = new string[] { ".7z", ".rar", ".lzh" },
            [ArchiverType.ZipArchiver] = new string[] { ".zip" },
            [ArchiverType.SusieArchiver] = new string[] { }
        };

        // アーカイバ優先度リスト
        Dictionary<ArchiverType, List<ArchiverType>> _OrderList = new Dictionary<ArchiverType, List<ArchiverType>>()
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

        // アーカイバ有効/無効
        public bool IsEnabled { get; set; } = true;

        // アーカイバ優先度リストの種類
        public ArchiverType OrderType { set; get; } = ArchiverType.DefaultArchiver;

        // サポートしているアーカイバがあるか判定
        public bool IsSupported(string fileName)
        {
            return GetSupportedType(fileName) != ArchiverType.None;
        }


        // サポートしているアーカイバを取得
        public ArchiverType GetSupportedType(string fileName, bool isArrowFileSystem=true)
        {
            if (isArrowFileSystem && fileName.Last() == '\\')
            {
                return ArchiverType.FolderFiles;
            }

            if (IsEnabled)
            {
                string ext = LoosePath.GetExtension(fileName);

                foreach (var type in _OrderList[OrderType])
                {
                    if (type == ArchiverType.SusieArchiver && !SusieArchiver.IsEnable)
                    {
                        continue;
                    }

                    if (_SupprtedFileTypes[type].Contains(ext))
                    {
                        return type;
                    }
                }
            }

            return ArchiverType.None;
        }

        // SevenZipアーカイバのサポート拡張子を更新
        public void UpdateSevenZipSupprtedFileTypes(string exts)
        {
            if (exts == null) return;

            var list = new List<string>();
            foreach (var token in exts.Split(';'))
            {
                var ext = token.Trim().TrimStart('.').ToLower();
                if (!string.IsNullOrWhiteSpace(ext)) list.Add("." + ext);
            }
            _SupprtedFileTypes[ArchiverType.SevenZipArchiver] = list.ToArray();
            Debug.WriteLine("7z.dll Support: " + string.Join(" ", _SupprtedFileTypes[ArchiverType.SevenZipArchiver]));
        }

        // Susieアーカイバのサポート拡張子を更新
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
            _SupprtedFileTypes[ArchiverType.SusieArchiver] = list.Distinct().ToArray();
            Debug.WriteLine("SusieAM Support: " + string.Join(" ", _SupprtedFileTypes[ArchiverType.SusieArchiver]));
        }

        // アーカイバ作成
        public Archiver CreateArchiver(ArchiverType type, string path, Archiver parent)
        {
            switch (type)
            {
                case ArchiverType.FolderFiles:
                    return new FolderFiles(path) { Parent = parent };
                case ArchiverType.ZipArchiver:
                    return new ZipArchiver(path) { Parent = parent };
                case ArchiverType.SevenZipArchiver:
                    return new SevenZipArchiver(path) { Parent = parent };
                case ArchiverType.SusieArchiver:
                    return new SusieArchiver(path) { Parent = parent };
                default:
                    throw new ArgumentException("no support ArchvierType.", nameof(type));
            }
        }

        // アーカイバ作成
        public Archiver CreateArchiver(string path, Archiver parent)
        {
            if (Directory.Exists(path))
            {
                return CreateArchiver(ArchiverType.FolderFiles, path, parent);
            }

            return CreateArchiver(GetSupportedType(path), path, parent);
        }
    }

}
