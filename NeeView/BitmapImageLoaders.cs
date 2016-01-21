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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 画像ローダーのインターフェイス
    /// </summary>
    public interface IBitmapLoader
    {
        BitmapSource Load(Stream stream, string fileName);
        BitmapSource LoadWithExif(Stream stream, string fileName);
    }

    /// <summary>
    /// 標準画像ローダー
    /// </summary>
    public class DefaultBitmapLoader : IBitmapLoader
    {
        private void DumpMetaData(string prefix, BitmapMetadata metadata)
        {
            foreach (var name in metadata)
            {
                string query = prefix + "(" + metadata.Format + ")" + name;

                if (metadata.ContainsQuery(name))
                {
                    var element = metadata.GetQuery(name);
                    if (element is BitmapMetadata)
                    {
                        DumpMetaData(query, (BitmapMetadata)element);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"{query}: {element?.ToString()}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"{prefix}: {name}");
                }
            }
        }

        // Bitmap読み込み
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

        // EXIF対応 Bitmap読み込み
        // 回転を反映させます
        public BitmapSource LoadWithExif(Stream stream, string fileName)
        {
            BitmapSource source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

            try
            {
                var metadata = (BitmapMetadata)source.Metadata;

                ////DumpMetaData("", metadata);

                string query = "/app1/ifd/{ushort=274}";
                if (metadata.ContainsQuery(query))
                {
                    var orientation = (ushort)metadata.GetQuery(query);

                    switch (orientation)
                    {
                        default:
                        case 1: //Horizontal(normal)
                            break;
                        case 2: // Mirror horizontal
                            source = new TransformedBitmap(source, new ScaleTransform(-1, 1));
                            break;
                        case 3: // Rotate 180
                            source = new TransformedBitmap(source, new RotateTransform(180));
                            break;
                        case 4: //Mirror vertical
                            source = new TransformedBitmap(source, new ScaleTransform(1, -1));
                            break;
                        case 5: // Mirror horizontal and rotate 270 CW
                            var group5 = new TransformGroup();
                            group5.Children.Add(new ScaleTransform(-1, 1));
                            group5.Children.Add(new RotateTransform(270));
                            source = new TransformedBitmap(source, group5);
                            break;
                        case 6: //Rotate 90 CW
                            source = new TransformedBitmap(source, new RotateTransform(90));
                            break;
                        case 7: // Mirror horizontal and rotate 90 CW
                            var group7 = new TransformGroup();
                            group7.Children.Add(new ScaleTransform(-1, 1));
                            group7.Children.Add(new RotateTransform(90));
                            source = new TransformedBitmap(source, group7);
                            break;
                        case 8: // Rotate 270 CW
                            source = new TransformedBitmap(source, new RotateTransform(270));
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            source.Freeze();
            return source;
        }
    }


    /// <summary>
    /// Susie画像ローダー
    /// </summary>
    public class SusieBitmapLoader : IBitmapLoader
    {
        public static bool IsEnable { get; set; }

        // Bitmap読み込み
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

        // EXIF対応 Bitmap読み込み
        // Susieは未対応
        public BitmapSource LoadWithExif(Stream stream, string fileName)
        {
            return Load(stream, fileName);
        }
    }
}
