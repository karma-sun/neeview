using NeeLaboratory.ComponentModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{

    /// <summary>
    /// MouseButton Bitmask
    /// </summary>
    [Flags]
    public enum MouseButtonBits
    {
        None = 0,
        LeftButton = (1 << MouseButton.Left),
        MiddleButton = (1 << MouseButton.Middle),
        RightButton = (1 << MouseButton.Right),
        XButton1 = (1 << MouseButton.XButton1),
        XButton2 = (1 << MouseButton.XButton2),
        All = LeftButton | MiddleButton | RightButton | XButton1 | XButton2,
    }

    public static class MouseButtonBitsExtensions
    {

        /// <summary>
        /// 押されているマウスボタンのビットマスク作成
        /// </summary>
        /// <param name="e">元になるデータ</param>
        /// <returns></returns>
        public static MouseButtonBits Create(MouseEventArgs e)
        {
            var bits = MouseButtonBits.None;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.LeftButton;
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.MiddleButton;
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.RightButton;
            }
            if (e.XButton1 == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.XButton1;
            }
            if (e.XButton2 == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.XButton2;
            }

            return bits;
        }

        /// <summary>
        /// 押されているマウスボタンのビットマスク作成
        /// </summary>
        /// <returns></returns>
        public static MouseButtonBits Create()
        {
            var bits = MouseButtonBits.None;

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.LeftButton;
            }
            if (Mouse.MiddleButton == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.MiddleButton;
            }
            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.RightButton;
            }
            if (Mouse.XButton1 == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.XButton1;
            }
            if (Mouse.XButton2 == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.XButton2;
            }

            return bits;
        }
    }

    /// <summary>
    /// マウス入力状態遷移イベントデータ
    /// </summary>
    public class MouseInputStateEventArgs : EventArgs
    {
        /// <summary>
        /// 遷移先状態
        /// </summary>
        public MouseInputState State { get; set; }

        /// <summary>
        /// 遷移パラメータ。
        /// 遷移状態により要求される内容は異なります。
        /// </summary>
        public object Parameter { get; set; }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="state"></param>
        public MouseInputStateEventArgs(MouseInputState state)
        {
            this.State = state;
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="state"></param>
        public MouseInputStateEventArgs(MouseInputState state, object parameter)
        {
            this.State = state;
            this.Parameter = parameter;
        }
    }

    /// <summary>
    /// マウス入力処理既定クラス
    /// </summary>
    public abstract class MouseInputBase : BindableBase
    {
        /// <summary>
        /// 状態遷移通知
        /// </summary>
        public EventHandler<MouseInputStateEventArgs> StateChanged;

        /// <summary>
        /// ボタン入力通知
        /// </summary>
        public EventHandler<MouseButtonEventArgs> MouseButtonChanged;

        /// <summary>
        /// ホイール入力通知
        /// </summary>
        public EventHandler<MouseWheelEventArgs> MouseWheelChanged;

        /// <summary>
        /// 水平ホイール入力通知
        /// </summary>
        public EventHandler<MouseWheelEventArgs> MouseHorizontalWheelChanged;

        /// <summary>
        /// 状態コンテキスト
        /// </summary>
        protected MouseInputContext _context;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public MouseInputBase(MouseInputContext context)
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
        /// 状態開始時処理。
        /// MouseCapture関連処理はUIスレッド切り替えが発生するため別処理にする。
        /// </summary>
        public virtual void OnCaptureOpened(FrameworkElement sender) { }

        /// <summary>
        /// 状態終了時処理。
        /// MouseCapture関連処理はUIスレッド切り替えが発生するため別処理にする。
        /// </summary>
        public virtual void OnCaptureClosed(FrameworkElement sender) { }

        /// <summary>
        /// 各種入力イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void OnMouseButtonDown(object sender, MouseButtonEventArgs e);
        public abstract void OnMouseButtonUp(object sender, MouseButtonEventArgs e);
        public abstract void OnMouseWheel(object sender, MouseWheelEventArgs e);
        public abstract void OnMouseHorizontalWheel(object sender, MouseWheelEventArgs e);
        public abstract void OnMouseMove(object sender, MouseEventArgs e);
        public virtual void OnKeyDown(object sender, KeyEventArgs e) { }

        /// <summary>
        /// 入力をキャンセル
        /// </summary>
        public virtual void Cancel() { }

        /// <summary>
        /// 専有判定
        /// </summary>
        /// <returns></returns>
        public virtual bool IsCaptured()
        {
            return true;
        }

        /// <summary>
        /// 状態遷移：既定状態に移動
        /// </summary>
        protected void ResetState()
        {
            StateChanged?.Invoke(this, new MouseInputStateEventArgs(MouseInputState.Normal));
        }

        /// <summary>
        /// 状態遷移：指定状態に移動
        /// </summary>
        /// <param name="state"></param>
        /// <param name="parameter"></param>
        protected void SetState(MouseInputState state, object parameter = null)
        {
            StateChanged?.Invoke(this, new MouseInputStateEventArgs(state, parameter));
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
}
