using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Diagnostics;
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
        /// <summary>
        /// 現在有効なMediaPlayerOperator。
        /// シングルトンではない。
        /// </summary>
        public static MediaPlayerOperator Current { get; set; }

        #region Fields

        private MediaPlayer _player;
        private DispatcherTimer _timer;

        private bool _isLastStart;
        private bool _isTimeLeftDisp;

        private Duration _duration;
        private TimeSpan _durationTimeSpan = TimeSpan.FromMilliseconds(1.0);
        private TimeSpan _position;

        private bool _isActive;
        private bool _isPlaying;
        private bool _isRepeat;
        private bool _isScrubbing;
        private double _volume = 0.5;
        private double _delay;

        #endregion

        #region Constructors

        public MediaPlayerOperator(MediaPlayer player)
        {
            _player = player;

            _player.ScrubbingEnabled = true;

            _player.MediaOpened += Player_MediaOpened;
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaFailed += Player_MediaFailed;

            this.IsMuted = Config.Current.Archive.Media.IsMuted;
            this.Volume = Config.Current.Archive.Media.Volume;
            this.IsRepeat = Config.Current.Archive.Media.IsRepeat;

            _timer = new DispatcherTimer(DispatcherPriority.Normal, App.Current.Dispatcher);
            _timer.Interval = TimeSpan.FromSeconds(0.1);
            _timer.Tick += DispatcherTimer_Tick;
            _timer.Start();
        }


        #endregion

        /// <summary>
        /// 再生が終端に達したときのイベント
        /// </summary>
        public event EventHandler MediaEnded;

        #region Properties

        public MediaPlayer MediaPlayer
        {
            get { return _player; }
            set { if (_player != value) { _player = value; RaisePropertyChanged(); } }
        }

        public Duration Duration
        {
            get { return _duration; }
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    _durationTimeSpan = MathUtility.Max(_duration.HasTimeSpan ? _duration.TimeSpan : TimeSpan.Zero, TimeSpan.FromMilliseconds(1.0));
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(DurationHasTimeSpan));
                }
            }
        }

        public bool DurationHasTimeSpan
        {
            get { return _duration.HasTimeSpan; }
        }


        public TimeSpan Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    SetPositionInner(value);

                    if (_duration.HasTimeSpan)
                    {
                        _player.Position = _position;
                    }
                }
            }
        }

        private void SetPositionInner(TimeSpan position)
        {
            _position = MathUtility.Clamp(position, TimeSpan.Zero, _durationTimeSpan);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(PositionRate));
            RaisePropertyChanged(nameof(DispTime));
        }

        // [0..1] for slider
        public double PositionRate
        {
            get { return (double)_position.Ticks / _durationTimeSpan.Ticks; }
            set { this.Position = TimeSpan.FromTicks((long)(_durationTimeSpan.Ticks * value)); }
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

                    Config.Current.Archive.Media.Volume = _volume;
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

                var now = _position;
                var total = _durationTimeSpan;
                var left = total - now;

                var totalString = total.GetHours() > 0 ? $"{total.GetHours()}:{total.Minutes:00}:{total.Seconds:00}" : $"{total.Minutes}:{total.Seconds:00}";

                var nowString = _isTimeLeftDisp
                    ? left.GetHours() > 0 ? $"-{left.GetHours()}:{left.Minutes:00}:{left.Seconds:00}" : $"-{left.Minutes}:{left.Seconds:00}"
                    : now.GetHours() > 0 ? $"{now.GetHours()}:{now.Minutes:00}:{now.Seconds:00}" : $"{now.Minutes}:{now.Seconds:00}";

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

                    Config.Current.Archive.Media.IsRepeat = _isRepeat;
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
                Config.Current.Archive.Media.IsMuted = _player.IsMuted;
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
                            UpdateVolume();
                        }
                        else
                        {
                            Resume();
                            _player.Position = _position;
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


        private void Player_MediaFailed(object sender, ExceptionEventArgs e)
        {
            Dispose();
        }

        private void Player_MediaEnded(object sender, EventArgs e)
        {
            if (_isRepeat)
            {
                _player.Position = TimeSpan.FromMilliseconds(1);
            }
            else
            {
                Debug.WriteLine($"END");
                _player.Pause();
                if (_duration.HasTimeSpan)
                {
                    _player.Position = _durationTimeSpan;
                }
                MediaEnded?.Invoke(this, null);
            }
        }

        private void Player_MediaOpened(object sender, EventArgs e)
        {
            Duration = _player.NaturalDuration;

            if (_isLastStart && _duration.HasTimeSpan)
            {
                // 最終フレームからの開始
                _player.Position = _duration.TimeSpan;
                Play();
                SetPositionLast();
            }
            else
            {
                // 最初からの開始
                _delay = Config.Current.Archive.Media.MediaStartDelaySeconds * 1000;
                if (_delay <= 0.0)
                {
                    // 画面のちらつきを許容してすぐに再生する
                    Play();
                }
                else
                {
                    // 画面がちらつくことがあるので、少し待ってから再生開始
                    _timer.Tick += DispatcherTimer_StartTick;
                }
            }
        }


        // 遅延再生開始用のタイマー処理
        private void DispatcherTimer_StartTick(object sender, EventArgs e)
        {
            _delay -= _timer.Interval.TotalMilliseconds;
            if (_delay < 0.0)
            {
                _timer.Tick -= DispatcherTimer_StartTick;
                Play();
            }
        }

        // 通常用タイマー処理
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_disposed) return;
            if (!_isActive || _isScrubbing) return;

            if (_duration.HasTimeSpan)
            {
                SetPositionInner(_player.Position);
            }

            Delay_Tick(_timer.Interval.TotalMilliseconds);
        }

        public void Open(Uri uri, bool isLastStart)
        {
            if (_disposed) return;

            _isLastStart = isLastStart;

            _player.Volume = 0.0;
            _player.Open(uri);
        }

        public void Play()
        {
            if (_disposed) return;

            _isActive = true;

            _player.Play();
            UpdateVolume();

            this.IsPlaying = true;
        }

        public void Pause()
        {
            if (_disposed) return;

            _player.Pause();

            IsPlaying = false;
        }

        /// <summary>
        /// コマンドによる移動
        /// </summary>
        /// <param name="delta"></param>
        /// <returns>終端を超える場合はtrue</returns>
        public bool AddPosition(TimeSpan delta)
        {
            if (_disposed) return false;
            if (!_duration.HasTimeSpan) return false;

            var t0 = _position;
            var t1 = _position + delta;

            SetPosition(t1);

            if (delta < TimeSpan.Zero && t0 < TimeSpan.FromSeconds(0.5))
            {
                return true;
            }
            if (delta >TimeSpan.Zero && t1 > _durationTimeSpan)
            {
                return true;
            }

            return false;
        }

        public void SetPositionFirst()
        {
            SetPosition(TimeSpan.Zero);
        }

        public void SetPositionLast()
        {
            SetPosition(_durationTimeSpan);
        }

        // コマンドによる移動[0..1]
        public void SetPosition(TimeSpan position)
        {
            if (_disposed) return;
            if (!_duration.HasTimeSpan) return;

            _delay = Config.Current.Archive.Media.MediaStartDelaySeconds * 1000;
            if (_delay <= 0.0)
            {
                this.Position = position;
            }
            else
            {
                UpdateVolume();
                _player.Pause();
                this.Position = position;
            }
        }

        // 移動による遅延再生処理用
        private void Delay_Tick(double ms)
        {
            if (_disposed) return;
            if (_delay <= 0.0) return;

            if (_isScrubbing)
            {
                _delay = 0.0;
                return;
            }

            _delay -= ms;
            if (_delay <= 0.0)
            {
                Resume();
            }
        }

        //
        private void Resume()
        {
            if (_disposed) return;

            if (_isPlaying && (_isRepeat || _position < _durationTimeSpan))
            {
                _player.Play();
                UpdateVolume();
            }
        }

        //
        private void UpdateVolume()
        {
            _player.Volume = _isScrubbing || _delay > 0.0 ? 0.0 : _volume;
        }

        public void AddVolume(double delta)
        {
            Volume = MathUtility.Clamp(Volume + delta, 0.0, 1.0);
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
                    MediaEnded = null;
                    _timer.Stop();
                    _player.MediaFailed -= Player_MediaFailed;
                    _player.MediaOpened -= Player_MediaOpened;
                    _player.MediaEnded -= Player_MediaEnded;
                    _player.Stop();
                    _player.Close();
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

    public static class TimeSpanExtensions
    {
        public static int GetHours(this TimeSpan timeSpan)
        {
            return Math.Abs(timeSpan.Days * 24 + timeSpan.Hours);
        }
    }
}
