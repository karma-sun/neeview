using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    public static class CommonExtensions
    {
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }

    public static partial class MenuExtensions
    {
        public static void UpdateInputGestureText(this ItemsControl control)
        {
            if (control == null) return;

            KeyGestureConverter kgc = new KeyGestureConverter();
            KeyExGestureConverter kxgc = new KeyExGestureConverter();
            //MouseGestureConverter mgc = new MouseGestureConverter();
            foreach (var item in control.Items.OfType<MenuItem>())
            {
                var command = item.Command as RoutedCommand;
                if (command != null)
                {
                    string text = "";
                    foreach (InputGesture gesture in command.InputGestures)
                    {
                        // キーショートカットのみ
                        if (gesture is KeyGesture)
                        {
                            text += ((text.Length > 0) ? ", " : "") + kgc.ConvertToString(gesture);
                        }
                        else if (gesture is KeyExGesture)
                        {
                            text += ((text.Length > 0) ? ", " : "") + kxgc.ConvertToString(gesture);
                        }
                    }
                    item.InputGestureText = text;
                }

                UpdateInputGestureText(item);
            }
        }

    }
}
