using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public class ImageDotKeepConfig : BindableBase
    {
        private bool _isEnabled;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }
    }
}