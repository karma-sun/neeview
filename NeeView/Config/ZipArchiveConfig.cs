using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ZipArchiveConfig : BindableBase
    {
        private bool _isEnabled = true;
        private FileTypeCollection _supportFileTypes = new FileTypeCollection(".zip");

        [PropertyMember("@ParamZipArchiverIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        [PropertyMember("@ParamZipArchiverSupportFileTypes", Tips = "@ParamZipArchiverSupportFileTypesTips")]
        public FileTypeCollection SupportFileTypes
        {
            get { return _supportFileTypes; }
            set { SetProperty(ref _supportFileTypes, value); }
        }

    }
}