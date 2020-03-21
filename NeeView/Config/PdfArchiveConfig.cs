using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Windows;

namespace NeeView
{
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
}