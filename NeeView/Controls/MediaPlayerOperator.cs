using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// MediaPlayer操作
    /// </summary>
    public class MediaPlayerOperator : BindableBase, IDisposable
    {
        #region Fields

        private MediaPlayer _player;
        private Rect _rect;
        private Duration _duration;
        private DispatcherTimer _timer;
        private bool _isActive;
        private bool _isTimeLeftDisp;
        private double _totalMilliseconds = 1.0;
        private double _position;
        private double _volume = 0.5;
        private bool _isPlaying;
        private bool _isRepeat;
        private bool _isScrubbing;

        #endregion

        #region Constructors

        public MediaPlayerOperator(MediaPlayer player)
        {
            _player = player;

            _player.ScrubbingEnabled = false;

            _rect = new Rect(0, 0, 704, 396);

            _player.MediaOpened += Player_MediaOpened;
            _player.MediaEnded += Player_MediaEnded;

            this.IsMuted = MediaPlayerProfile.Current.IsMuted;
            this.Volume = MediaPlayerProfile.Current.Volume;
            this.IsRepeat = MediaPlayerProfile.Current.IsRepat;

            _timer = new DispatcherTimer(DispatcherPriority.Normal, App.Current.Dispatcher);
            _timer.Interval = TimeSpan.FromSeconds(0.1);
            _timer.Tick += new EventHandler(DispatcherTimer_Tick);
        }

        #endregion

        #region Properties

        public MediaPlayer MediaPlayer
        {
            get { return _player; }
            set { if (_player != value) { _player = value; RaisePropertyChanged(); } }
        }

        public Rect Rect
        {
            get { return _rect; }
            set { if (_rect != value) { _rect = value; RaisePropertyChanged(); } }
        }

        public Duration Duration
        {
            get { return _duration; }
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    TotalMilliseconds = _duration.TimeSpan.TotalMilliseconds;
                    RaisePropertyChanged();
                }
            }
        }

        public double TotalMilliseconds
        {
            get { return _totalMilliseconds; }
            private set { _totalMilliseconds = Math.Max(1.0, value); }
        }

        // [0..1]
        public double Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    _player.Position = TimeSpan.FromMilliseconds(_position * _totalMilliseconds);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(DispTime));
                }
            }
        }

        // [0..1]
        public double Volume
        {
            get { return _volume; }
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    UpdateVolume();
                    RaisePropertyChanged();

                    MediaPlayerProfile.Current.Volume = _volume;
                }
            }
        }

        public bool IsTimeLeftDisp
        {
            get { return _isTimeLeftDisp; }
            set { if (_isTimeLeftDisp != value) { _isTimeLeftDisp = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(DispTime)); } }
        }

        public string DispTime
        {
            get
            {
                if (!_duration.HasTimeSpan) return null;

                var now = TimeSpan.FromMilliseconds(_position * _totalMilliseconds);
                var total = _duration.TimeSpan;
                var left = total - now;

                var totalString = total.Hours > 0 ? $"{total.Hours}:{total.Minutes:00}:{total.Seconds:00}" : $"{total.Minutes}:{total.Seconds:00}";

                var nowString = _isTimeLeftDisp
                    ? left.Hours > 0 ? $"-{left.Hours}:{left.Minutes:00}:{left.Seconds:00}" : $"-{left.Minutes}:{left.Seconds:00}"
                    : now.Hours > 0 ? $"{now.Hours}:{now.Minutes:00}:{now.Seconds:00}" : $"{now.Minutes}:{now.Seconds:00}";

                return nowString + " / " + totalString;
            }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { if (_isPlaying != value) { _isPlaying = value; RaisePropertyChanged(); } }
        }

        public bool IsRepeat
        {
            get { return _isRepeat; }
            set
            {
                if (_isRepeat != value)
                {
                    _isRepeat = value;
                    RaisePropertyChanged();

                    MediaPlayerProfile.Current.IsRepat = _isRepeat;
                }
            }
        }

        public bool IsMuted
        {
            get { return _player.IsMuted; }
            set
            {
                _player.IsMuted = value;
                RaisePropertyChanged();
                MediaPlayerProfile.Current.IsMuted = _player.IsMuted;
            }
        }

        public bool IsScrubbing
        {
            get { return _isScrubbing; }
            set
            {
                if (_disposed) return;

                if (_isScrubbing != value)
                {
                    _isScrubbing = value;

                    if (_isActive)
                    {
                        if (_isScrubbing)
                        {
                            _player.Pause();
                            _player.ScrubbingEnabled = true;
                            UpdateVolume();
                        }
                        else
                        {
                            if (_isPlaying)
                            {
                                _player.Play();
                                _player.ScrubbingEnabled = false;
                                UpdateVolume();
                            }
                        }
                    }

                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Commands

        //
        private RelayCommand _PlayCommand;
        public RelayCommand PlayCommand
        {
            get { return _PlayCommand = _PlayCommand ?? new RelayCommand(PlayCommand_Executed); }
        }

        private void PlayCommand_Executed()
        {
            if (!IsPlaying)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        //
        private RelayCommand _RepeatCommand;
        public RelayCommand RepeatCommand
        {
            get { return _RepeatCommand = _RepeatCommand ?? new RelayCommand(RepeatCommand_Executed); }
        }

        private void RepeatCommand_Executed()
        {
            IsRepeat = !IsRepeat;
        }

        //
        private RelayCommand _MuteCommand;
        public RelayCommand MuteCommand
        {
            get { return _MuteCommand = _MuteCommand ?? new RelayCommand(MuteCommand_Executed); }
        }

        private void MuteCommand_Executed()
        {
            IsMuted = !IsMuted;
        }

        #endregion

        #region Methods

        private void Player_MediaEnded(object sender, EventArgs e)
        {
            if (_isRepeat)
            {
                _player.Position = TimeSpan.FromMilliseconds(1.0);
            }
            else
            {
                Pause();
            }
        }

        private void Player_MediaOpened(object sender, EventArgs e)
        {
            _rect = new Rect(new Size(_player.NaturalVideoWidth, _player.NaturalVideoHeight));
            _volume = _player.Volume;
            Duration = _player.NaturalDuration;
            RaisePropertyChanged(null);

            // 自動再生開始
            Play();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_disposed) return;
            if (_isScrubbing) return;

            _position = _player.Position.TotalMilliseconds / _totalMilliseconds;
            RaisePropertyChanged(nameof(Position));
            RaisePropertyChanged(nameof(DispTime));
        }

        public void Open(Uri uri)
        {
            if (_disposed) return;

            _player.Open(uri);
        }

        public void Play()
        {
            if (_disposed) return;

            _player.Play();
            _timer.Start();
            _isActive = true;
            IsPlaying = true;

            if (_position >= 1.0)
            {
                _player.Position = TimeSpan.FromMilliseconds(1.0);
            }

            UpdateVolume();
        }

        public void Stop()
        {
            if (_disposed) return;

            _player.Stop();
            _timer.Stop();
            _isActive = false;
            IsPlaying = false;
        }

        public void Pause()
        {
            if (_disposed) return;

            _player.Pause();
            _timer.Stop();
            IsPlaying = false;
        }

        private void UpdateVolume()
        {
            _player.Volume = _isScrubbing ? 0.0 : _volume;
        }

        #endregion

        #region IDisposable Support

        private bool _disposed = false; 

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();
                    _player.MediaOpened -= Player_MediaOpened;
                    _player.MediaEnded -= Player_MediaEnded;
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }
}
