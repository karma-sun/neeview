using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeeView
{
    public enum LongButtonMask
    {
        [AliasName("@EnumLongButtonMaskLeft")]
        Left,
        [AliasName("@EnumLongButtonMaskRight")]
        Right,
        [AliasName("@EnumLongButtonMaskAll")]
        All,
    }

    public static class LingButtonMasExtensions
    {
        public static MouseButtonBits ToMouseButtonBits(this LongButtonMask self)
        {
            switch (self)
            {
                default:
                case LongButtonMask.Left:
                    return MouseButtonBits.LeftButton;
                case LongButtonMask.Right:
                    return MouseButtonBits.RightButton;
                case LongButtonMask.All:
                    return MouseButtonBits.All;
            }
        }
    }

    /// <summary>
    /// マウス通常入力状態
    /// </summary>
    public class MouseInputNormal : MouseInputBase
    {
        private LongButtonDownMode _longButtonDownMode = LongButtonDownMode.Loupe;
        [PropertyMember("@ParamMouseLongButtonDownMode")]
        public LongButtonDownMode LongButtonDownMode
        {
            get { return _longButtonDownMode; }
            set { _longButtonDownMode = value; RaisePropertyChanged(); }
        }

        [PropertyMember("@ParamMouseLongButtonMask")]
        public LongButtonMask LongButtonMask { get; set; }

        [PropertyRange("@ParamMouseLongButtonDownTime", 0.1, 2.0, TickFrequency = 0.1, Tips = "@ParamMouseLongButtonDownTimeTips")]
        public double LongButtonDownTime { get; set; } = 1.0;

        [PropertyRange("@ParamMouseLongButtonRepeatTime", 0.01, 1.0, TickFrequency = 0.01, Tips = "@ParamMouseLongButtonRepeatTimeTips")]
        public double LongButtonRepeatTime { get; set; } = 0.1;

        // マウスジェスチャー有効
        private bool _isGestureEnabled = true;
        [PropertyMember("@ParamMouseIsGestureEnabled")]
        public bool IsGestureEnabled
        {
            get { return _isGestureEnabled; }
            set { SetProperty(ref _isGestureEnabled, value); }
        }

        // マウスドラッグ有効
        private bool _isDragEnabled = true;
        [PropertyMember("@ParamMouseIsDragEnabled")]
        public bool IsDragEnabled
        {
            get { return _isDragEnabled; }
            set { SetProperty(ref _isDragEnabled, value); }
        }

        // ドラッグ開始距離
        [PropertyRange("@ParamMouseMinimumDragDistance", 1.0, 200.0, TickFrequency = 1.0, IsEditable = true, Tips = "@ParamMouseMinimumDragDistanceTips")]
        public double MinimumDragDistance { get; set; } = 5.0;

        /// <summary>
        /// ボタン押されている？
        /// </summary>
        private bool _isButtonDown;

        /// <summary>
        /// 長押し判定用タイマー
        /// </summary>
        private DispatcherTimer _timer = new DispatcherTimer();

        private DispatcherTimer _timerRepeat = new DispatcherTimer();
        private MouseButtonEventArgs _mouseButtonEventArgs;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public MouseInputNormal(MouseInputContext context) : base(context)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += OnTimeout;

            _timerRepeat.Interval = TimeSpan.FromMilliseconds(50);
            _timerRepeat.Tick += TimerRepeat_Tick;
        }

        /// <summary>
        /// マウスボタンが一定時間押され続けた時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimeout(object sender, object e)
        {
            _timer.Stop();

            if ((CreateMouseButtonBits() & LongButtonMask.ToMouseButtonBits()) == 0)
            {
                return;
            }

            switch (LongButtonDownMode)
            {
                case LongButtonDownMode.Loupe:
                    SetState(MouseInputState.Loupe, true);
                    break;

                case LongButtonDownMode.Repeat:
                    // 最初のコマンド発行
                    MouseButtonChanged?.Invoke(sender, _mouseButtonEventArgs);
                    // その後の操作は全て無効
                    _isButtonDown = false;

                    _timerRepeat.Interval = TimeSpan.FromSeconds(LongButtonRepeatTime);
                    _timerRepeat.Start();
                    break;
            }
        }

        //
        private void TimerRepeat_Tick(object sender, EventArgs e)
        {
            // リピートコマンド発行
            MouseButtonChanged?.Invoke(sender, _mouseButtonEventArgs);
        }


        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            _isButtonDown = false;
            if (sender.Cursor != Cursors.None)
            {
                sender.Cursor = null;
            }
        }

        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            _timer.Stop();
            _timerRepeat.Stop();
        }


        //
        public override bool IsCaptured()
        {
            return _isButtonDown;
        }



        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isButtonDown = true;
            _context.Sender.Focus();

            _context.StartPoint = e.GetPosition(_context.Sender);

            _timerRepeat.Stop();

            // ダブルクリック？
            if (e.ClickCount >= 2)
            {
                // コマンド決定
                MouseButtonChanged?.Invoke(sender, e);
                if (e.Handled)
                {
                    // その後の操作は全て無効
                    _isButtonDown = false;

                    _timer.Stop();
                    _timerRepeat.Stop();

                    return;
                }
            }

            if (e.StylusDevice == null)
            {
                // 長押し判定開始
                _timer.Interval = TimeSpan.FromSeconds(this.LongButtonDownTime);
                _timer.Start();

                // リピート用にパラメータ保存
                _mouseButtonEventArgs = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton);
            }
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            _timer.Stop();
            _timerRepeat.Stop();

            if (!_isButtonDown) return;

            // コマンド決定
            // 離されたボタンがメインキー、それ以外は装飾キー
            MouseButtonChanged?.Invoke(sender, e);

            // その後の操作は全て無効
            _isButtonDown = false;
        }

        /// <summary>
        /// マウスホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // コマンド決定
            // ホイールがメインキー、それ以外は装飾キー
            MouseWheelChanged?.Invoke(sender, e);

            // その後の操作は全て無効
            _isButtonDown = false;

            _timer.Stop();
            _timerRepeat.Stop();
        }


        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isButtonDown) return;

            var point = e.GetPosition(_context.Sender);

            var deltaX = Math.Abs(point.X - _context.StartPoint.X);
            var deltaY = Math.Abs(point.Y - _context.StartPoint.Y);

            // drag check
            if (deltaX > MinimumDragDistance || deltaY > MinimumDragDistance)
            {
                // ドラッグ開始。処理をドラッグ系に移行
                var action = DragActionTable.Current.GetActionType(new DragKey(CreateMouseButtonBits(e), Keyboard.Modifiers));
                if (action == DragActionType.None)
                {
                }
                else if (this.IsGestureEnabled && action == DragActionType.Gesture)
                {
                    SetState(MouseInputState.Gesture);
                }
                else if (this.IsDragEnabled)
                {
                    SetState(MouseInputState.Drag, e);
                }
            }
        }


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember(Name = "LongLeftButtonDownMode")]
            public LongButtonDownMode LongButtonDownMode { get; set; }

            [DataMember]
            public LongButtonMask LongButtonMask { get; set; }

            [DataMember(Name = "LongLeftButtonDownTime"), DefaultValue(1.0)]
            public double LongButtonDownTime { get; set; }

            [DataMember, DefaultValue(0.1)]
            public double LongButtonRepeatTime { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsGestureEnabled { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsDragEnabled { get; set; }

            [DataMember, DefaultValue(5.0)]
            public double MinimumDragDistance { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.LongButtonDownMode = this.LongButtonDownMode;
            memento.LongButtonMask = this.LongButtonMask;
            memento.LongButtonDownTime = this.LongButtonDownTime;
            memento.LongButtonRepeatTime = this.LongButtonRepeatTime;
            memento.IsGestureEnabled = this.IsGestureEnabled;
            memento.IsDragEnabled = this.IsDragEnabled;
            memento.MinimumDragDistance = this.MinimumDragDistance;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.LongButtonDownMode = memento.LongButtonDownMode;
            this.LongButtonMask = memento.LongButtonMask;
            this.LongButtonDownTime = memento.LongButtonDownTime;
            this.LongButtonRepeatTime = memento.LongButtonRepeatTime;
            this.IsGestureEnabled = memento.IsGestureEnabled;
            this.IsDragEnabled = memento.IsDragEnabled;
            this.MinimumDragDistance = memento.MinimumDragDistance;
        }
        #endregion


    }
}
