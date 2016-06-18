// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeeView
{
    public enum MouseLongDownStatus
    {
        Off,
        On
    }

    public class MouseLongDown
    {
        public event EventHandler<MouseLongDownStatus> StatusChanged;
        public event EventHandler<MouseWheelEventArgs> MouseWheel;

        #region Property: Status
        private MouseLongDownStatus _Status;
        public MouseLongDownStatus Status
        {
            get { return _Status; }
            set
            {
                //if (_Status != value)
                {
                    _Status = value;
                    StatusChanged(this, _Status);
                }
            }
        }
        #endregion


        // 長押し判定用
        private DispatcherTimer _Timer = new DispatcherTimer();


        private FrameworkElement _Sender;
        private Point _StartPoint;
        private Point _EndPoint;

        // コンストラクタ
        public MouseLongDown(FrameworkElement sender)
        {
            _Sender = sender;

            _Sender.PreviewMouseDown += OnMouseButtonDown;
            _Sender.PreviewMouseUp += OnMouseButtonUp;
            _Sender.PreviewMouseMove += OnMouseMove;
            _Sender.PreviewMouseWheel += OnMouseWheel;

            this._Timer.Interval = TimeSpan.FromMilliseconds(750);
            this._Timer.Tick += this.OnTimeout;
        }


        // マウスボタンが押された時の処理
        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            _StartPoint = e.GetPosition(_Sender);
            _Timer.Start();

            //Debug.WriteLine("ON");
        }

        // マウスボタンが離された時の処理
        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            _Timer.Stop();
            Status = MouseLongDownStatus.Off;

            //Debug.WriteLine("OFF");
        }

        // マウスポインタが移動した時の処理
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            _EndPoint = e.GetPosition(_Sender);
            if (_Timer.IsEnabled && Math.Abs(_EndPoint.X - _StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(_EndPoint.Y - _StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _Timer.Stop();
            }
        }

        // マウスホイールの処理
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Status == MouseLongDownStatus.On)
            {
                MouseWheel?.Invoke(this, e);
            }
            else if (_Timer.IsEnabled)
            {
                _Timer.Stop();
            }
        }

        // マウスボタンが一定時間押され続けた時の処理
        private void OnTimeout(object sender, object e)
        {
            _Timer.Stop();
            Status = MouseLongDownStatus.On;
        }
    }
}
