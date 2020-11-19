using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// 画像ファイル拡張子
    /// </summary>
    public static class PictureFileExtensionTools
    {
        // デフォルトローダーのサポート拡張子を更新
        public static FileTypeCollection CreateDefaultSupprtedFileTypes(bool useWic)
        {
            var collection = (System.Threading.Thread.CurrentThread.GetApartmentState() == System.Threading.ApartmentState.STA)
                ? CreateSystemExtensions(useWic)
                : AppDispatcher.Invoke(() => CreateSystemExtensions(useWic));

            var list = new List<string>();
            foreach (var pair in collection)
            {
                list.AddRange(pair.Value.Split(','));
            }

            var defaultExtensions = new FileTypeCollection();
            defaultExtensions.Restore(list);
            Debug.WriteLine($"DefaultExtensions: {defaultExtensions}");

            return defaultExtensions;
        }

        // 標準対応拡張子取得
        private static Dictionary<string, string> CreateSystemExtensions(bool useWic)
        {
            if (useWic)
            {
                try
                {
                    return WicDecoders.ListUp();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"WicDecoders.ListUp failed: {ex.Message}");
                }
            }

            return CreateDefaultExtensions();
        }

        private static Dictionary<string, string> CreateDefaultExtensions()
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("BMP Decoder", ".bmp,.dib,.rle");
            dictionary.Add("GIF Decoder", ".gif");
            dictionary.Add("ICO Decoder", ".ico,.icon");
            dictionary.Add("JPEG Decoder", ".jpeg,.jpe,.jpg,.jfif,.exif");
            dictionary.Add("PNG Decoder", ".png");
            dictionary.Add("TIFF Decoder", ".tiff,.tif");
            dictionary.Add("WMPhoto Decoder", ".wdp,.jxr");
            dictionary.Add("DDS Decoder", ".dds");
            return dictionary;
        }
    }
}
