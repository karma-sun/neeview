using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class MainViewConfig : BindableBase
    {
        private bool _isFloating;

        [PropertyMember("@ParamMainViewIsFloating")]
        public bool IsFloating
        {
            get { return _isFloating; }
            set { SetProperty(ref _isFloating, value); }
        }
    }
}


