// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    //
    public class PictureFileExtension
    {
        //
        public List<string> DefaultExtensoins { get; set; }

        //
        public List<string> SusieExtensions => SusieContext.Current.Extensions;


        public PictureFileExtension()
        {
            UpdateDefaultSupprtedFileTypes();
        }

        // デフォルトローダーのサポート拡張子を更新
        private void UpdateDefaultSupprtedFileTypes()
        {
            var list = new List<string>();

            foreach (var pair in GetDefaultExtensions())
            {
                list.AddRange(pair.Value.Split(','));
            }

            DefaultExtensoins = list;
        }


        // 対応拡張子取得
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

            if (this.DefaultExtensoins.Contains(ext)) return true;

            if (SusieContext.Current.IsEnabled)
            {
                if (this.SusieExtensions.Contains(ext)) return true;
            }

            return false;
        }
    }
}
