using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Windows;

namespace NeeView
{
    public class SevenZipArchiveConfig : BindableBase
    {
        private bool _isEnabled = true;

        [PropertyMember("@ParamSevenZipArchiverIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyPath("@ParamSevenZipArchiverX86DllPath", Tips = "@ParamSevenZipArchiverX86DllPathTips", Filter = "DLL|*.dll", DefaultFileName = "7z.dll")]
        public string X86DllPath { get; set; } = "";

        [PropertyPath("@ParamSevenZipArchiverX64DllPath", Tips = "@ParamSevenZipArchiverX64DllPathTips", Filter = "DLL|*.dll", DefaultFileName = "7z.dll")]
        public string X64DllPath { get; set; } = "";

        [PropertyMember("@ParamSevenZipArchiverSupportFileTypes")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".7z;.cb7;.cbr;.cbz;.lzh;.rar;.zip");
    }
}