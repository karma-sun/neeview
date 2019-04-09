using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    public static partial class MenuExtensions
    {
        // メニューコントロールのジェスチャーテキスト更新
        public static void UpdateInputGestureText(this ItemsControl control)
        {
            if (control == null) return;

            KeyGestureConverter kgc = new KeyGestureConverter();
            KeyGestureExConverter kgxc = new KeyGestureExConverter();
            foreach (var item in control.Items.OfType<MenuItem>())
            {
                var command = item.Command as RoutedCommand;
                if (command != null)
                {
                    string text = "";
                    foreach (InputGesture gesture in command.InputGestures)
                    {
                        // キーショートカットのみ対応
                        if (gesture is KeyGesture)
                        {
                            text += ((text.Length > 0) ? ", " : "") + kgc.ConvertToString(gesture);
                        }
                        else if (gesture is KeyExGesture)
                        {
                            text += ((text.Length > 0) ? ", " : "") + kgxc.ConvertToString(gesture);
                        }
                    }
                    item.InputGestureText = text;
                }

                UpdateInputGestureText(item);
            }
        }
    }
}
