using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ImageStandardConfig: BindableBase
    {
        private bool _isAspectRatioEnabled;

        // 画像の解像度情報を表示に反映する
        [PropertyMember("@ParamPictureProfileIsAspectRatioEnabled", Tips = "@ParamPictureProfileIsAspectRatioEnabledTips")]
        public bool IsAspectRatioEnabled
        {
            get { return _isAspectRatioEnabled; }
            set { SetProperty(ref _isAspectRatioEnabled, value); }
        }

        // GIFアニメ有効
        [PropertyMember("@ParamBookIsEnableAnimatedGif", Tips = "@ParamBookIsEnableAnimatedGifTips")]
        public bool IsAnimatedGifEnabled { get; set; } = true;


        // サポート外ファイル有効のときに、すべてのファイルを画像とみなす
        [PropertyMember("@ParamBookIsAllFileAnImage", Tips = "@ParamBookIsAllFileAnImageTips")]
        public bool IsAllFileSupported { get; set; }
    }
}