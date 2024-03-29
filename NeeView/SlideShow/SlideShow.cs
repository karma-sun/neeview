﻿using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// スライドショー管理
    /// </summary>
    public class SlideShow : BindableBase, IDisposable
    {
        static SlideShow() => Current = new SlideShow();
        public static SlideShow Current { get; }

        // タイマーディスパッチ
        private DispatcherTimer _timer;

        // スライドショー表示間隔用
        private DateTime _lastShowTime;

        //
        private const double _minTimerTick = 0.01;
        private const double _maxTimerTick = 0.2;

        // レジューム状態
        private bool _isPlayingSlideShowMemento;


        // コンストラクター
        private SlideShow()
        {
            // timer for slideshow
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(_maxTimerTick);
            _timer.Tick += new EventHandler(DispatcherTimer_Tick);

            BookOperation.Current.ViewContentsChanged +=
                (s, e) => ResetTimer();

            MainWindow.Current.PreviewKeyDown +=
                (s, e) => ResetTimer();

            MainViewComponent.Current.MouseInput.MouseMoved +=
                (s, e) => { if (Config.Current.SlideShow.IsCancelSlideByMouseMove) ResetTimer(); };

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }

        /// <summary>
        /// スライドショー再生状態
        /// </summary>
        private bool _IsPlayingSlideShow;
        public bool IsPlayingSlideShow
        {
            get { return _IsPlayingSlideShow; }
            set
            {
                if (_IsPlayingSlideShow != value)
                {
                    _IsPlayingSlideShow = value;
                    if (_IsPlayingSlideShow)
                    {
                        Play();
                    }
                    else
                    {
                        Stop();
                    }
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// スライドショー再生/停止切り替え
        /// </summary>
        public void TogglePlayingSlideShow()
        {
            this.IsPlayingSlideShow = !this.IsPlayingSlideShow;
        }


        /// <summary>
        /// 一時停止
        /// </summary>
        public void PauseSlideShow()
        {
            _isPlayingSlideShowMemento = IsPlayingSlideShow;
            IsPlayingSlideShow = false;
        }

        /// <summary>
        /// 再開
        /// </summary>
        public void ResumeSlideShow()
        {
            IsPlayingSlideShow = _isPlayingSlideShowMemento;
        }

        /// <summary>
        /// 次のスライドへ移動：スライドショー専用
        /// </summary>
        public void NextSlide()
        {
            BookOperation.Current.NextSlide(this);
        }

        /// <summary>
        /// 再生開始
        /// </summary>
        private void Play()
        {
            if (_disposedValue) return;

            // インターバル時間を修正する
            _timer.Interval = TimeSpan.FromSeconds(MathUtility.Clamp(Config.Current.SlideShow.SlideShowInterval * 0.5, _minTimerTick, _maxTimerTick));
            _lastShowTime = DateTime.Now;
            _timer.Start();
        }

        /// <summary>
        /// 再生停止
        /// </summary>
        private void Stop()
        {
            _timer.Stop();
        }


        /// <summary>
        /// 再生中のタイマー処理
        /// </summary>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            // マウスボタンが押されていたらキャンセル
            if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
            {
                _lastShowTime = DateTime.Now;
                return;
            }

            // スライドショーのインターバルを非アクティブ時間で求める
            if ((DateTime.Now - _lastShowTime).TotalSeconds >= Config.Current.SlideShow.SlideShowInterval)
            {
                if (!BookHub.Current.IsLoading) NextSlide();
                _lastShowTime = DateTime.Now;
            }
        }

        /// <summary>
        /// スライドショータイマーリセット
        /// </summary>
        public void ResetTimer()
        {
            if (!_timer.IsEnabled) return;
            _lastShowTime = DateTime.Now;
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public double SlideShowInterval { get; set; }

            [DataMember]
            public bool IsCancelSlideByMouseMove { get; set; }

            [DataMember]
            public bool IsSlideShowByLoop { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsAutoPlaySlideShow { get; set; }


            public void RestoreConfig(Config config)
            {
                config.SlideShow.SlideShowInterval = SlideShowInterval;
                config.SlideShow.IsCancelSlideByMouseMove = IsCancelSlideByMouseMove;
                config.SlideShow.IsSlideShowByLoop = IsSlideShowByLoop;
                config.StartUp.IsAutoPlaySlideShow = IsAutoPlaySlideShow;
            }
        }

        #endregion

    }
}
