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
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Susie画像ローダー
    /// </summary>
    public class SusieBitmapLoader : IBitmapLoader
    {
        public static bool IsEnable { get; set; }

        private Susie.SusiePlugin _SusiePlugin;

        // Bitmap読み込み
        public BitmapContent Load(Stream stream, ArchiveEntry entry, bool allowExifOrientation)
        {
            if (!IsEnable) return null;

            byte[] buff;
            if (stream is MemoryStream)
            {
                buff = ((MemoryStream)stream).GetBuffer();
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    buff = ms.GetBuffer();
                }
            }

            var bmpSource = ModelContext.Susie?.GetPicture(entry.EntryName, buff, true, out _SusiePlugin); // ファイル名は識別用
            if (bmpSource == null) return null;

            var info = new FileBasicInfo();
            info.FileSize = entry.FileSize;
            info.LastWriteTime = entry.LastWriteTime;
            info.Decoder = _SusiePlugin?.ToString();

            var resource = new BitmapContent();
            resource.Source = bmpSource;
            resource.Info = info;

            return resource;
        }

        // Bitmap読み込み(ファイル版)
        public BitmapContent LoadFromFile(string fileName, ArchiveEntry entry, bool allowExifOrientation)
        {
            if (!IsEnable) return null;

            var bmpSource = ModelContext.Susie?.GetPictureFromFile(fileName, true, out _SusiePlugin);
            if (bmpSource == null) return null;

            var info = new FileBasicInfo();
            info.FileSize = entry.FileSize;
            info.LastWriteTime = entry.LastWriteTime;
            info.Decoder = _SusiePlugin?.ToString();

            var resource = new BitmapContent();
            resource.Source = bmpSource;
            resource.Info = info;

            return resource;
        }
    }
}
