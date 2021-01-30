using NeeLaboratory.ComponentModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// タッチ入力処理既定クラス
    /// </summary>
    public abstract class TouchInputBase : BindableBase
    {
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
        /// 状態遷移通知
        /// </summary>
        public EventHandler<TouchInputStateEventArgs> StateChanged;

        /// <summary>
        /// タッチによるコマンド発動
        /// </summary>
        public EventHandler<TouchGestureEventArgs> TouchGestureChanged;

        
        /// <summary>
        /// 状態開始時処理
        /// </summary>
        public virtual void OnOpened(FrameworkElement sender, object parameter) { }

        /// <summary>
        /// 状態終了時処理
        /// </summary>
        public virtual void OnClosed(FrameworkElement sender) { }

        /// <summary>
        /// 各種入力イベント
        /// </summary>
        public abstract void OnStylusDown(object sender, StylusDownEventArgs e);
        public abstract void OnStylusUp(object sender, StylusEventArgs e);
        public abstract void OnStylusMove(object sender, StylusEventArgs e);
        public virtual void OnStylusSystemGesture(object sender, StylusSystemGestureEventArgs e) { }
        public virtual void OnMouseWheel(object sender, MouseWheelEventArgs e) { }
        public virtual void OnKeyDown(object sender, KeyEventArgs e) { }


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
        protected MouseButtonBits CreateMouseButtonBits()
        {
            return MouseButtonBitsExtensions.Create();
        }

        /// <summary>
        /// 押されているボタンを１つだけ返す
        /// </summary>
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


        /// <summary>
        /// タッチ座標からコマンド発行
        /// </summary>
        protected void ExecuteTouchGesture(object sender, StylusEventArgs e)
        {
            var point = e.GetPosition(_context.Sender);
            ExecuteTouchGesture(point);
        }

        protected void ExecuteTouchGesture(Point point)
        {
            var xRate = point.X / _context.Sender.ActualWidth;
            var yRate = point.Y / _context.Sender.ActualHeight;
            
            // TouchCenter を優先的に判定
            if (TouchGesture.TouchCenter.IsTouched(xRate, yRate))
            {
                var arg = new TouchGestureEventArgs(TouchGesture.TouchCenter);
                TouchGestureChanged?.Invoke(this, arg);
                if (arg.Handled) return;
            }

            // TouchLeft / Right
            var gesture = TouchGestureExtensions.GetTouchGestureLast(xRate, yRate);
            TouchGestureChanged?.Invoke(this, new TouchGestureEventArgs(gesture));
        }
    }
}
