using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ImageSvgConfig: BindableBase
    {
        private bool _isEnabled = true;

        // support SVG
        [PropertyMember("@ParamPictureProfileIsSvgEnabled", Tips = "@ParamPictureProfileIsSvgEnabledTips")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }
    }
}