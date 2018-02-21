using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace NeeView
{
    public class MediaControl : BindableBase
    {
        public static MediaControl Current { get; private set; }

        private MediaViewContent _viewContent;

        public MediaControl()
        {
            Current = this;

            ContentCanvas.Current.ContentChanged += ContentCanvas_ContentChanged;
        }

        public event EventHandler<MediaPlayerChanged> Changed;


        public bool IsMuted { get; set; }

        public double Volume { get; set; } = 0.5;

        public bool IsRepeat { get; set; }

        [PropertyMember("ページ移動での変化時間(秒)")]
        public double PageSeconds { get; set; } = 10.0;


        private void ContentCanvas_ContentChanged(object sender, EventArgs e)
        {
            if (_viewContent != null)
            {
                _viewContent.Unloaded -= ViewContent_Unloaded;
                _viewContent = null;
            }

            var viewContent = ContentCanvas.Current.MainContent;

            if (viewContent is MediaViewContent mediaViewContent)
            {
                _viewContent = mediaViewContent;
                _viewContent.Unloaded += ViewContent_Unloaded;
                Changed?.Invoke(this, new MediaPlayerChanged(_viewContent.MediaPlayer, _viewContent.MediaUri, _viewContent.IsLastStart));
            }
            else
            {
                Changed?.Invoke(this, new MediaPlayerChanged());
            }
        }

        private void ViewContent_Unloaded(object sender, EventArgs e)
        {
            if (_viewContent == sender)
            {
                _viewContent = null;
                Changed?.Invoke(this, new MediaPlayerChanged());
            }
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
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsMuted = this.IsMuted;
            memento.Volume = this.Volume;
            memento.IsRepeat = this.IsRepeat;
            memento.PageSeconds = this.PageSeconds;

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
