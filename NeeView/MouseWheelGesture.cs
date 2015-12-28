using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    public enum MouseWheelAction
    {
        None,
        WheelUp,
        WheelDown,
    }

    [Flags]
    public enum ModifierMouseButtons
    {
        None = 0,
        LeftButton = (1 << 0),
        MiddleButton = (1 << 1),
        RightButton = (1 << 2),
        XButton1 = (1 << 3),
        XButton2 = (1 << 4),
    }

    public class MouseWheelGesture : InputGesture
    {
        public MouseWheelAction MouseWheelAction { get; private set; }
        public ModifierKeys ModifierKeys { get; private set; }
        public ModifierMouseButtons ModifierMouseButtons { get; private set; }


        public MouseWheelGesture(MouseWheelAction wheelAction, ModifierKeys modifierKeys, ModifierMouseButtons modifierMouseButtons)
        {
            this.MouseWheelAction = wheelAction;
            this.ModifierKeys = modifierKeys;
            this.ModifierMouseButtons = modifierMouseButtons;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var mouseEventArgs = inputEventArgs as MouseWheelEventArgs;
            if (mouseEventArgs == null) return false;

            MouseWheelAction wheelAction = MouseWheelAction.None;
            if (mouseEventArgs.Delta > 0)
            {
                wheelAction = MouseWheelAction.WheelUp;
            }
            else if (mouseEventArgs.Delta < 0)
            {
                wheelAction = MouseWheelAction.WheelDown;
            }

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


    public class MouseWheelGestureConverter
    {
        public InputGesture ConvertFromString(string source)
        {
            var keys = source.Split('+');

            MouseWheelAction action = MouseWheelAction.None;
            ModifierKeys modifierKeys = ModifierKeys.None;
            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;

            if (!Enum.TryParse(keys.Last(), out action))
            {
                throw new NotSupportedException();
            }

            for (int i = 0; i < keys.Length - 1; ++i)
            {
                var key = keys[i];

                ModifierKeys modifierKeysOne;
                if (Enum.TryParse<ModifierKeys>(key, out modifierKeysOne))
                {
                    modifierKeys |= modifierKeysOne;
                    continue;
                }

                ModifierMouseButtons modifierMouseButtonsOne;
                if (Enum.TryParse<ModifierMouseButtons>(key, out modifierMouseButtonsOne))
                {
                    modifierMouseButtons |= modifierMouseButtonsOne;
                    continue;
                }

                throw new NotSupportedException();
            }

            return new MouseWheelGesture(action, modifierKeys, modifierMouseButtons);
        }

        public string ConvertToString(MouseWheelGesture gesture)
        {
            string text = "";

            foreach (ModifierKeys key in Enum.GetValues(typeof(ModifierKeys)))
            {
                if ((gesture.ModifierKeys & key) != ModifierKeys.None)
                {
                    text += "+" + key.ToString();
                }
            }

            foreach (ModifierMouseButtons button in Enum.GetValues(typeof(ModifierMouseButtons)))
            {
                if ((gesture.ModifierMouseButtons & button) != ModifierMouseButtons.None)
                {
                    text += "+" + button.ToString();
                }
            }

            text += "+" + gesture.MouseWheelAction;

            return text.TrimStart('+');
        }
    }

}
