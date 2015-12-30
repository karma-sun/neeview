using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    public enum MouseExAction
    {
        None,
        XButton1Click,
        XButton2Click,
    }

    public class MouseExGesture : InputGesture
    {
        public MouseExAction MouseExAction { get; private set; }
        public ModifierKeys ModifierKeys { get; private set; }


        public MouseExGesture(MouseExAction action, ModifierKeys modifierKeys)
        {
            this.MouseExAction = action;
            this.ModifierKeys = modifierKeys;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var mouseEventArgs = inputEventArgs as MouseEventArgs;
            if (mouseEventArgs == null) return false;

            MouseExAction action = MouseExAction.None;

            // 不完全だが、ひとまずこれで。
            if (mouseEventArgs.XButton1 == MouseButtonState.Pressed)
            {
                action = MouseExAction.XButton1Click;
            }
            else if (mouseEventArgs.XButton2 == MouseButtonState.Pressed)
            {
                action = MouseExAction.XButton2Click;
            }

            return this.MouseExAction == action && ModifierKeys == Keyboard.Modifiers;
        }
    }

    public class MouseExGestureConverter
    {
        public MouseExGesture ConvertFromString(string source)
        {
            var keys = source.Split('+');

            MouseExAction action = MouseExAction.None;
            ModifierKeys modifierKeys = ModifierKeys.None;

            if (!Enum.TryParse(keys.Last(), out action))
            {
                throw new NotSupportedException();
            }

            for (int i = 0; i < keys.Length - 1; ++i)
            {
                var key = keys[i];
                if (key == "Ctrl") key = "Control";

                ModifierKeys modifierKeysOne;
                if (Enum.TryParse<ModifierKeys>(key, out modifierKeysOne))
                {
                    modifierKeys |= modifierKeysOne;
                    continue;
                }

                throw new NotSupportedException();
            }

            return new MouseExGesture(action, modifierKeys);
        }


        public string ConvertToString(MouseExGesture gesture)
        {
            string text = "";

            foreach (ModifierKeys key in Enum.GetValues(typeof(ModifierKeys)))
            {
                if ((gesture.ModifierKeys & key) != ModifierKeys.None)
                {
                    text += "+" + ((key == ModifierKeys.Control) ? "Ctrl" : key.ToString());
                }
            }

            text += "+" + gesture.MouseExAction;

            return text.TrimStart('+');
        }
    }


}
