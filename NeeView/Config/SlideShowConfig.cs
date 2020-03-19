using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class SlideShowConfig : BindableBase
    {
        /// <summary>
        /// 起動時の自動開始
        /// </summary>
        [PropertyMember("@ParamIsAutoPlaySlideShow")]
        public bool IsAutoPlaySlideShow { get; set; }
    }
}