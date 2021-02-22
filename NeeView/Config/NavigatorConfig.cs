using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class NavigatorConfig : BindableBase
    {
        private bool _isVisibleThumbnail;

        [PropertyMember]
        public bool IsVisibleThumbnail
        {
            get { return _isVisibleThumbnail; }
            set { SetProperty(ref _isVisibleThumbnail, value); }
        }
    }
}


