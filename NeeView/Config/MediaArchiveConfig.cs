using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
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


        [PropertyMapIgnore]
        public bool IsMuted { get; set; }
        
        [PropertyMapIgnore]
        public double Volume { get; set; } = 0.5;

        [PropertyMapIgnore]
        public bool IsRepeat { get; set; }
    }
}