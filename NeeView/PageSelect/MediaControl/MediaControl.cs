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


        public bool IsMuted { get; set; }

        public double Volume { get; set; } = 0.5;

        public bool IsRepeat { get; set; }

        [PropertyMember("@ParamPageSeconds")]
        public double PageSeconds { get; set; } = 10.0;

        [PropertyMember("@ParamMediaStartDelaySeconds", Tips = "@ParamMediaStartDelaySecondsTips")]
        public double MediaStartDelaySeconds { get; set; } = 0.5;

        public void RiaseContentChanged(object sender, MediaPlayerChanged e)
        {
            Changed?.Invoke(sender, e);
        }


        #region Memento

        [DataContract]
        public class Memento
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
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsMuted = this.IsMuted;
            memento.Volume = this.Volume;
            memento.IsRepeat = this.IsRepeat;
            memento.PageSeconds = this.PageSeconds;
            memento.MediaStartDelaySeconds = this.MediaStartDelaySeconds;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsMuted = memento.IsMuted;
            this.Volume = memento.Volume;
            this.IsRepeat = memento.IsRepeat;
            this.PageSeconds = memento.PageSeconds;
            this.MediaStartDelaySeconds = memento.MediaStartDelaySeconds;
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
