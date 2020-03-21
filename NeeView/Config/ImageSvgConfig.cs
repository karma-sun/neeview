using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ImageSvgConfig: BindableBase
    {
        // support SVG
        [PropertyMember("@ParamPictureProfileIsSvgEnabled", Tips = "@ParamPictureProfileIsSvgEnabledTips")]
        public bool IsEnabled { get; set; } = true;
    }
}