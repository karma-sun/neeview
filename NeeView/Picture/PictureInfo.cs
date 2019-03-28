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
        /// ファイルサイズ
        /// </summary>
        public long Length { get; set; } = -1;

        /// <summary>
        /// 最終更新日
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// EXIF
        /// </summary>
        public BitmapExif Exif { get; set; }


        /// <summary>
        /// Archiver
        /// </summary>
        public string Archiver { get; set; }

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


        //
        public bool IsPixelInfoEnabled => BitsPerPixel > 0;

        //
        public PictureInfo()
        {
        }

        //
        public PictureInfo(ArchiveEntry entry)
        {
            this.Length = entry.Length;
            this.LastWriteTime = entry.LastWriteTime;
            this.Archiver = entry.Archiver?.ToString();
        }


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
