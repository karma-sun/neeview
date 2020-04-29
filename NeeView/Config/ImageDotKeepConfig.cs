using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Windows;

namespace NeeView
{
    public class ImageDotKeepConfig : BindableBase
    {
        private bool _isEnabled;
        private double _threshold = 1.0;

        [PropertyMember("@ParamDotKeepIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        [PropertyRange("@ParamDotKeepThreshold", 0.0, 5.0, TickFrequency = 0.1, IsEditable = true, Tips = "@ParamDotKeepThresholdTips")]
        public double Threshold
        {
            get { return _threshold; }
            set { SetProperty(ref _threshold, value); }
        }


        /// <summary>
        /// DotKeepで表示するかの判定
        /// </summary>
        /// <param name="viewSize">実際に表示するサイズ</param>
        /// <param name="sourceSize">画像データのサイズ</param>
        /// <returns></returns>
        public bool IsImgeDotKeep(Size viewSize, Size sourceSize)
        {
            return IsEnabled && viewSize.Width >= sourceSize.Width * Threshold && viewSize.Height >= sourceSize.Height * Threshold;
        }
    }
}