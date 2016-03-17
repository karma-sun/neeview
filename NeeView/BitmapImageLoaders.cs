// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        BitmapSource Load(Stream stream, string fileName, bool withExif);
        BitmapSource LoadFromFile(string fileName, bool withExif);
    }

    /// <summary>
    /// 標準画像ローダー
    /// </summary>
    public class DefaultBitmapLoader : IBitmapLoader
    {
        private BitmapCodecInfo _CodecInfo;

        public override string ToString()
        {
            if (_CodecInfo != null)
            {
                return $"{_CodecInfo.FriendlyName} (.Net)";
            }
            else
            {
                return "--";
            }
        }


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
        public BitmapSource Load(Stream stream, string fileName, bool withExif)
        {
            if (withExif)
            {
                return LoadWithExif(stream, fileName);
            }
            else
            {
                var decoder = System.Windows.Media.Imaging.BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                decoder.Frames[0].Freeze();
                _CodecInfo = decoder.CodecInfo;
                return decoder.Frames[0];
#if false
                BitmapImage bmpImage = new BitmapImage();

                bmpImage.BeginInit();
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.StreamSource = stream;
                bmpImage.EndInit();
                bmpImage.Freeze();

                return bmpImage;
#endif
            }
        }


        // Bitmap読み込み
        public BitmapSource LoadFromFile(string fileName, bool withExif)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return Load(stream, fileName, withExif);
            }
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



        // 対応拡張子取得
        public static Dictionary<string, string> GetExtensions()
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

            // from WIC
            try
            {
                //string root = @"SOFTWARE\WOW6432Node\Classes\CLSID\";
                string root = @"SOFTWARE\Classes\";
                
                // WICBitmapDecodersの一覧を開く
                var decoders = Registry.LocalMachine.OpenSubKey(root + @"CLSID\{7ED96837-96F0-4812-B211-F13C24117ED3}\Instance");
                foreach (var clsId in decoders.GetSubKeyNames())
                {
                    try
                    {
                        // コーデックのレジストリを開く
                        var codec = Registry.LocalMachine.OpenSubKey(root + @"CLSID\" + clsId);
                        string name = codec.GetValue("FriendlyName").ToString();
                        string extensions = codec.GetValue("FileExtensions").ToString().ToLower();
                        dictionary.Add(name, extensions);
                    }
                    catch { }
                }
            }
            catch { }

            return dictionary;
        }


    }


    /// <summary>
    /// Susie画像ローダー
    /// </summary>
    public class SusieBitmapLoader : IBitmapLoader
    {
        public static bool IsEnable { get; set; }

        private Susie.SusiePlugin _SusiePlugin;

        public override string ToString()
        {
            if (_SusiePlugin != null)
            {
                return $"{_SusiePlugin} (SusiePlugin)";
            }
            else
            {
                return "--";
            }
        }

        // Bitmap読み込み
        public BitmapSource Load(Stream stream, string fileName, bool withExif)
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
            return ModelContext.Susie?.GetPicture(fileName, buff, out _SusiePlugin); // ファイル名は識別用
        }

        // Bitmap読み込み(ファイル版)
        public BitmapSource LoadFromFile(string fileName, bool withExif)
        {
            if (!IsEnable) return null;

            return ModelContext.Susie?.GetPictureFromFile(fileName, out _SusiePlugin);
        }
    }
}
