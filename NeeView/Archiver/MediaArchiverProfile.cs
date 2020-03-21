using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    public class MediaArchiverProfile : BindableBase
    {
        static MediaArchiverProfile() => Current = new MediaArchiverProfile();
        public static MediaArchiverProfile Current { get; }


        ////private bool _isEnabled = true;

        private MediaArchiverProfile()
        {
        }

#if false
        [PropertyMember("@ParamArchiverMediaIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamArchiverMediaSupportFileTypes", Tips = "@ParamArchiverMediaSupportFileTypesTips")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".asf;.avi;.mp4;.mkv;.mov;.wmv");
#endif

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public bool IsEnabled { get; set; }

            [DataMember]
            public string SupportFileTypes { get; set; }

            public void RestoreConfig()
            {
                Config.Current.Archive.Media.IsEnabled = IsEnabled;
                Config.Current.Archive.Media.SupportFileTypes.OneLine = SupportFileTypes;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsEnabled = Config.Current.Archive.Media.IsEnabled;
            memento.SupportFileTypes = Config.Current.Archive.Media.SupportFileTypes.OneLine;

            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            ////this.IsEnabled = memento.IsEnabled;
            ////this.SupportFileTypes.OneLine = memento.SupportFileTypes;
        }

        #endregion

    }
}
