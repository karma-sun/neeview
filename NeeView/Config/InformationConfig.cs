using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class InformationConfig : BindableBase
    {
        private bool _isVisibleBitsPerPixel;
        private bool _isVisibleLoader;
        private bool _isVisibleFilePath;

        [PropertyMember("@ParamFileInformationIsVisibleBitsPerPixel")]
        public bool IsVisibleBitsPerPixel
        {
            get { return _isVisibleBitsPerPixel; }
            set { if (_isVisibleBitsPerPixel != value) { _isVisibleBitsPerPixel = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamFileInformationIsVisibleLoader")]
        public bool IsVisibleLoader
        {
            get { return _isVisibleLoader; }
            set { if (_isVisibleLoader != value) { _isVisibleLoader = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamFileInformationIsVisibleFilePath")]
        public bool IsVisibleFilePath
        {
            get { return _isVisibleFilePath; }
            set { if (_isVisibleFilePath != value) { _isVisibleFilePath = value; RaisePropertyChanged(); } }
        }
    }
}


