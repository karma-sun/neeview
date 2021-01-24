using NeeLaboratory.ComponentModel;
using NeeView.Windows;
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
    /// マウス入力状態
    /// </summary>
    public enum MouseInputState
    {
        None,
        Normal,
        Loupe,
        Drag,
        Gesture,
    }

    /// <summary>
    /// MouseInputManager
    /// </summary>
    public class MouseInput : BindableBase
    {
        private FrameworkElement _sender;
        private MouseInputState _state;
        private MouseHorizontalWheelSource _mouseHorizontalWheelSource;

        /// <summary>
        /// 現在状態（実体）
        /// </summary>
        private MouseInputBase _current;

        /// <summary>
        /// 遷移テーブル
        /// </summary>
        private Dictionary<MouseInputState, MouseInputBase> _mouseInputCollection;

        /// <summary>
        /// 状態コンテキスト
        /// </summary>
        private MouseInputContext _context;

        /// <summary>
        /// マウス移動検知用
        /// </summary>
        private Point _lastActionPoint;


        /// <summary>
        /// コンストラクター
        /// </summary>
        public MouseInput(MouseInputContext context)
        {
            _context = context;
            _sender = _context.Sender;

            this.Normal = new MouseInputNormal(_context);
            this.Normal.StateChanged += StateChanged;
            this.Normal.MouseButtonChanged += (s, e) => MouseButtonChanged?.Invoke(_sender, e);
            this.Normal.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(_sender, e);
            this.Normal.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(_sender, e);

            if (context.LoupeTransform != null)
            {
                this.Loupe = new MouseInputLoupe(_context);
                this.Loupe.StateChanged += StateChanged;
                this.Loupe.MouseButtonChanged += (s, e) => MouseButtonChanged?.Invoke(_sender, e);
                this.Loupe.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(_sender, e);
                this.Loupe.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(_sender, e);
            }

            if (context.DragTransform != null)
            {
                this.Drag = new MouseInputDrag(_context);
                this.Drag.StateChanged += StateChanged;
                this.Drag.MouseButtonChanged += (s, e) => MouseButtonChanged?.Invoke(_sender, e);
                this.Drag.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(_sender, e);
                this.Drag.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(_sender, e);
            }

            this.Gesture = new MouseInputGesture(_context);
            this.Gesture.StateChanged += StateChanged;
            this.Gesture.MouseButtonChanged += (s, e) => MouseButtonChanged?.Invoke(_sender, e);
            this.Gesture.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(_sender, e);
            this.Gesture.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(_sender, e);
            this.Gesture.GestureChanged += (s, e) => _context.GestureCommandCollection.Execute(e.Sequence);
            this.Gesture.GestureProgressed += (s, e) => _context.GestureCommandCollection.ShowProgressed(e.Sequence);

            // initialize state
            _mouseInputCollection = new Dictionary<MouseInputState, MouseInputBase>();
            _mouseInputCollection.Add(MouseInputState.Normal, this.Normal);
            _mouseInputCollection.Add(MouseInputState.Loupe, this.Loupe);
            _mouseInputCollection.Add(MouseInputState.Drag, this.Drag);
            _mouseInputCollection.Add(MouseInputState.Gesture, this.Gesture);
            SetState(MouseInputState.Normal, null);

            // initialize event
            // NOTE: 時々操作が奪われしてまう原因の可能性その１
            _sender.MouseDown += OnMouseButtonDown;
            _sender.MouseUp += OnMouseButtonUp;
            _sender.MouseWheel += OnMouseWheel;
            _sender.MouseMove += OnMouseMove;
            _sender.PreviewKeyDown += OnKeyDown;

            // 水平ホイールイベント管理
            _sender.Loaded += (s, e) => InitializeMouseHorizontalWheel();
            _sender.Unloaded += (s, e) => ReleaseMouseHorizontalWheel();
            InitializeMouseHorizontalWheel();
        }


        /// <summary>
        /// ボタン入力イベント
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> MouseButtonChanged;

        /// <summary>
        /// ホイール入力イベント
        /// </summary>
        public event EventHandler<MouseWheelEventArgs> MouseWheelChanged;

        /// <summary>
        /// 水平ホイール入力イベント
        /// </summary>
        public event EventHandler<MouseWheelEventArgs> MouseHorizontalWheelChanged;

        /// <summary>
        /// 一定距離カーソルが移動したイベント
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseMoved;


        public FrameworkElement Sender => _sender;

        public MouseInputState State => _state;

        /// <summary>
        /// 状態：既定
        /// </summary>
        public MouseInputNormal Normal { get; private set; }

        /// <summary>
        /// 状態：ルーペ
        /// </summary>
        public MouseInputLoupe Loupe { get; private set; }

        /// <summary>
        /// 状態：ドラッグ
        /// </summary>
        public MouseInputDrag Drag { get; private set; }

        /// <summary>
        /// 状態：ジェスチャー
        /// </summary>
        public MouseInputGesture Gesture { get; private set; }

        public bool IsLoupeMode
        {
            get { return _state == MouseInputState.Loupe; }
            set { SetState(value ? MouseInputState.Loupe : MouseInputState.Normal, false); }
        }

        public bool IsNormalMode => _state == MouseInputState.Normal;


        /// <summary>
        /// コマンド系イベントクリア
        /// </summary>
        public void ClearMouseEventHandler()
        {
            MouseButtonChanged = null;
            MouseWheelChanged = null;
            MouseHorizontalWheelChanged = null;
        }

        /// <summary>
        /// 水平ホイール有効化
        /// </summary>
        private void InitializeMouseHorizontalWheel()
        {
            _mouseHorizontalWheelSource?.Dispose();

            var window = Window.GetWindow(_sender) as INotifyMouseHorizontalWheelChanged;
            if (window is null) return;

            ////Debug.WriteLine($"MouseInput.Sender: InitializeMouseHorizontalWheel()");
            _mouseHorizontalWheelSource = new MouseHorizontalWheelSource(_sender, window);
            _mouseHorizontalWheelSource.MouseHorizontalWheelChanged += (s, e) => OnMouseHorizontalWheel(s, e);
        }

        /// <summary>
        /// 水平ホイール無効化
        /// </summary>
        private void ReleaseMouseHorizontalWheel()
        {
            ////Debug.WriteLine($"MouseInput.Sender: ReleaseMouseHorizontalWheel()");
            _mouseHorizontalWheelSource?.Dispose();
            _mouseHorizontalWheelSource = null;
        }

        public bool IsCaptured()
        {
            return _current.IsCaptured();
        }

        /// <summary>
        /// 状態変更イベント処理
        /// </summary>
        private void StateChanged(object sender, MouseInputStateEventArgs e)
        {
            SetState(e.State, e.Parameter);
        }

        /// <summary>
        /// 状態変更
        /// </summary>
        public void SetState(MouseInputState state, object parameter, bool force = false)
        {
            if (!force && state == _state) return;
            //Debug.WriteLine($"#MouseState: {state}");

            var inputOld = _current;
            var inputNew = _mouseInputCollection[state];

            if (inputNew is null)
            {
                //Debug.WriteLine($"MouseInput: Not support state: {inputNew}");
                return;
            }

            inputOld?.OnClosed(_sender);
            _state = state;
            _current = inputNew;
            inputNew?.OnOpened(_sender, parameter);

            // NOTE: MouseCaptureの影響で同じUIスレッドで再入する可能性があるため、まとめて処理
            inputOld?.OnCaptureClosed(_sender);
            inputNew?.OnCaptureOpened(_sender);
        }

        /// <summary>
        /// 状態初期化
        /// </summary>
        public void ResetState()
        {
            SetState(MouseInputState.Normal, null, true);
        }

        private bool IsStylusDevice(MouseEventArgs e)
        {
            return e.StylusDevice != null && Config.Current.Touch.IsEnabled;
        }

        /// <summary>
        /// OnMouseButtonDown
        /// </summary>
        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != _sender) return;
            if (IsStylusDevice(e)) return;
            if (MainWindow.Current.IsMouseActivate) return;

            _current.OnMouseButtonDown(_sender, e);
        }

        /// <summary>
        /// OnMouseButtonUp
        /// </summary>
        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender != _sender) return;

            if (!IsStylusDevice(e))
            {
                _current.OnMouseButtonUp(_sender, e);
            }

            // 右クリックでのコンテキストメニュー無効
            e.Handled = true;
        }

        /// <summary>
        /// OnMouseWheel
        /// </summary>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender != _sender) return;
            if (IsStylusDevice(e)) return;

            _current.OnMouseWheel(_sender, e);
        }

        /// <summary>
        /// マウス水平ホイール
        /// </summary>
        private void OnMouseHorizontalWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender != _sender) return;

            ////Debug.WriteLine($"MouseInput: OnMouseHorizontalWheel: {e.Delta} ({e.Timestamp})");
            _current.OnMouseHorizontalWheel(_sender, e);
        }

        /// <summary>
        /// OnMouseMove
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender != _sender) return;
            if (IsStylusDevice(e)) return;

            _current.OnMouseMove(_sender, e);

            // マウス移動を通知
            var nowPoint = e.GetPosition(_sender);
            if (Math.Abs(nowPoint.X - _lastActionPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(nowPoint.Y - _lastActionPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                MouseMoved?.Invoke(this, e);
                _lastActionPoint = nowPoint;
            }
        }

        /// <summary>
        /// OnKeyDown
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (sender != _sender) return;
            _current.OnKeyDown(_sender, e);
        }

        /// <summary>
        /// Cancel input
        /// </summary>
        public void Cancel()
        {
            _current.Cancel();
        }


        // メッセージとして状態表示
        // TODO: 外部への依存が強すぎるので、定義場所を別にする？
        public void ShowMessage(TransformActionType ActionType, ViewContent mainContent)
        {
            var infoMessage = InfoMessage.Current; // TODO: not singleton
            if (Config.Current.Notice.ViewTransformShowMessageStyle == ShowMessageStyle.None) return;

            var dragTransform = _context.DragTransform;
            var loupeTransform = _context.LoupeTransform;

            switch (ActionType)
            {
                case TransformActionType.Scale:
                    var dpi = (Window.GetWindow(_sender) is IDpiScaleProvider dpiProvider) ? dpiProvider.GetDpiScale().ToFixedScale().DpiScaleX : 1.0;
                    string scaleText = Config.Current.Notice.IsOriginalScaleShowMessage && mainContent != null && mainContent.IsValid
                        ? $"{(int)(dragTransform.Scale * mainContent.Scale * dpi * 100 + 0.1)}%"
                        : $"{(int)(dragTransform.Scale * 100.0 + 0.1)}%";
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, scaleText);
                    break;
                case TransformActionType.Angle:
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, $"{(int)(dragTransform.Angle)}°");
                    break;
                case TransformActionType.FlipHorizontal:
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, Properties.Resources.Notice_FlipHorizontal + " " + (dragTransform.IsFlipHorizontal ? "ON" : "OFF"));
                    break;
                case TransformActionType.FlipVertical:
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, Properties.Resources.Notice_FlipVertical + " " + (dragTransform.IsFlipVertical ? "ON" : "OFF"));
                    break;
                case TransformActionType.LoupeScale:
                    if (loupeTransform.Scale != 1.0)
                    {
                        infoMessage.SetMessage(InfoMessageType.ViewTransform, $"×{loupeTransform.Scale:0.0}");
                    }
                    break;
            }
        }

        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public MouseInputNormal.Memento Normal { get; set; }
            [DataMember]
            public MouseInputLoupe.Memento Loupe { get; set; }
            [DataMember]
            public MouseInputGesture.Memento Gesture { get; set; }

            #region Obsolete
            [Obsolete, DataMember(EmitDefaultValue = false)] // ver 34.0
            public MouseInputDrag.Memento Drag { get; set; }
            #endregion


            public void RestoreConfig(Config config)
            {
                Normal.RestoreConfig(config);
                Loupe.RestoreConfig(config);
                Gesture.RestoreConfig(config);
            }
        }

        #endregion

    }
}
