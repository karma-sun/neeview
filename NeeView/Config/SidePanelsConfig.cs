using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class SidePanelsConfig : BindableBase
    {
        private double _opacity = 1.0;


        [PropertyPercent("@ParamSidePanelOpacity", Tips = "@ParamSidePanelOpacityTips")]
        public double Opacity
        {
            get { return _opacity; }
            set { SetProperty(ref _opacity, value); }
        }
    }
}