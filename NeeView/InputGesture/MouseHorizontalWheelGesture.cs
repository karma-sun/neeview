using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// マウス水平ホイールアクション
    /// </summary>
    public class MouseHorizontalWheelGesture : InputGesture
    {
        // マウスホイールアクション
        public MouseHorizontalWheelAction MouseWheelAction { get; private set; }

        // 修飾キー
        public ModifierKeys ModifierKeys { get; private set; }

        // 修飾マウスボタン
        public ModifierMouseButtons ModifierMouseButtons { get; private set; }

        // コンストラクタ
        public MouseHorizontalWheelGesture(MouseHorizontalWheelAction wheelAction, ModifierKeys modifierKeys, ModifierMouseButtons modifierMouseButtons)
        {
            this.MouseWheelAction = wheelAction;
            this.ModifierKeys = modifierKeys;
            this.ModifierMouseButtons = modifierMouseButtons;
        }

        // 入力判定
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var mouseEventArgs = inputEventArgs as MouseWheelEventArgs;
            if (mouseEventArgs == null) return false;

            MouseHorizontalWheelAction wheelAction = MouseHorizontalWheelAction.None;
            if (mouseEventArgs.Delta > 0)
            {
                wheelAction = MouseHorizontalWheelAction.WheelLeft;
            }
            else if (mouseEventArgs.Delta < 0)
            {
                wheelAction = MouseHorizontalWheelAction.WheelRight;
            }
            //System.Diagnostics.Debug.WriteLine($"HorizontalWheel: {mouseEventArgs.Delta}");

            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;
            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.LeftButton;
            if (mouseEventArgs.RightButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.RightButton;
            if (mouseEventArgs.MiddleButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.MiddleButton;
            if (mouseEventArgs.XButton1 == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.XButton1;
            if (mouseEventArgs.XButton2 == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.XButton2;

            return this.MouseWheelAction == wheelAction && ModifierKeys == Keyboard.Modifiers && ModifierMouseButtons == modifierMouseButtons;
        }
    }

}
