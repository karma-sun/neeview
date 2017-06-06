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
    public enum TouchGesture
    {
        None,
        TouchLeft,
        TouchRight,
        TouchTop1,
        TouchTop2,
        TouchTop3,
        TouchTop4,
        FlickLeft,
        FlickRight,
        FlickUp,
        FlickDown,
    }

    //
    public class TouchGestureEventArgs : EventArgs
    {
        public TouchEventArgs TouchEventArgs { get; set; }
        public TouchGesture Gesture { get; set; }

        public TouchGestureEventArgs()
        {
        }

        public TouchGestureEventArgs(TouchEventArgs e, TouchGesture gesture)
        {
            this.TouchEventArgs = e;
            this.Gesture = gesture;
        }
    }


    //
    public class TouchInputContext
    {
        /// <summary>
        /// コントロール初期化
        /// </summary>
        /// <param name="window"></param>
        /// <param name="sender"></param>
        /// <param name="targetView"></param>
        /// <param name="targetShadow"></param>
        public void Initialize(Window window, FrameworkElement sender, FrameworkElement targetView, FrameworkElement targetShadow)
        {
            this.Window = window;
            this.Sender = sender;
            this.TargetView = targetView;
            this.TargetShadow = targetShadow;
        }

        /// <summary>
        /// 所属ウィンドウ
        /// </summary>
        public Window Window { get; set; }

        /// <summary>
        /// イベント受取エレメント
        /// </summary>
        public FrameworkElement Sender { get; set; }

        /// <summary>
        /// 操作対象エレメント
        /// アニメーション対応
        /// </summary>
        public FrameworkElement TargetView { get; set; }

        /// <summary>
        /// 操作対象エレメント計算用
        /// アニメーション非対応。非表示の矩形のみ。
        /// 表示領域計算にはこちらを利用する
        /// </summary>
        public FrameworkElement TargetShadow { get; set; }

        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        public TouchPoint StartPoint { get; set; }
    }

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

            // initialize state
            _touchInputCollection = new Dictionary<TouchInputState, TouchInputBase>();
            _touchInputCollection.Add(TouchInputState.Normal, this.Normal);
            SetState(TouchInputState.Normal, null);

            // initialize event
            _sender.PreviewTouchDown += OnTouchDown;
            _sender.PreviewTouchUp += OnTouchUp;
            _sender.PreviewTouchMove += OnTouchMove;

#if DEBUG
            this.TouchGestureChanged +=
                (s, e) =>
                {
                    Debug.WriteLine($"TOUCH: {e.Gesture}");
                };
