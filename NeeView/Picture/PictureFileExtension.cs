// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// 画像ファイル拡張子
    /// </summary>
    public class PictureFileExtension
    {
        #region Fields

        private FileTypeCollection _defaultExtensoins = new FileTypeCollection();
        private FileTypeCollection _susieExtensions = SusieContext.Current.ImageExtensions;

        #endregion

        #region Constructors

        public PictureFileExtension()
        {
            UpdateDefaultSupprtedFileTypes();
        }

        #endregion

        #region Methods

        // デフォルトローダーのサポート拡張子を更新
        private void UpdateDefaultSupprtedFileTypes()
        {
            var list = new List<string>();

            foreach (var pair in GetDefaultExtensions())
            {
                list.AddRange(pair.Value.Split(','));
            }

            _defaultExtensoins.FromCollection(list);
        }

        // 標準対応拡張子取得
        private Dictionary<string, string> GetDefaultExtensions()
        {
            var dictionary = new Dictionary<string, string>();

            // 標準
            dictionary.Add("BMP Decoder", ".bmp,.dib,.rle");
            dictionary.Add("GIF Decoder", ".gif");
            dictionary.Add("ICO Decoder", ".ico,.icon");
            dictionary.Add("JPEG Decoder", ".jpeg,.jpe,.jpg,.jfif,.exif");
            dictionary.Add("PNG Decoder", ".png");
            dictionary.Add("TIFF Decoder", ".tiff,.tif");
            dictionary.Add("WMPhoto Decoder", ".wdp,.jxr");
            dictionary.Add("DDS Decoder", ".dds"); // (微妙..)

            // WIC
            try
            {
                var wics = Utility.WicDecoders.ListUp();
                dictionary = dictionary.Concat(wics).ToDictionary(x => x.Key, x => x.Value);
            }
            catch { } // 失敗しても動くように例外スルー

            return dictionary;
        }

        // サポートしている拡張子か
        public bool IsSupported(string fileName)
        {
            string ext = LoosePath.GetExtension(fileName);

            if (_defaultExtensoins.Contains(ext)) return true;

            if (SusieContext.Current.IsEnabled)
            {
                if (_susieExtensions.Contains(ext)) return true;
            }

            return false;
        }

        #endregion
    }
}
