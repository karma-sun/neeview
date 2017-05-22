// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
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
    /// 能動的に動作する機能を持つオブジェクト。スレッドやタイマーで非同期に実行されるもの等。
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// エンジン始動
        /// </summary>
        void StartEngine();

        /// <summary>
        /// エンジン停止
        /// </summary>
        void StopEngine();
    }



    /// <summary>
    /// スライドショー管理
    /// </summary>
    public class SlideShow : BindableBase, IEngine
    {
        // System object
        public static SlideShow Current { get; private set; }


        //
        private BookHub _bookHub;
        private BookOperation _bookOperation;

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
        public SlideShow(BookHub bookHub, BookOperation bookOperation, MouseInput mouseInput)
        {
            Current = this;

            _bookHub = bookHub;
            _bookOperation = bookOperation;

            // timer for slideshow
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(_maxTimerTick);
            _timer.Tick += new EventHandler(DispatcherTimer_Tick);

            bookOperation.PageChanged +=
                (s, e) => ResetTimer();

            //
            mouseInput.MouseMoved +=
                (s, e) => { if (this.IsCancelSlideByMouseMove) ResetTimer(); };
        }


        /// <summary>
        /// スライドショーの表示間隔(秒)
        /// </summary>
        public double SlideShowInterval
        {
            get { return _SlideShowInterval; }
            set { if (_SlideShowInterval != value) { _SlideShowInterval = value; RaisePropertyChanged(); } }
        }

        private double _SlideShowInterval = 5.0;


        /// <summary>
        /// カーソルでスライドを止める.
        /// </summary>
        public bool IsCancelSlideByMouseMove
        {
            get { return _IsCancelSlideByMouseMove; }
            set { if (_IsCancelSlideByMouseMove != value) { _IsCancelSlideByMouseMove = value; RaisePropertyChanged(); } }
        }

        private bool _IsCancelSlideByMouseMove = true;


        /// <summary>
        /// ループ再生フラグ
        /// </summary>
        public bool IsSlideShowByLoop
        {
            get { return _IsSlideShowByLoop; }
            set { if (_IsSlideShowByLoop != value) { _IsSlideShowByLoop = value; RaisePropertyChanged(); } }
        }

        private bool _IsSlideShowByLoop = true;



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
        public void ToggleSlideShow()
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
            _bookOperation.NextSlide();
        }

        /// <summary>
        /// 再生開始
        /// </summary>
        private void Play()
        {
            // インターバル時間を修正する
            _timer.Interval = TimeSpan.FromSeconds(NVUtility.Clamp(this.SlideShowInterval * 0.5, _minTimerTick, _maxTimerTick));
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
            if ((DateTime.Now - _lastShowTime).TotalSeconds >= this.SlideShowInterval)
            {
                if (!_bookHub.IsLoading) NextSlide();
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


        // エンジン始動
        public void StartEngine()
        {
        }

        // エンジン停止
        public void StopEngine()
        {
            _timer.Stop();
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public double SlideShowInterval { get; set; }

            [DataMember]
            public bool IsCancelSlideByMouseMove { get; set; }

            [DataMember]
            public bool IsSlideShowByLoop { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.SlideShowInterval = this.SlideShowInterval;
            memento.IsCancelSlideByMouseMove = this.IsCancelSlideByMouseMove;
            memento.IsSlideShowByLoop = this.IsSlideShowByLoop;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.SlideShowInterval = memento.SlideShowInterval;
            this.IsCancelSlideByMouseMove = memento.IsCancelSlideByMouseMove;
            this.IsSlideShowByLoop = memento.IsSlideShowByLoop;
        }

        #endregion

    }
}
