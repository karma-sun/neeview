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
            MouseGestureConverter mgc = new MouseGestureConverter();
            foreach (var item in control.Items.OfType<MenuItem>())
            {
                var command = item.Command as RoutedCommand;
                if (command != null)
                {
                    string text = "";
                    foreach (InputGesture gesture in command.InputGestures)
                    {
                        if (gesture is MouseGesture)
                        {
                            //マウス操作は表示しない
                            //text += ((text.Length > 0) ? ", " : "") + mgc.ConvertToString(gesture);
                        }
                        else
                        {
                            text += ((text.Length > 0) ? ", " : "") + kgc.ConvertToString(gesture);
                        }
                    }
                    item.InputGestureText = text;
                }

                UpdateInputGestureText(item);
            }
        }

    }
}
