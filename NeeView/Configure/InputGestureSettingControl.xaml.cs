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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// InputGestureSettingControl.xaml の相互作用ロジック
    /// </summary>
    public partial class InputGestureSettingControl : UserControl
    {
        private InputGestureSettingViewModel _vm;

        //
        public InputGestureSettingControl()
        {
            InitializeComponent();
        }

        //
        public void Initialize(CommandTable.Memento memento, CommandType key)
        {
            _vm = new InputGestureSettingViewModel(memento, key);
            this.DataContext = _vm;
        }

        //
        public void Flush()
        {
            _vm?.Flush();
        }


        // キー入力処理
        // 押されているキーの状態からショートカットテキスト作成
        private void KeyGestureBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat) return;

            // TAB, ALT+にも対応
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            //Debug.WriteLine($"{Keyboard.Modifiers}+{e.Key}({key})");

            Key[] ignoreKeys = new Key[]
            {
                Key.System, Key.LeftShift, Key.LeftCtrl, Key.RightShift, Key.RightCtrl, Key.LWin, Key.RWin, Key.LeftAlt, Key.RightAlt,
                Key.ImeProcessed, Key.ImeNonConvert, Key.ImeModeChange, Key.ImeConvert, Key.ImeAccept,
                Key.Apps, Key.NumLock
            };

            if (ignoreKeys.Contains(key))
            {
                this.KeyGestureText.Text = null;
                return;
            }

            KeyGesture keyGesture = null;
            try
            {
                keyGesture = new KeyGesture(key, Keyboard.Modifiers);
            }
            catch { }

            if (keyGesture != null)
            {
                var converter = new KeyGestureConverter();
                this.KeyGestureText.Text = ValidateKeyGestureText(converter.ConvertToString(keyGesture));
                e.Handled = true;
            }
            else
            {
                KeyExGesture keyExGesture = null;
                try
                {
                    keyExGesture = new KeyExGesture(key, Keyboard.Modifiers);
                }
                catch { }

                if (keyExGesture != null)
                {
                    var converter = new KeyGestureExConverter();
                    this.KeyGestureText.Text = ValidateKeyGestureText(converter.ConvertToString(keyExGesture));
                }
                else
                {
                    this.KeyGestureText.Text = null;
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// キーコード表示補正
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private string ValidateKeyGestureText(string source)
        {
            var keys = source.Split('+');

            // Next ->PageDown
            // NextとPageDownはキーコードが同一であるため、表示文字列が意図しない物になることがある
            var tokens = keys.Select(e => e == "Next" ? "PageDown" : e);

            return string.Join("+", tokens);
        }


        // 追加ボタン処理
        private void AddKeyGestureButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.AddGesture(this.KeyGestureText.Text);
            this.KeyGestureText.Text = null;
        }

        // 削除ボタン処理
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var token = this.InputGestureList.SelectedValue as GestureToken;
            if (token != null)
            {
                _vm.RemoveGesture(token.Gesture);
            }
        }


        // マウス入力処理
        // マウスの状態からショートカットテキスト作成
        private void MouseGestureBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // [?]TODO: チルトボタン .. WinProcの監視が必要なようなので、後回しです。

            MouseExAction exAction = MouseExAction.None;
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    exAction = e.ClickCount >= 2 ? MouseExAction.LeftDoubleClick : MouseExAction.LeftClick;
                    break;
                case MouseButton.Right:
                    exAction = e.ClickCount >= 2 ? MouseExAction.RightDoubleClick : MouseExAction.RightClick;
                    break;
                case MouseButton.Middle:
                    exAction = e.ClickCount >= 2 ? MouseExAction.MiddleDoubleClick : MouseExAction.MiddleClick;
                    break;
                case MouseButton.XButton1:
                    exAction = e.ClickCount >= 2 ? MouseExAction.XButton1DoubleClick : MouseExAction.XButton1Click;
                    break;
                case MouseButton.XButton2:
                    exAction = e.ClickCount >= 2 ? MouseExAction.XButton2DoubleClick : MouseExAction.XButton2Click;
                    break;
            }

            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;
            if (e.LeftButton == MouseButtonState.Pressed && e.ChangedButton != MouseButton.Left)
                modifierMouseButtons |= ModifierMouseButtons.LeftButton;
            if (e.RightButton == MouseButtonState.Pressed && e.ChangedButton != MouseButton.Right)
                modifierMouseButtons |= ModifierMouseButtons.RightButton;
            if (e.MiddleButton == MouseButtonState.Pressed && e.ChangedButton != MouseButton.Middle)
                modifierMouseButtons |= ModifierMouseButtons.MiddleButton;
            if (e.XButton1 == MouseButtonState.Pressed && e.ChangedButton != MouseButton.XButton1)
                modifierMouseButtons |= ModifierMouseButtons.XButton1;
            if (e.XButton2 == MouseButtonState.Pressed && e.ChangedButton != MouseButton.XButton2)
                modifierMouseButtons |= ModifierMouseButtons.XButton2;

            // 拡張マウス入力で判定 (標準マウス入力も含まれるため）
            {
                MouseExGesture mouseGesture = null;
                try
                {
                    mouseGesture = new MouseExGesture(exAction, Keyboard.Modifiers, modifierMouseButtons);
                }
                catch { }

                if (mouseGesture != null)
                {
                    var converter = new MouseGestureExConverter();
                    this.MouseGestureText.Text = converter.ConvertToString(mouseGesture);
                }
                else
                {
                    this.MouseGestureText.Text = null;
                }
            }
        }


        // マウスホイール入力処理
        // マウスの状態からショートカットテキスト作成
        private void MouseGestureBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MouseWheelAction wheelAction = MouseWheelAction.None;
            if (e.Delta > 0)
            {
                wheelAction = MouseWheelAction.WheelUp;
            }
            else if (e.Delta < 0)
            {
                wheelAction = MouseWheelAction.WheelDown;
            }

            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;
            if (e.LeftButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.LeftButton;
            if (e.RightButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.RightButton;
            if (e.MiddleButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.MiddleButton;
            if (e.XButton1 == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.XButton1;
            if (e.XButton2 == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.XButton2;

            MouseWheelGesture mouseGesture = null;
            try
            {
                mouseGesture = new MouseWheelGesture(wheelAction, Keyboard.Modifiers, modifierMouseButtons);
            }
            catch { }

            if (mouseGesture != null)
            {
                var converter = new MouseWheelGestureConverter();
                this.MouseGestureText.Text = converter.ConvertToString(mouseGesture);
            }
            else
            {
                this.MouseGestureText.Text = null;
            }
        }

        // マウスショートカット追加ボタン処理
        private void AddMouseGestureButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.AddGesture(this.MouseGestureText.Text);
            this.MouseGestureText.Text = null;
        }

        /// <summary>
        /// 競合の解消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConflictButton_Click(object sender, RoutedEventArgs e)
        {
            var item = this.InputGestureList.SelectedValue as GestureToken;
            if (item != null)
            {
                _vm.ResolveConflict(item, Window.GetWindow(this));
            }
        }
    }
}
