// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{

    /// <summary>
    /// タッチ力状態
    /// </summary>
    public enum TouchInputState
    {
        None,
        Normal,
        Drag,
        Gesture,
    }


    // タッチ処理
    public class TouchInput : BindableBase
    {
        public static TouchInput Current { get; private set; }

        public TouchInput(TouchInputContext context)
        {
            Current = this;

            _context = context;
            _sender = context.Sender;

            this.Normal = new TouchInputNormal(_context);
            this.Normal.StateChanged += StateChanged;
            this.Normal.TouchGestureChanged += (s, e) => TouchGestureChanged?.Invoke(_sender, e);

            this.Drag = new TouchInputDrag(_context);
            this.Drag.StateChanged += StateChanged;

            this.Gesture = new TouchInputGesture(_context);
            this.Gesture.StateChanged += StateChanged;
            this.Gesture.GestureChanged += (s, e) => _context.GestureCommandCollection.Execute(e.Sequence);
            this.Gesture.GestureProgressed += (s, e) => _context.GestureCommandCollection.ShowProgressed(e.Sequence); ;

            // initialize state
            _touchInputCollection = new Dictionary<TouchInputState, TouchInputBase>();
            _touchInputCollection.Add(TouchInputState.Normal, this.Normal);
            _touchInputCollection.Add(TouchInputState.Drag, this.Drag);
            _touchInputCollection.Add(TouchInputState.Gesture, this.Gesture);
            SetState(TouchInputState.Normal, null);

            // initialize event
            _sender.PreviewTouchDown += OnTouchDown;
            _sender.PreviewTouchUp += OnTouchUp;
            _sender.PreviewTouchMove += OnTouchMove;

            //
            ClearTouchEventHandler();
        }

        //
        public event EventHandler<TouchGestureEventArgs> TouchGestureChanged;


        /// <summary>
        /// コマンド系イベントクリア
        /// </summary>
        public void ClearTouchEventHandler()
        {
            TouchGestureChanged = null;

#if DEBUG
            this.TouchGestureChanged +=
                (s, e) =>
                {
                    Debug.WriteLine($"TOUCH: {e.Gesture}");
                };
#endif
        }


        //
        private TouchInputContext _context;
        private FrameworkElement _sender;

        /// <summary>
        /// 状態：既定
        /// </summary>
        public TouchInputNormal Normal { get; private set; }


        /// <summary>
        /// 状態：ドラッグ
        /// </summary>
        public TouchInputDrag Drag { get; private set; }


        /// <summary>
        /// 状態：ジェスチャー
        /// </summary>
        public TouchInputGesture Gesture { get; private set; }

        /// <summary>
        /// 遷移テーブル
        /// </summary>
        private Dictionary<TouchInputState, TouchInputBase> _touchInputCollection;


        /// <summary>
        /// 現在状態
        /// </summary>
        private TouchInputState _state;
        public TouchInputState State => _state;

        /// <summary>
        /// 現在状態（実体）
        /// </summary>
        private TouchInputBase _current;


        /// <summary>
        /// 状態変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StateChanged(object sender, TouchInputStateEventArgs e)
        {
            SetState(e.State, e.Parameter);
        }

        /// <summary>
        /// 状態変更
        /// </summary>
        /// <param name="state"></param>
        /// <param name="parameter"></param>
        public void SetState(TouchInputState state, object parameter)
        {
            if (state == _state) return;

            _current?.OnClosed(_sender);
            _state = state;
            _current = _touchInputCollection[_state];
            _current.OnOpened(_sender, parameter);
        }

        //
        private void OnTouchDown(object sender, TouchEventArgs e)
        {
            if (sender != _sender) return;

            Debug.WriteLine($"TouchDown: {e.TouchDevice.Id}");

            // 非アクティブなデバイスを削除
            foreach (var item in _context.TouchMap.Where(item => !item.Value.TouchDevice.IsActive).Select(item => item.Value))
            {
                Debug.WriteLine($"NonActiveDevice: {item.TouchDevice.Id}");
            }
            _context.TouchMap = _context.TouchMap.Where(item => item.Value.TouchDevice.IsActive).ToDictionary(item => item.Key, item => item.Value);

            _context.TouchMap[e.TouchDevice] = new TouchContext()
            {
                TouchDevice = e.TouchDevice,
                StartPoint = e.GetTouchPoint(_sender),
                StartTimestamp = e.Timestamp
            };

            _sender.CaptureTouch(e.TouchDevice);
                        
            _current.OnTouchDown(_sender, e);
        }

        private void OnTouchUp(object sender, TouchEventArgs e)
        {
            if (sender != _sender) return;

            _context.TouchMap.Remove(e.TouchDevice);

            _sender.ReleaseTouchCapture(e.TouchDevice);

            _current.OnTouchUp(_sender, e);

            Debug.WriteLine($"TouchUp: {e.TouchDevice.Id}");

            ///e.Handled = true;
        }

        private void OnTouchMove(object sender, TouchEventArgs e)
        {
            if (sender != _sender) return;
            _current.OnTouchMove(_sender, e);
        }
    }
}
