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
        Loupe,
    }


    // タッチ処理
    public class TouchInput : BindableBase
    {
        private TouchInputContext _context;
        private FrameworkElement _sender;
        private Dictionary<TouchInputState, TouchInputBase> _touchInputCollection;
        private TouchInputState _state;
        private TouchInputBase _current;


        public TouchInput(TouchInputContext context)
        {
            _context = context;
            _sender = _context.Sender;

            if (_context.DragTransform != null)
            {
                this.Drag = new TouchInputDrag(_context);
                this.Drag.StateChanged += StateChanged;
            }

            this.Gesture = new TouchInputGesture(_context);
            this.Gesture.StateChanged += StateChanged;
            this.Gesture.GestureChanged += (s, e) => _context.GestureCommandCollection.Execute(e.Sequence);
            this.Gesture.GestureProgressed += (s, e) => _context.GestureCommandCollection.ShowProgressed(e.Sequence);

            if (_context.DragTransform != null)
            {
                this.MouseDrag = new TouchInputMouseDrag(_context);
                this.MouseDrag.StateChanged += StateChanged;
            }

            this.Normal = new TouchInputNormal(_context, this.Gesture);
            this.Normal.StateChanged += StateChanged;
            this.Normal.TouchGestureChanged += (s, e) => TouchGestureChanged?.Invoke(_sender, e);

            this.Loupe = new TouchInputLoupe(_context);
            this.Loupe.StateChanged += StateChanged;

            this.Emulator = new TouchInputEmulator(_context);
            this.Emulator.TouchGestureChanged += (s, e) => TouchGestureChanged?.Invoke(_sender, e);

            // initialize state
            _touchInputCollection = new Dictionary<TouchInputState, TouchInputBase>();
            _touchInputCollection.Add(TouchInputState.Normal, this.Normal);
            _touchInputCollection.Add(TouchInputState.MouseDrag, this.MouseDrag);
            _touchInputCollection.Add(TouchInputState.Drag, this.Drag);
            _touchInputCollection.Add(TouchInputState.Gesture, this.Gesture);
            _touchInputCollection.Add(TouchInputState.Loupe, this.Loupe);
            SetState(TouchInputState.Normal, null);

            // initialize event
            _sender.StylusDown += OnStylusDown;
            _sender.StylusUp += OnStylusUp;
            _sender.StylusMove += OnStylusMove;
            _sender.StylusInAirMove += OnStylusInAirMove;
            _sender.StylusSystemGesture += OnStylusSystemGesture;
            _sender.MouseWheel += OnMouseWheel;
            _sender.PreviewKeyDown += OnKeyDown;

            ClearTouchEventHandler();

            // ルーペモード監視
            _context.LoupeTransform?.AddPropertyChanged(nameof(LoupeTransform.IsEnabled), LoupeTransform_IsEnabledChanged);
        }


        public event EventHandler<TouchGestureEventArgs> TouchGestureChanged;


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
        /// 状態：ルーペ
        /// </summary>
        public TouchInputLoupe Loupe { get; private set; }

        /// <summary>
        /// エミュレート
        /// </summary>
        public TouchInputEmulator Emulator { get; private set; }


        private void LoupeTransform_IsEnabledChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_state == TouchInputState.Loupe && !_context.LoupeTransform.IsEnabled)
            {
                SetState(TouchInputState.Normal, null);
            }
        }

        public bool IsCaptured()
        {
            return _context.TouchMap.Any();
        }

        /// <summary>
        /// コマンド系イベントクリア
        /// </summary>
        public void ClearTouchEventHandler()
        {
            TouchGestureChanged = null;
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

            var inputOld = _current;
            var inputNew = _touchInputCollection[state];

            if (inputNew is null)
            {
                ////Debug.WriteLine($"TouchInput: Not support state: {inputNew}");
                return;
            }

            _state = state;
            _current = inputNew;

            inputOld?.OnClosed(_sender);
            inputNew?.OnOpened(_sender, parameter);
        }

        // 非アクティブなデバイスを削除
        private void CleanupTouchMap()
        {
            _context.TouchMap = _context.TouchMap.Where(item => !item.Key.InAir).ToDictionary(item => item.Key, item => item.Value);
        }

        private void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            if (sender != _sender) return;
            if (!Config.Current.Touch.IsEnabled) return;
            if (MainWindow.Current.IsMouseActivate) return;

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

        private void OnStylusUp(object sender, StylusEventArgs e)
        {
            if (!Config.Current.Touch.IsEnabled) return;
            if (sender != _sender) return;

            _context.TouchMap.Remove(e.StylusDevice);

            CleanupTouchMap();

            if (!_context.TouchMap.Any())
            {
                _sender.ReleaseStylusCapture();
            }

            _current.OnStylusUp(_sender, e);
        }

        private void OnStylusMove(object sender, StylusEventArgs e)
        {
            if (!Config.Current.Touch.IsEnabled) return;
            if (sender != _sender) return;

            _current.OnStylusMove(_sender, e);
        }

        private void OnStylusInAirMove(object sender, StylusEventArgs e)
        {
            if (!Config.Current.Touch.IsEnabled) return;
            if (sender != _sender) return;

            _current.OnStylusInAirMove(_sender, e);
        }

        private void OnStylusSystemGesture(object sender, StylusSystemGestureEventArgs e)
        {
            if (!Config.Current.Touch.IsEnabled) return;
            if (sender != _sender) return;

            ////Debug.WriteLine($"Gesture: {e.SystemGesture}");

            _current.OnStylusSystemGesture(_sender, e);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender != _sender) return;
            _current.OnMouseWheel(_sender, e);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (sender != _sender) return;
            _current.OnKeyDown(_sender, e);
        }


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            public Memento()
            {
                Constructor();
            }

            [DataMember]
            public bool IsEnabled { get; set; }
            [DataMember]
            public TouchInputNormal.Memento Normal { get; set; }
            [DataMember]
            public TouchInputGesture.Memento Gesture { get; set; }
            [DataMember]
            public TouchInputDrag.Memento Drag { get; set; }


            private void Constructor()
            {
                this.IsEnabled = true;
            }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                Constructor();
            }

            public void RestoreConfig(Config config)
            {
                config.Touch.IsEnabled = IsEnabled;

                Normal.RestoreConfig(config);
                Gesture.RestoreConfig(config);
                Drag.RestoreConfig(config);
            }
        }

        #endregion
    }
}
