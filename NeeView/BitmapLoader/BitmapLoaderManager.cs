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
    // ローダーの種類
    public enum BitmapLoaderType
    {
        None,
        Default,
        Susie,
    }
    
    /// <summary>
    /// 画像ローダーのインターフェイス
    /// </summary>
    public interface IBitmapLoader
    {
        bool IsEnabled { get; }
        BitmapContent Load(Stream stream, ArchiveEntry entry, bool allowExifOrientation);
        BitmapContent LoadFromFile(string fileName, ArchiveEntry entry, bool allowExifOrientation);
    }


    /// <summary>
    /// BitmapLoader管理
    /// </summary>
    public class BitmapLoaderManager
    {
        // サポート拡張子
        Dictionary<BitmapLoaderType, string[]> _supprtedFileTypes = new Dictionary<BitmapLoaderType, string[]>()
        {
            [BitmapLoaderType.Default] = new string[] { ".bmp", ".dib", ".jpg", ".jpeg", ".jpe", ".jfif", ".gif", ".tif", ".tiff", ".png", ".ico",  },
            [BitmapLoaderType.Susie] = new string[] { },
        };

        // ローダー優先順位
        Dictionary<BitmapLoaderType, List<BitmapLoaderType>> _orderList = new Dictionary<BitmapLoaderType, List<BitmapLoaderType>>()
        {
            [BitmapLoaderType.Default] = new List<BitmapLoaderType>()
            {
                BitmapLoaderType.Default,
                BitmapLoaderType.Susie,
            },
            [BitmapLoaderType.Susie] = new List<BitmapLoaderType>()
            {
                BitmapLoaderType.Susie,
                BitmapLoaderType.Default,
            },
        };

        // ローダー優先順位の種類
        public BitmapLoaderType OrderType { set; get; } = BitmapLoaderType.Default;

        // ローダー優先リストを取得
        public List<BitmapLoaderType> OrderList
        {
            get { return _orderList[OrderType]; }
        }


        // コンストラクタ
        public BitmapLoaderManager()
        {
            UpdateDefaultSupprtedFileTypes();
        }


        // サポートしているローダーがあるか判定
        public bool IsSupported(string fileName)
        {
            return GetSupportedType(fileName) != BitmapLoaderType.None;
        }


        // サポートしているローダーの種類を取得
        public BitmapLoaderType GetSupportedType(string fileName)
        {
            string ext = LoosePath.GetExtension(fileName);

            foreach (var type in _orderList[OrderType])
            {
                if (type == BitmapLoaderType.Susie && !SusieBitmapLoader.IsEnable)
                {
                    continue;
                }

                if (_supprtedFileTypes[type].Contains(ext))
                {
                    return type;
                }
            }
            return BitmapLoaderType.None;
        }

        // デフォルトローダーのサポート拡張子を更新
        public void UpdateDefaultSupprtedFileTypes()
        {
            var list = new List<string>();

            foreach(var pair in DefaultBitmapLoader.GetExtensions())
            {
                list.AddRange(pair.Value.Split(','));
            }

            _supprtedFileTypes[BitmapLoaderType.Default] = list.ToArray();
        }


        // Susieローダーのサポート拡張子を更新
        public void UpdateSusieSupprtedFileTypes(Susie.Susie susie)
        {
            var list = new List<string>();
            foreach (var plugin in susie.INPlgunList)
            {
                if (plugin.IsEnable)
                {
                    list.AddRange(plugin.Extensions);
                }
            }
            _supprtedFileTypes[BitmapLoaderType.Susie] = list.Distinct().ToArray();
        }


        // ローダー作成
        public static IBitmapLoader Create(BitmapLoaderType type)
        {
            switch (type)
            {
                case BitmapLoaderType.Default:
                    return new DefaultBitmapLoader();

                case BitmapLoaderType.Susie:
                    return new SusieBitmapLoader();

                default:
                    throw new ArgumentException("no support BitmapLoaderType.", nameof(type));
            }
        }
    }



}
