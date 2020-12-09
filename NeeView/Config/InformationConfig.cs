using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

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
            set { SetProperty(ref _isVisibleBitsPerPixel, value); }
        }

        [PropertyMember("@ParamFileInformationIsVisibleLoader")]
        public bool IsVisibleLoader
        {
            get { return _isVisibleLoader; }
            set { SetProperty(ref _isVisibleLoader, value); }
        }

        [PropertyMember("@ParamFileInformationIsVisibleFilePath")]
        public bool IsVisibleFilePath
        {
            get { return _isVisibleFilePath; }
            set { SetProperty(ref _isVisibleFilePath, value); }
        }
    }
}


