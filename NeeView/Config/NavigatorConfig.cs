using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class NavigatorConfig : BindableBase
    {
        private bool _isVisibleThumbnail;
        private double _thumbnailHeight = 256.0;

        [PropertyMember]
        public bool IsVisibleThumbnail
        {
            get { return _isVisibleThumbnail; }
            set { SetProperty(ref _isVisibleThumbnail, value); }
        }

        [PropertyMember]
        public double ThumbnailHeight
        {
            get { return _thumbnailHeight; }
            set { SetProperty(ref _thumbnailHeight, value); }
        }

    }
}


