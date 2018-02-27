using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

// TODO: 整備
// TODO: 関数が大きすぎる？細分化を検討

namespace NeeView
{
    /// <summary>
    /// ドラッグ操作
    /// </summary>
    public class MouseInputDrag : MouseInputBase
    {
        DragTransformControl _drag;

        //
        public MouseInputDrag(MouseInputContext context) : base(context)
        {
            _drag = DragTransformControl.Current; // ##
        }


        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            sender.CaptureMouse();
            sender.Cursor = Cursors.Hand;

            _drag.ResetState();
            _drag.UpdateState(CreateMouseButtonBits(), Keyboard.Modifiers, _context.StartPoint);
        }

        public override void OnClosed(FrameworkElement sender)
        {
            sender.ReleaseMouseCapture();
        }

        public override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            // nop.
        }

        public override void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            // ドラッグ解除
            if (CreateMouseButtonBits(e) == MouseButtonBits.None)
            {
                ResetState();
            }
        }

        //
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // コマンド実行
            MouseWheelChanged?.Invoke(sender, e);

            // ドラッグ解除
            if (e.Handled)
            {
                ResetState();
            }
        }

        //
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            _drag.UpdateState(CreateMouseButtonBits(e), Keyboard.Modifiers, e.GetPosition(_context.Sender));
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsOriginalScaleShowMessage { get; set; }
            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsControlCenterImage { get; set; }
            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsKeepScale { get; set; }
            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsKeepAngle { get; set; }
            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsKeepFlip { get; set; }
            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsViewStartPositionCenter { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            return null;
        }

#pragma warning disable CS0612

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            var _drag = DragTransformControl.Current;
            _drag.IsOriginalScaleShowMessage = memento.IsOriginalScaleShowMessage;
            _drag.IsControlCenterImage = memento.IsControlCenterImage;
            _drag.IsKeepScale = memento.IsKeepScale;
            _drag.IsKeepAngle = memento.IsKeepAngle;
            _drag.IsKeepFlip = memento.IsKeepFlip;
            _drag.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;
        }

#pragma warning restore CS0612

        #endregion

    }
}
