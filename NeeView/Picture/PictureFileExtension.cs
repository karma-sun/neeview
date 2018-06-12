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
            Dictionary<string, string> dictionary;

            try
            {
                dictionary = WicDecoders.ListUp();
            }
            catch
            {
                // 失敗した場合は標準設定にする
                dictionary = new Dictionary<string, string>();
                dictionary.Add("BMP Decoder", ".bmp,.dib,.rle");
                dictionary.Add("GIF Decoder", ".gif");
                dictionary.Add("ICO Decoder", ".ico,.icon");
                dictionary.Add("JPEG Decoder", ".jpeg,.jpe,.jpg,.jfif,.exif");
                dictionary.Add("PNG Decoder", ".png");
                dictionary.Add("TIFF Decoder", ".tiff,.tif");
                dictionary.Add("WMPhoto Decoder", ".wdp,.jxr");
                dictionary.Add("DDS Decoder", ".dds");
            }

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

        // サポートしている拡張子か (標準)
        public bool IsDefaultSupported(string fileName)
        {
            string ext = LoosePath.GetExtension(fileName);
            return _defaultExtensoins.Contains(ext);
        }

        // サポートしている拡張子か (Susie)
        public bool IsSusieSupported(string fileName)
        {
            if (!SusieContext.Current.IsEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return _susieExtensions.Contains(ext);
        }

        #endregion
    }
}
