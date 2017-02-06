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
        private MouseLongDownStatus _status;
        public MouseLongDownStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                StatusChanged(this, _status);
            }
        }
        #endregion

        public bool IsLongDowned => Status == MouseLongDownStatus.On;


        // 長押し判定用
        private DispatcherTimer _timer = new DispatcherTimer();

        public TimeSpan Tick { get; set; }

        private FrameworkElement _sender;
        private Point _startPoint;
        private Point _endPoint;

        // コンストラクタ
        public MouseLongDown(FrameworkElement sender)
        {
            _sender = sender;

            _sender.PreviewMouseDown += OnMouseButtonDown;
            _sender.PreviewMouseUp += OnMouseButtonUp;
            _sender.PreviewMouseMove += OnMouseMove;
            _sender.PreviewMouseWheel += OnMouseWheel;

            _timer.Tick += this.OnTimeout;

            Tick = TimeSpan.FromSeconds(1.0);
        }


        // マウスボタンが押された時の処理
        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            _startPoint = e.GetPosition(_sender);

            _timer.Interval = Tick;
            _timer.Start();

            //Debug.WriteLine("ON");
        }

        // マウスボタンが離された時の処理
        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            _timer.Stop();
            Status = MouseLongDownStatus.Off;

            //Debug.WriteLine("OFF");
        }

        // マウスポインタが移動した時の処理
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            _endPoint = e.GetPosition(_sender);
            if (_timer.IsEnabled && Math.Abs(_endPoint.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(_endPoint.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _timer.Stop();
            }
        }

        // マウスホイールの処理
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Status == MouseLongDownStatus.On)
            {
                MouseWheel?.Invoke(this, e);
            }
            else if (_timer.IsEnabled)
            {
                _timer.Stop();
            }
        }

        // マウスボタンが一定時間押され続けた時の処理
        private void OnTimeout(object sender, object e)
        {
            _timer.Stop();
            Status = MouseLongDownStatus.On;
        }
    }
}
