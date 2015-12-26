using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Collections.ObjectModel;

namespace NeeView
{
    /// <summary>
    /// InputGestureSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class InputGestureSettingWindow : Window
    {
        public InputGestureSettingWindow(SettingWindow.BookCommand command)
        {
            Command = command;

            if (command.ShortCut != null)
            {
                foreach (var gesture in command.ShortCut.Split(','))
                {
                    InputGestureCollection.Add(gesture);
                }
            }

            InitializeComponent();
            this.DataContext = this;
        }

        public SettingWindow.BookCommand Command { get; set; }
        public ObservableCollection<string> InputGestureCollection { get; set; } = new ObservableCollection<string>();

        private void KeyGestureBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // TODO: 単キーサポート

            if (e.IsRepeat) return;
            if (e.Key == Key.Tab) return;

            Key[] ignoreKeys = new Key[]
            {
                Key.System, Key.LeftShift, Key.LeftCtrl, Key.RightShift, Key.RightCtrl, Key.LWin, Key.RWin, Key.Escape,
                Key.ImeProcessed, Key.ImeNonConvert, Key.ImeModeChange, Key.ImeConvert, Key.ImeAccept,
                Key.Apps, Key.Tab, Key.NumLock
            };

            if (ignoreKeys.Contains(e.Key))
            {
                this.KeyGestureText.Text = null;
                return;
            }

            KeyGesture keyGesture = null;
            try {
                keyGesture = new KeyGesture(e.Key, Keyboard.Modifiers);
            }
            catch { }

            if (keyGesture != null)
            {
                var converter = new KeyGestureConverter();
                this.KeyGestureText.Text = converter.ConvertToString(keyGesture);
            }
            else
            {
                this.KeyGestureText.Text = null;
            }
        }

        private void AddKeyGestureButton_Click(object sender, RoutedEventArgs e)
        {
            var key = this.KeyGestureText.Text;

            if (string.IsNullOrEmpty(key)) return;

            if (!InputGestureCollection.Contains(key))
            {
                InputGestureCollection.Add(key);
            }

            this.KeyGestureText.Text = null;
         }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            InputGestureCollection.Remove(this.InputGestureList.SelectedValue as string);
        }

        private void MouseGestureBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: ホイール
            // TODO: 拡張ボタン
            // TODO: チルドボタン

            MouseAction action = MouseAction.None;
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    action = e.ClickCount >= 2 ? MouseAction.LeftDoubleClick : MouseAction.LeftClick;
                    break;
                case MouseButton.Right:
                    action = e.ClickCount >= 2 ? MouseAction.RightDoubleClick : MouseAction.RightClick;
                    break;
                case MouseButton.Middle:
                    action = e.ClickCount >= 2 ? MouseAction.MiddleDoubleClick : MouseAction.MiddleClick;
                    break;
            }

            MouseGesture mouseGesture = null;
            try
            {
                mouseGesture = new MouseGesture(action, Keyboard.Modifiers);
            }
            catch { }

            if (mouseGesture != null)
            {
                var converter = new MouseGestureConverter();
                this.MouseGestureText.Text = converter.ConvertToString(mouseGesture);
            }
            else
            {
                this.MouseGestureText.Text = null;
            }
        }

        private void AddMouseGestureButton_Click(object sender, RoutedEventArgs e)
        {
            var key = this.MouseGestureText.Text;

            if (string.IsNullOrEmpty(key)) return;

            if (!InputGestureCollection.Contains(key))
            {
                InputGestureCollection.Add(key);
            }

            this.MouseGestureText.Text = null;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            string shortcut = null;
            foreach(var gesture in InputGestureCollection)
            {
                shortcut = shortcut == null ? gesture : shortcut + "," + gesture;
            }
            Command.ShortCut = shortcut;

            this.DialogResult = true;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
