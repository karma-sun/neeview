using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace NeeView
{
    public class MediaControl : BindableBase
    {
        static MediaControl() => Current = new MediaControl();
        public static MediaControl Current { get; }


        private MediaControl()
        {
        }

        public event EventHandler<MediaPlayerChanged> Changed;


        public void RiaseContentChanged(object sender, MediaPlayerChanged e)
        {
            Changed?.Invoke(sender, e);
        }


        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public bool IsMuted { get; set; }

            [DataMember]
            public double Volume { get; set; }

            [DataMember]
            public bool IsRepeat { get; set; }

            [DataMember]
            public double PageSeconds { get; set; }

            [DataMember, DefaultValue(0.5)]
            public double MediaStartDelaySeconds { get; set; }


            [OnDeserializing]
            public void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                config.Archive.Media.IsMuted = IsMuted;
                config.Archive.Media.Volume = Volume;
                config.Archive.Media.IsRepeat = IsRepeat;
                config.Archive.Media.PageSeconds = PageSeconds;
                config.Archive.Media.MediaStartDelaySeconds = MediaStartDelaySeconds;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsMuted = Config.Current.Archive.Media.IsMuted;
            memento.Volume = Config.Current.Archive.Media.Volume;
            memento.IsRepeat = Config.Current.Archive.Media.IsRepeat;
            memento.PageSeconds = Config.Current.Archive.Media.PageSeconds;
            memento.MediaStartDelaySeconds = Config.Current.Archive.Media.MediaStartDelaySeconds;

            return memento;
        }

        #endregion
    }

    /// <summary>
    /// MediaPlayer変更通知パラメータ
    /// </summary>
    public class MediaPlayerChanged : EventArgs
    {
        public MediaPlayerChanged()
        {
        }

        public MediaPlayerChanged(MediaPlayer player, Uri uri, bool isLastStart)
        {
            MediaPlayer = player;
            Uri = uri;
            IsLastStart = isLastStart;
        }

        public MediaPlayer MediaPlayer { get; set; }
        public Uri Uri { get; set; }
        public bool IsLastStart { get; set; }
        public bool IsValid => MediaPlayer != null;
    }
}
