using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// ドラッグ操作
    /// </summary>
    public class MouseInputDrag : MouseInputBase
    {
        DragTransformControl _drag;

        public MouseInputDrag(MouseInputContext context) : base(context)
        {
            _drag = DragTransformControl.Current; // ##
        }


        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            sender.Cursor = Cursors.Hand;

            _drag.ResetState();
            _drag.UpdateState(CreateMouseButtonBits(), Keyboard.Modifiers, _context.StartPoint);
        }

        public override void OnClosed(FrameworkElement sender)
        {
        }

        public override void OnCaptureOpened(FrameworkElement sender)
        {
            MouseInputHelper.CaptureMouse(this, sender);
        }

        public override void OnCaptureClosed(FrameworkElement sender)
        {
            MouseInputHelper.ReleaseMouseCapture(this, sender);
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

        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            _drag.UpdateState(CreateMouseButtonBits(e), Keyboard.Modifiers, e.GetPosition(_context.Sender));
        }


        #region Obsolete

        [Obsolete, DataContract]
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

        [Obsolete]
        public Memento CreateMemento()
        {
            return null;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            var _drag = DragTransformControl.Current;
            _drag.IsOriginalScaleShowMessage = memento.IsOriginalScaleShowMessage;
            _drag.DragControlRotateCenter = memento.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
            _drag.DragControlScaleCenter = memento.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
            _drag.DragControlFlipCenter = memento.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
            _drag.IsKeepScale = memento.IsKeepScale;
            _drag.IsKeepAngle = memento.IsKeepAngle;
            _drag.IsKeepFlip = memento.IsKeepFlip;
            _drag.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;
        }

        #endregion
    }
}
