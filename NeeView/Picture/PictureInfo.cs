using NeeView.Media.Imaging.Metadata;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 画像情報
    /// </summary>
    public class PictureInfo
    {
        private bool _isPixelInfoInitialized;
        private Size _aspectSize = Size.Empty;


        public PictureInfo()
        {
        }


        /// <summary>
        /// Bitmap画像のRaw情報
        /// </summary>
        public BitmapInfo BitmapInfo { get; set; }

        /// <summary>
        /// 画像サイズ
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// 本来の画像サイズ
        /// </summary>
        public Size OriginalSize { get; set; }

        /// <summary>
        /// 画像サイズが制限された本来の画像サイズと異なる値である
        /// </summary>
        public bool IsLimited => Size != OriginalSize;

        /// <summary>
        /// 画像解像度を適用したサイズ
        /// </summary>
        public Size AspectSize
        {
            get => _aspectSize.IsEmpty ? Size : _aspectSize;
            set => _aspectSize = value;
        }

        /// <summary>
        /// Metadata
        /// </summary>
        public BitmapMetadataDatabase Metadata { get; set; }

        /// <summary>
        /// Decoder
        /// </summary>
        public string Decoder { get; set; }


        // 実際に読み込まないとわからないもの

        /// <summary>
        /// 基本色
        /// </summary>
        public Color Color { get; set; } = Colors.Black;

        /// <summary>
        /// ピクセル深度
        /// </summary>
        public int BitsPerPixel { get; set; }

        public bool IsPixelInfoEnabled => BitsPerPixel > 0;



        /// <summary>
        /// 画素情報。
        /// </summary>
        public void SetPixelInfo(BitmapSource bitmap)
        {
            // 設定は1回だけで良い
            if (_isPixelInfoInitialized) return;
            _isPixelInfoInitialized = true;

            // 補助情報なので重要度は低いので、取得できなくても問題ない。
            try
            {
                this.Color = bitmap.GetOneColor();
            }
            catch
            {
            }
        }

    }
}
