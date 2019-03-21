using PhotoSauce.MagicScaler;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Bitmap生成パラメータ
    /// </summary>
    public class BitmapCreateSetting
    {
        /// <summary>
        /// Bitmap生成モード
        /// </summary>
        public BitmapCreateMode Mode { get; set; }

        /// <summary>
        /// アスペクト比を維持
        /// </summary>
        public bool IsKeepAspectRatio { get; set; }

        /// <summary>
        /// リサイズパラメータ
        /// </summary>
        public ProcessImageSettings ProcessImageSettings { get; set; }

        /// <summary>
        /// 生成元として使用可能なBitmap。
        /// 指定されない場合もある
        /// </summary>
        public BitmapSource Source { get; set; }
    }
}
