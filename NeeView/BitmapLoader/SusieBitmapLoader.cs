// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class SusieIOException : Exception
    {
        public SusieIOException() : base("Susieでの画像取得に失敗しました。")
        {
        }

        public SusieIOException(string message) : base(message)
        {
        }

        public SusieIOException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Susie画像ローダー
    /// </summary>
    public class SusieBitmapLoader : IBitmapLoader
    {
        public static bool IsEnable { get; set; }

        private Susie.SusiePlugin _susiePlugin;

        // 有効判定
        public bool IsEnabled => IsEnable;

        // Bitmap読み込み
        public BitmapContentSource Load(Stream stream, ArchiveEntry entry, bool allowExifOrientation)
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

            var bmpSource = BitmapSourceFactory.Current.Create(SusieContext.Current.Susie?.GetPicture(entry.EntryName, buff, true, out _susiePlugin)); // ファイル名は識別用
            if (bmpSource == null)
            {
                throw new SusieIOException();
            }

            var info = new BitmapInfo();
            info.Length = entry.Length;
            info.LastWriteTime = entry.LastWriteTime;
            info.Decoder = _susiePlugin?.ToString();

            var resource = new BitmapContentSource();
            resource.Source = bmpSource;
            resource.Info = info;

            return resource;
        }

        // Bitmap読み込み(ファイル版)
        public BitmapContentSource LoadFromFile(string fileName, ArchiveEntry entry, bool allowExifOrientation)
        {
            if (!IsEnable) return null;

            var bmpSource = BitmapSourceFactory.Current.Create(SusieContext.Current.Susie?.GetPictureFromFile(fileName, true, out _susiePlugin));
            if (bmpSource == null)
            {
                throw new SusieIOException();
            }

            var info = new BitmapInfo();
            info.Length = entry.Length;
            info.LastWriteTime = entry.LastWriteTime;
            info.Decoder = _susiePlugin?.ToString();

            var resource = new BitmapContentSource();
            resource.Source = bmpSource;
            resource.Info = info;

            return resource;
        }
    }
}
