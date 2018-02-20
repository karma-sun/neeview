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

        private bool _isMuted;
        public bool IsMuted
        {
            get { return _isMuted; }
            set { if (_isMuted != value) { _isMuted = value; RaisePropertyChanged(); } }
        }

        private double _volume = 0.5;
        public double Volume
        {
            get { return _volume; }
            set { if (_volume != value) { _volume = value; RaisePropertyChanged(); } }
        }

        private bool _isRepeat;
        public bool IsRepeat
        {
            get { return _isRepeat; }
            set { if (_isRepeat != value) { _isRepeat = value; RaisePropertyChanged(); } }
        }


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
                Changed?.Invoke(this, new MediaPlayerChanged(_viewContent.MediaPlayer, _viewContent.MediaUri));
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
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsMuted = this.IsMuted;
            memento.Volume = this.Volume;
            memento.IsRepeat = this.IsRepeat;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsMuted = memento.IsMuted;
            this.Volume = memento.Volume;
            this.IsRepeat = memento.IsRepeat;
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

        public MediaPlayerChanged(MediaPlayer player, Uri uri)
        {
            MediaPlayer = player;
            Uri = uri;
        }

        public MediaPlayer MediaPlayer { get; set; }
        public Uri Uri { get; set; }
        public bool IsValid => MediaPlayer != null;
    }
}