#endif
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
            _current.OnTouchDown(_sender, e);
        }

        private void OnTouchUp(object sender, TouchEventArgs e)
        {
            if (sender != _sender) return;
            _current.OnTouchUp(_sender, e);

            ///e.Handled = true;
        }

        private void OnTouchMove(object sender, TouchEventArgs e)
        {
            if (sender != _sender) return;
            _current.OnTouchMove(_sender, e);
        }

    }




    /// <summary>
    /// タッチ入力状態遷移イベントデータ
    /// </summary>
    public class TouchInputStateEventArgs : EventArgs
    {
        /// <summary>
        /// 遷移先状態
        /// </summary>
        public TouchInputState State { get; set; }

        /// <summary>
        /// 遷移パラメータ。
        /// 遷移状態により要求される内容は異なります。
        /// </summary>
        public object Parameter { get; set; }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="state"></param>
        public TouchInputStateEventArgs(TouchInputState state)
        {
            this.State = state;
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="state"></param>
        public TouchInputStateEventArgs(TouchInputState state, object parameter)
        {
            this.State = state;
            this.Parameter = parameter;
        }
    }


    /// <summary>
    /// タッチ入力処理既定クラス
    /// </summary>
    public abstract class TouchInputBase : BindableBase
    {
        /// <summary>
        /// 状態遷移通知
        /// </summary>
        public EventHandler<TouchInputStateEventArgs> StateChanged;

        /// <summary>
        /// タッチ入力通知
        /// </summary>
        public EventHandler<TouchGestureEventArgs> TouchGestureChanged;


        /// <summary>
        /// 状態コンテキスト
        /// </summary>
        protected TouchInputContext _context;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public TouchInputBase(TouchInputContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 状態開始時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public virtual void OnOpened(FrameworkElement sender, object parameter) { }

        /// <summary>
        /// 状態終了時処理
        /// </summary>
        /// <param name="sender"></param>
        public virtual void OnClosed(FrameworkElement sender) { }

        /// <summary>
        /// 各種入力イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void OnTouchDown(object sender, TouchEventArgs e);
        public abstract void OnTouchUp(object sender, TouchEventArgs e);
        public abstract void OnTouchMove(object sender, TouchEventArgs e);

        /// <summary>
        /// 状態遷移：既定状態に移動
        /// </summary>
        protected void ResetState()
        {
            StateChanged?.Invoke(this, new TouchInputStateEventArgs(TouchInputState.Normal));
        }

        /// <summary>
        /// 状態遷移：指定状態に移動
        /// </summary>
        /// <param name="state"></param>
        /// <param name="parameter"></param>
        protected void SetState(TouchInputState state, object parameter = null)
        {
            StateChanged?.Invoke(this, new TouchInputStateEventArgs(state, parameter));
        }

        /// <summary>
        /// 押されているマウスボタンのビットマスク作成
        /// </summary>
        /// <param name="e">元になるデータ</param>
        /// <returns></returns>
        protected MouseButtonBits CreateMouseButtonBits(MouseEventArgs e)
        {
            return MouseButtonBitsExtensions.Create(e);
        }

        /// <summary>
        /// 押されているマウスボタンのビットマスク作成
        /// </summary>
        /// <returns></returns>
        protected MouseButtonBits CreateMouseButtonBits()
        {
            return MouseButtonBitsExtensions.Create();
        }

        /// <summary>
        /// 押されているボタンを１つだけ返す
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected MouseButton? GetMouseButton(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                return MouseButton.Left;
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                return MouseButton.Middle;
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return MouseButton.Right;
            }
            if (e.XButton1 == MouseButtonState.Pressed)
            {
                return MouseButton.XButton1;
            }
            if (e.XButton2 == MouseButtonState.Pressed)
            {
                return MouseButton.XButton2;
            }

            return null;
        }
    }

    /// <summary>
    /// タッチ通常入力状態
    /// </summary>
    public class TouchInputNormal : TouchInputBase
    {
        /// <summary>
        /// ボタン押されている？
        /// </summary>
        private bool _isTouchDown;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public TouchInputNormal(TouchInputContext context) : base(context)
        {
        }

        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            _isTouchDown = false;
        }

        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
        }

        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchDown(object sender, TouchEventArgs e)
        {
            _isTouchDown = true;
            _context.Sender.Focus();

            _context.StartPoint = e.GetTouchPoint(_context.Sender);
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchUp(object sender, TouchEventArgs e)
        {
            if (!_isTouchDown) return;

            // タッチジェスチャー判定
            TouchGesture gesture = TouchGesture.None;
            var touchPoint = e.GetTouchPoint(_context.Sender);

            // タッチエリア 左右判定
            if (touchPoint.Position.X < _context.Sender.ActualWidth * 0.5)
            {
                gesture = TouchGesture.TouchLeft;
            }
            else
            {
                gesture = TouchGesture.TouchRight;
            }

            // コマンド決定
            if (gesture != TouchGesture.None)
            {
                TouchGestureChanged?.Invoke(sender, new TouchGestureEventArgs(e, gesture));
            }

            // その後の操作は全て無効
            _isTouchDown = false;
        }


        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchMove(object sender, TouchEventArgs e)
        {
            if (!_isTouchDown) return;

            var touchPoint = e.GetTouchPoint(_context.Sender);

            // nop.
        }



    }
}
