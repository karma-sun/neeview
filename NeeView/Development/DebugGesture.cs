using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    public static class DebugGesture
    {
        /// <summary>
        /// Ctrl+F12をトリガーにしたデバッグコマンド実行
        /// </summary>
        [Conditional("DEBUG")]
        public static void Initialize()
        {
            App.Current.MainWindow.PreviewKeyDown += OnPreviewKeyDown;

            void OnPreviewKeyDown(object sender, KeyEventArgs e)
            {
                // trigger is Ctrl+F12
                if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F12)
                {
                    CheckFocus();
                    //CheckMouseCapture();

                    e.Handled = true;
                }
            };
        }

        // 現在のフォーカスを取得
        [Conditional("DEBUG")]
        public static void CheckFocus()
        {
            var element = FocusManager.GetFocusedElement(App.Current.MainWindow) as Visual;
            ElementWalk(element);
            Debug.WriteLine(".");

            void ElementWalk(Visual e)
            {
                if (e == null) return;

                var framewrkElement = e as FrameworkElement;
                var isKeyboardFocused = framewrkElement != null ? framewrkElement.IsKeyboardFocused : false;

                var name = (e as FrameworkElement)?.Name;


                var typename = e.GetType().ToString();
                var valuestring = e.ToString();


                if (typename == valuestring)
                {
                    Debug.WriteLine($"FocusTree: {isKeyboardFocused} {name} ({typename})");
                }
                else if (valuestring.StartsWith(typename))
                {
                    Debug.WriteLine($"FocusTree: {isKeyboardFocused} {name} ({valuestring})");
                }
                else
                {
                    Debug.WriteLine($"FocusTree: {isKeyboardFocused} {name} ({typename}: {valuestring})");
                }

                var parent = VisualTreeHelper.GetParent(e) as Visual;
                if (parent != null)
                {
                    ElementWalk(parent);
                }
            }
        }

        // 現在のマウスキャプチャを取得
        public static void CheckMouseCapture()
        {
            MouseInputHelper.DumpMouseCaptureState();
        }

    }

}


