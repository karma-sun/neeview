using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    public class KeyExGesture : InputGesture
    {
        public Key Key { get; private set; }
        public ModifierKeys ModifierKeys { get; private set; }

        public KeyExGesture(Key key, ModifierKeys modifierKeys)
        {
            if (!IsDefinedKey(key)) throw new NotSupportedException();
            Key = key;
            ModifierKeys = modifierKeys;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var keyEventArgs = inputEventArgs as KeyEventArgs;
            if (keyEventArgs == null) return false;

            return this.Key == keyEventArgs.Key && this.ModifierKeys == Keyboard.Modifiers;
        }

        private bool IsDefinedKey(Key key)
        {
            return Key.None <= key && key <= Key.OemClear;
        }
    }


    public class KeyGestureExConverter
    {
        public KeyExGesture ConvertFromString(string source)
        {
            var keys = source.Split('+');

            var code = keys.Last();
            if (char.IsNumber(code, 0))
            {
                code = "D" + code;
            }

            Key action = Key.None;
            if (!Enum.TryParse(code, out action))
            {
                throw new NotSupportedException($"'{source}' キーと修飾キーの組み合わせは、KeyGestureEx ではサポートされていません。");
            }

            ModifierKeys modifierKeys = ModifierKeys.None;
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

                throw new NotSupportedException($"'{source}' キーと修飾キーの組み合わせは、KeyGestureEx ではサポートされていません。");
            }

            return new KeyExGesture(action, modifierKeys);
        }

        public string ConvertToString(InputGesture gesture)
        {
            var keyExGesture = gesture as KeyExGesture;
            if (keyExGesture == null) throw new NotSupportedException();

            string text = "";

            foreach (ModifierKeys key in Enum.GetValues(typeof(ModifierKeys)))
            {
                if ((keyExGesture.ModifierKeys & key) != ModifierKeys.None)
                {
                    text += "+" + ((key == ModifierKeys.Control) ? "Ctrl" : key.ToString());
                }
            }

            switch (keyExGesture.Key)
            {
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                    text += "+" + keyExGesture.Key.ToString().TrimStart('D');
                    break;
                default:
                    text += "+" + keyExGesture.Key.ToString();
                    break;
            }

            return text.TrimStart('+');
        }
    }

}
