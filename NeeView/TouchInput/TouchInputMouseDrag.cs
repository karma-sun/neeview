using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// マウスドラッグ互換
    /// </summary>
    public class TouchInputMouseDrag : TouchInputBase
    {
        DragTransformControl _drag;

        //
        TouchContext _touch;


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public TouchInputMouseDrag(TouchInputContext context) : base(context)
        {
            _drag = context.DragTransformControl; // ##
        }


        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            _touch = (TouchContext)parameter;

            _drag.ResetState();
            _drag.UpdateState(MouseButtonBits.LeftButton, Keyboard.Modifiers, _touch.StartPoint);
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
        public override void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            // マルチタッチでドラッグへ
            if (_context.TouchMap.Count >= 2)
            {
                SetState(TouchInputState.Drag, null);
            }
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusUp(object sender, StylusEventArgs e)
        {
            SetState(TouchInputState.Normal, null);
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusMove(object sender, StylusEventArgs e)
        {
            if (e.StylusDevice != _touch?.StylusDevice) return;

            _drag.UpdateState(MouseButtonBits.LeftButton, Keyboard.Modifiers, e.GetPosition(_context.Sender));
        }
    }

}
