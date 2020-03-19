using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public class LayoutConfig : BindableBase
    {
        public WindowLayoutConfig Window { get; set; } = new WindowLayoutConfig();
    }

}