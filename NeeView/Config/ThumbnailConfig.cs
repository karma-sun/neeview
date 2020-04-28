using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;

namespace NeeView
{
    public class ThumbnailConfig : BindableBase
    {
        private bool _isCacheEnabled = true;
        private TimeSpan _cacheLimitSpan;
        private BitmapImageFormat _format = BitmapImageFormat.Jpeg;
        private int _quality = 80;
        private int _thumbnailBookCapacity = 200;
        private int _thumbnailPageCapacity = 100;
        private double _resolution = 256.0;
        private string _thumbnailCacheFilePath;


        [PropertyMember("@ParamThumbnailIsCacheEnabled", Tips = "@ParamThumbnailIsCacheEnabledTips")]
        public bool IsCacheEnabled
        {
            get { return _isCacheEnabled; }
            set { SetProperty(ref _isCacheEnabled, value); }
        }

        // キャッシュの保存場所
        [PropertyPath("@ParamThumbnailCacheFilePath", FileDialogType = FileDialogType.SaveFile, Filter = "DB|*.db")]
        public string ThumbnailCacheFilePath
        {
            get { return _thumbnailCacheFilePath; }
            set { SetProperty(ref _thumbnailCacheFilePath, string.IsNullOrWhiteSpace(value) || value == ThumbnailCache.DefaultThumbnailCacheFilePath ? null : value); }
        }

        /// <summary>
        /// キャッシュ制限(時間)
        /// </summary>
        [PropertyMember("@ParamThumbnailCacheLimitSpan")]
        public TimeSpan CacheLimitSpan
        {
            get { return _cacheLimitSpan; }
            set { SetProperty(ref _cacheLimitSpan, value); }
        }

        /// <summary>
        /// 画像サイズ
        /// </summary>
        [PropertyRange("@ParamThumbnailResolution", 64, 512, TickFrequency = 64, IsEditable = true, Tips = "@ParamThumbnailResolutionTips")]
        public double Resolution
        {
            get { return _resolution; }
            set { SetProperty(ref _resolution, MathUtility.Max(value, 64)); }
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




        /// <summary>
        /// サムネイル画像生成パラメータのハッシュ値
        /// </summary>
        public int GetThumbnailImageGenerateHash()
        {
            int hash = (int)Resolution;
            if (Format == BitmapImageFormat.Jpeg)
            {
                hash |= 0x40000000;
                hash |= Quality << 16;
            }
            return hash;
        }
    }
}