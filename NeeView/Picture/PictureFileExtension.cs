using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// 画像ファイル拡張子
    /// </summary>
    public class PictureFileExtension
    {
        private FileTypeCollection _defaultExtensoins = new FileTypeCollection();


        public PictureFileExtension()
        {
            UpdateDefaultSupprtedFileTypes();

            Config.Current.Image.Standard.AddPropertyChanged(nameof(ImageStandardConfig.UseWicInformation), (s, e) => UpdateDefaultSupprtedFileTypes());
        }


        public FileTypeCollection DefaultExtensions => _defaultExtensoins;


        // デフォルトローダーのサポート拡張子を更新
        private void UpdateDefaultSupprtedFileTypes()
        {
            var list = new List<string>();

            var collection = AppDispatcher.Invoke(() => CreateSystemExtensions());
            foreach (var pair in collection)
            {
                list.AddRange(pair.Value.Split(','));
            }

            _defaultExtensoins.Restore(list);

            Debug.WriteLine($"DefaultExtensions: {_defaultExtensoins}");
        }

        // 標準対応拡張子取得
        private Dictionary<string, string> CreateSystemExtensions()
        {
            if (Config.Current.Image.Standard.UseWicInformation)
            {
                try
                {
                    return WicDecoders.ListUp();
                }
                catch
                {
                    return CreateDefaultExtensions();
                }
            }
            else
            {
                return CreateDefaultExtensions();
            }
        }

        private Dictionary<string, string> CreateDefaultExtensions()
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
