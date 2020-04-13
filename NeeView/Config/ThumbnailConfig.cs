using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ThumbnailConfig : BindableBase
    {
        private bool _isCacheEnabled = true;
        private BitmapImageFormat _format = BitmapImageFormat.Jpeg;
        private int _quality = 80;
        private int _thumbnailBookCapacity = 200;
        private int _thumbnailPageCapacity = 100;
        private double _resolution = 256.0;


        [PropertyMember("@ParamThumbnailIsCacheEnabled", Tips = "@ParamThumbnailIsCacheEnabledTips")]
        public bool IsCacheEnabled
        {
            get { return _isCacheEnabled; }
            set { SetProperty(ref _isCacheEnabled, value); }
        }

        /// <summary>
        /// 画像サイズ
        /// </summary>
        [PropertyRange("@ParamThumbnailResolution", 64, 1024, TickFrequency = 64, Tips = "@ParamThumbnailResolutionTips")]
        public double Resolution
        {
            get { return _resolution; }
            set { SetProperty(ref _resolution, MathUtility.Clamp(value, 64, 1024)); }
        }

        /// <summary>
        /// 画像フォーマット
        /// </summary>
        [PropertyMember("@ParamThumbnailFormat", Tips = "@ParamThumbnailFormatTips")]
        public BitmapImageFormat Format
        {
            get { return _format; }
            set { SetProperty(ref _format, value); }
        }

        /// <summary>
        /// 画像品質
        /// </summary>
        [PropertyRange("@ParamThumbnailQuality", 5, 100, TickFrequency = 5, Tips = "@ParamThumbnailQualityTips")]
        public int Quality
        {
            get { return _quality; }
            set { SetProperty(ref _quality, MathUtility.Clamp(value, 5, 100)); }
        }

        [PropertyMember("@ParamThumbnailBookCapacity", Tips = "@ParamThumbnailBookCapacityTips")]
        public int ThumbnailBookCapacity
        {
            get { return _thumbnailBookCapacity; }
            set { SetProperty(ref _thumbnailBookCapacity, value); }
        }

        [PropertyMember("@ParamThumbnailPageCapacity", Tips = "@ParamThumbnailPageCapacityTips")]
        public int ThumbnailPageCapacity
        {
            get { return _thumbnailPageCapacity; }
            set { SetProperty(ref _thumbnailPageCapacity, value); }
        }

    }
}