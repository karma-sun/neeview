// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{

    /// <summary>
    /// タッチ状態
    /// </summary>
    public enum TouchInputState
    {
        None,
        Normal,
        MouseDrag,
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

            this.Drag = new TouchInputDrag(_context);
            this.Drag.StateChanged += StateChanged;

            this.Gesture = new TouchInputGesture(_context);
            this.Gesture.StateChanged += StateChanged;
            this.Gesture.GestureChanged += (s, e) => _context.GestureCommandCollection.Execute(e.Sequence);
            this.Gesture.GestureProgressed += (s, e) => _context.GestureCommandCollection.ShowProgressed(e.Sequence);

            this.MouseDrag = new TouchInputMouseDrag(_context);
            this.MouseDrag.StateChanged += StateChanged;

            this.Normal = new TouchInputNormal(_context, this.Gesture);
            this.Normal.StateChanged += StateChanged;
            this.Normal.TouchGestureChanged += (s, e) => TouchGestureChanged?.Invoke(_sender, e);


            // initialize state
            _touchInputCollection = new Dictionary<TouchInputState, TouchInputBase>();
            _touchInputCollection.Add(TouchInputState.Normal, this.Normal);
            _touchInputCollection.Add(TouchInputState.MouseDrag, this.MouseDrag);
            _touchInputCollection.Add(TouchInputState.Drag, this.Drag);
            _touchInputCollection.Add(TouchInputState.Gesture, this.Gesture);
            SetState(TouchInputState.Normal, null);

            // initialize event
            _sender.PreviewStylusDown += OnStylusDown;
            _sender.PreviewStylusUp += OnStylusUp;
            _sender.PreviewStylusMove += OnStylusMove;
            _sender.PreviewStylusSystemGesture += OnStylusSystemGesture;

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
        }


        //
        private TouchInputContext _context;
        private FrameworkElement _sender;


        /// <summary>
        /// 状態：既定
        /// </summary>
        public TouchInputNormal Normal { get; private set; }

        /// <summary>
        /// 状態：マウスドラッグ
        /// </summary>
        public TouchInputMouseDrag MouseDrag { get; private set; }

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
        /// IsEnabled property.
        /// </summary>
        private bool _isEnabled = true;
        [PropertyMember("タッチ操作を有効にする", Tips = "OFFのときは標準のマウス操作として機能します")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }


        //
        public bool IsCaptured()
        {
            return _context.TouchMap.Any();
        }

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

            ////Debug.WriteLine($"#TouchState: {state}");

            _current?.OnClosed(_sender);
            _state = state;
            _current = _touchInputCollection[_state];
            _current.OnOpened(_sender, parameter);
        }

        // 非アクティブなデバイスを削除
        private void CleanupTouchMap()
        {
            _context.TouchMap = _context.TouchMap.Where(item => !item.Key.InAir).ToDictionary(item => item.Key, item => item.Value);
        }

        //
        private void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            if (!_isEnabled) return;
            if (sender != _sender) return;

            ////Debug.WriteLine($"TouchDown: {e.StylusDevice.Id}");

            CleanupTouchMap();

            _context.TouchMap[e.StylusDevice] = new TouchContext()
            {
                StylusDevice = e.StylusDevice,
                StartPoint = e.GetPosition(_sender),
                StartTimestamp = e.Timestamp
            };

            _sender.CaptureStylus();

            _current.OnStylusDown(_sender, e);
        }

        //
        private void OnStylusUp(object sender, StylusEventArgs e)
        {
            if (!_isEnabled) return;
            if (sender != _sender) return;

            _context.TouchMap.Remove(e.StylusDevice);

            CleanupTouchMap();

            if (!_context.TouchMap.Any())
            {
                _sender.ReleaseStylusCapture();
            }

            _current.OnStylusUp(_sender, e);
        }

        //
        private void OnStylusMove(object sender, StylusEventArgs e)
        {
            if (!_isEnabled) return;
            if (sender != _sender) return;

            _current.OnStylusMove(_sender, e);
        }

        //
        private void OnStylusSystemGesture(object sender, StylusSystemGestureEventArgs e)
        {
            if (!_isEnabled) return;
            if (sender != _sender) return;

            ////Debug.WriteLine($"Gesture: {e.SystemGesture}");

            _current.OnStylusSystemGesture(_sender, e);
        }



        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsEnabled { get; set; }
            [DataMember]
            public TouchInputNormal.Memento Normal { get; set; }
            [DataMember]
            public TouchInputGesture.Memento Gesture { get; set; }
            [DataMember]
            public TouchInputDrag.Memento Drag { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.IsEnabled = true;
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsEnabled = this.IsEnabled;
            memento.Normal = this.Normal.CreateMemento();
            memento.Gesture = this.Gesture.CreateMemento();
            memento.Drag = this.Drag.CreateMemento();
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsEnabled = memento.IsEnabled;
            this.Normal.Restore(memento.Normal);
            this.Gesture.Restore(memento.Gesture);
            this.Drag.Restore(memento.Drag);
        }
        #endregion


    }
}
