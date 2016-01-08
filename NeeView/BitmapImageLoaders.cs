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
    /// 画像ローダーのインターフェイス
    /// </summary>
    public interface IBitmapLoader
    {
        BitmapSource Load(Stream stream, string fileName);
    }

    /// <summary>
    /// 標準画像ローダー
    /// </summary>
    public class DefaultBitmapLoader : IBitmapLoader
    {
        public BitmapSource Load(Stream stream, string fileName)
        {
            BitmapImage bmpImage = new BitmapImage();

            bmpImage.BeginInit();
            bmpImage.CacheOption = BitmapCacheOption.OnLoad;
            bmpImage.StreamSource = stream;
            bmpImage.EndInit();
            bmpImage.Freeze();

            return bmpImage;
        }
    }


    /// <summary>
    /// Susie画像ローダー
    /// </summary>
    public class SusieBitmapLoader : IBitmapLoader
    {
        public static bool IsEnable;

        public BitmapSource Load(Stream stream, string fileName)
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
            return ModelContext.Susie.GetPicture(fileName, buff); // ファイル名は識別用
        }
    }
}
