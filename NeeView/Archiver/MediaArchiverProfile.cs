using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class MediaArchiverProfile
    {
        public static MediaArchiverProfile Current { get; private set; }

        public MediaArchiverProfile()
        {
            Current = this;
        }

        [PropertyMember("動画のサポート")]
        public bool IsSupported { get; set; } = true;

        [PropertyMember("動画ファイルの拡張子", Tips = "Windows Media Player で再生できるものが、おおよそ再生可能です。")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".asf;.avi;.mpg;.mpeg;.mpe;.mp4;.mp4v;.mkv;.mov;.wmv");

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsSupported { get; set; }

            [DataMember]
            public string SupportFileTypes { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsSupported = this.IsSupported;
            memento.SupportFileTypes = this.SupportFileTypes.ToString();

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsSupported = memento.IsSupported;
            this.SupportFileTypes.FromString(memento.SupportFileTypes.ToString());
        }

        #endregion

    }
}
