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


        private bool _isEnabled = true;

        private MediaArchiverProfile()
        {
        }

        [PropertyMember("@ParamArchiverMediaIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamArchiverMediaSupportFileTypes", Tips = "@ParamArchiverMediaSupportFileTypesTips")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".asf;.avi;.mp4;.mkv;.mov;.wmv");

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
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsEnabled = this.IsEnabled;
            memento.SupportFileTypes = this.SupportFileTypes.OneLine;

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsEnabled = memento.IsEnabled;
            this.SupportFileTypes.OneLine = memento.SupportFileTypes;
        }

        #endregion

    }
}
