using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Windows;

namespace NeeView
{
    public class ArchiveConfig : BindableBase
    {
        public ZipArchiveConfig Zip { get; set; } = new ZipArchiveConfig();

        public SevenZipArchiveConfig SevenZip { get; set; } = new SevenZipArchiveConfig();

        public PdfArchiveConfig Pdf { get; set; } = new PdfArchiveConfig();
      
        public MediaArchiveConfig Media { get; set; } = new MediaArchiveConfig();
    }


    public class ZipArchiveConfig : BindableBase
    {
        private bool _isEnabled = true;

        [PropertyMember("@ParamZipArchiverIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamZipArchiverSupportFileTypes", Tips = "@ParamZipArchiverSupportFileTypesTips")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".zip");
    }

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


    public class PdfArchiveConfig : BindableBase
    {
        private bool _isEnabled = true;
        private Size _renderSize = new Size(1920, 1080);


        [PropertyMember("@ParamArchiverPdfIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamArchiverPdfSupportFileTypes")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".pfd");

        [PropertyMember("@ParamArchiverPdfRenderSize", Tips = "@ParamArchiverPdfRenderSizeTips")]
        public Size RenderSize
        {
            get { return _renderSize; }
            set
            {
                if (_renderSize != value)
                {
                    _renderSize = new Size(Math.Max(value.Width, 256), Math.Max(value.Height, 256));
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class MediaArchiveConfig : BindableBase
    {
        private bool _isEnabled = true;

        [PropertyMember("@ParamArchiverMediaIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamArchiverMediaSupportFileTypes", Tips = "@ParamArchiverMediaSupportFileTypesTips")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".asf;.avi;.mp4;.mkv;.mov;.wmv");

        [PropertyMember("@ParamPageSeconds")]
        public double PageSeconds { get; set; } = 10.0;

        [PropertyMember("@ParamMediaStartDelaySeconds", Tips = "@ParamMediaStartDelaySecondsTips")]
        public double MediaStartDelaySeconds { get; set; } = 0.5;
    }
}