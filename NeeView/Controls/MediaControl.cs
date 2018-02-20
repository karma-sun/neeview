using NeeLaboratory.ComponentModel;
using System;
using System.Windows.Media;

namespace NeeView
{
    public class MediaPlayerProfile : BindableBase
    {
        public static MediaPlayerProfile Current { get; private set; }

        public MediaPlayerProfile()
        {
            Current = this;
        }

        public bool IsMuted { get; set; }

        public double Volume { get; set; } = 0.5;

        public bool IsRepat { get; set; }
    }


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
