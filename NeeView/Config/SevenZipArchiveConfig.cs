using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Windows;

namespace NeeView
{
    public class SevenZipArchiveConfig : BindableBase
    {
        private bool _isEnabled = true;
        private string _x86DllPath = "";
        private string _x64DllPath = "";
        private FileTypeCollection _supportFileTypes = new FileTypeCollection(".7z;.cb7;.cbr;.cbz;.lzh;.rar;.zip");


        [PropertyMember("@ParamSevenZipArchiverIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        [PropertyPath("@ParamSevenZipArchiverX86DllPath", Tips = "@ParamSevenZipArchiverX86DllPathTips", Filter = "DLL|*.dll", DefaultFileName = "7z.dll")]
        public string X86DllPath
        {
            get { return _x86DllPath; }
            set { SetProperty(ref _x86DllPath, value); }
        }

        [PropertyPath("@ParamSevenZipArchiverX64DllPath", Tips = "@ParamSevenZipArchiverX64DllPathTips", Filter = "DLL|*.dll", DefaultFileName = "7z.dll")]
        public string X64DllPath
        {
            get { return _x64DllPath; }
            set { SetProperty(ref _x64DllPath, value); }
        }

        [PropertyMember("@ParamSevenZipArchiverSupportFileTypes")]
        public FileTypeCollection SupportFileTypes
        {
            get { return _supportFileTypes; }
            set { SetProperty(ref _supportFileTypes, value); }
        }

    }
}