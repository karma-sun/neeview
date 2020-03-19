using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class WindowLayoutConfig : BindableBase
    {
        [PropertyMember("@ParamIsCaptionEmulateInFullScreen", Tips = "@ParamIsCaptionEmulateInFullScreenTips")]
        public bool IsCaptionEmulateInFullScreen { get; set; }
    }

}