// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
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
    /// BitmapLoader管理
    /// </summary>
    public class BitmapLoaderManager
    {
        // サポート拡張子
        Dictionary<BitmapLoaderType, string[]> _SupprtedFileTypes = new Dictionary<BitmapLoaderType, string[]>()
        {
            [BitmapLoaderType.Default] = new string[] { ".bmp", ".dib", ".jpg", ".jpeg", ".jpe", ".jfif", ".gif", ".tif", ".tiff", ".png", ".ico" },
            [BitmapLoaderType.Susie] = new string[] { },
        };

        // ローダー優先順位
        Dictionary<BitmapLoaderType, List<BitmapLoaderType>> _OrderList = new Dictionary<BitmapLoaderType, List<BitmapLoaderType>>()
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
            get { return _OrderList[OrderType]; }
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

            foreach (var type in _OrderList[OrderType])
            {
                if (type == BitmapLoaderType.Susie && !SusieBitmapLoader.IsEnable)
                {
                    continue;
                }

                if (_SupprtedFileTypes[type].Contains(ext))
                {
                    return type;
                }
            }
            return BitmapLoaderType.None;
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
            _SupprtedFileTypes[BitmapLoaderType.Susie] = list.Distinct().ToArray();
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
