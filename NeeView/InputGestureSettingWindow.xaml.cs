// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    public class ShortCutElement2
    {
        public string Gesture { get; set; }
        public List<CommandType> Overlaps { get; set; }
        public string OverlapsText { get; set; }
    }

    /// <summary>
    /// InputGestureSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class InputGestureSettingWindow : Window
    {
        // すべてのコマンドのショートカット
        public Dictionary<CommandType, string> Gestures { get; set; }

        // 編集するコマンド
        public CommandType CommandType { get; set; }

        // ショートカットテキストのリスト
        public ObservableCollection<ShortCutElement2> InputGestureCollection { get; set; } = new ObservableCollection<ShortCutElement2>();

        public string Header { get; set; }

        // コンストラクタ
        public InputGestureSettingWindow(Dictionary<CommandType, string> gestures, CommandType key)
        {
            InitializeComponent();

            Gestures = gestures;
            CommandType = key;
            Header = $"{CommandType.ToDispString()} - キーの設定";
          

            /*
            if (gestures[CommandType] != null)
            {
                foreach (var gesture in gestures[CommandType].Split(','))
                {
                    var element = CreateShortCutElement(gesture);
                    InputGestureCollection.Add(element);
                }
            }
            InitializeComponent();
            */

            UpdateGestures();


            this.DataContext = this;

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
        }

        //
        public void UpdateGestures()
        {
            InputGestureCollection.Clear();
            if (Gestures[CommandType] != null)
            {
                foreach (var gesture in Gestures[CommandType].Split(','))
                {
                    var element = CreateShortCutElement(gesture);
                    InputGestureCollection.Add(element);
                }
            }
        }

        //
        public ShortCutElement2 CreateShortCutElement(string gesture)
        {
            var element = new ShortCutElement2() { Gesture = gesture };

            var overlaps = Gestures
                .Where(e => !string.IsNullOrEmpty(e.Value) && e.Key != CommandType && e.Value.Split(',').Contains(gesture))
                .Select(e => e.Key)
                .ToList();

            if (overlaps.Count > 0)
            {
                element.Overlaps = overlaps;
                element.OverlapsText = string.Join("", overlaps.Select(e => $"「{e.ToDispString()}」")) + "と重複しています";
            }

            return element;
        }


        // キー入力処理
        // 押されているキーの状態からショートカットテキスト作成
        private void KeyGestureBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
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
            try
            {
                keyGesture = new KeyGesture(e.Key, Keyboard.Modifiers);
            }
            catch { }

            if (keyGesture != null)
            {
                var converter = new KeyGestureConverter();
                this.KeyGestureText.Text = ValidateKeyGestureText(converter.ConvertToString(keyGesture));
            }
            else
            {
                KeyExGesture keyExGesture = null;
                try
                {
                    keyExGesture = new KeyExGesture(e.Key, Keyboard.Modifiers);
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
            var key = this.KeyGestureText.Text;

            if (string.IsNullOrEmpty(key)) return;

            if (!InputGestureCollection.Any(item => item.Gesture == key))
            {
                var element = CreateShortCutElement(key);
                InputGestureCollection.Add(element);
            }

            this.KeyGestureText.Text = null;
        }

        // 削除ボタン処理
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            InputGestureCollection.Remove(this.InputGestureList.SelectedValue as ShortCutElement2);
        }


        // マウス入力処理
        // マウスの状態からショートカットテキスト作成
        private void MouseGestureBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // [?]TODO: チルトボタン .. WinProcの監視が必要なようなので、後回しです。

            bool isDefaultMouseAction = true;
            MouseAction action = MouseAction.None;
            MouseExAction exAction = MouseExAction.None;
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
                case MouseButton.XButton1:
                    exAction = MouseExAction.XButton1Click;
                    isDefaultMouseAction = false;
                    break;
                case MouseButton.XButton2:
                    exAction = MouseExAction.XButton2Click;
                    isDefaultMouseAction = false;
                    break;
            }

            if (isDefaultMouseAction)
            {
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
            else
            {
                MouseExGesture mouseGesture = null;
                try
                {
                    mouseGesture = new MouseExGesture(exAction, Keyboard.Modifiers);
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
            var key = this.MouseGestureText.Text;

            if (string.IsNullOrEmpty(key)) return;

            if (!InputGestureCollection.Any(item => item.Gesture == key))
            {
                var element = CreateShortCutElement(key);
                InputGestureCollection.Add(element);
            }

            this.MouseGestureText.Text = null;
        }

        // OKボタン処理
        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            /*
            string shortcut = null;
            foreach (var gesture in InputGestureCollection)
            {
                shortcut = shortcut == null ? gesture.Gesture : shortcut + "," + gesture.Gesture;
            }
            Gestures[CommandType] = shortcut;
            */

            Flush();

            this.DialogResult = true;
            this.Close();
        }

        //
        public void Flush()
        {
            string shortcut = null;
            foreach (var gesture in InputGestureCollection)
            {
                shortcut = shortcut == null ? gesture.Gesture : shortcut + "," + gesture.Gesture;
            }
            Gestures[CommandType] = shortcut;
        }

        // キャンセルボタン処理
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ConflictButton_Click(object sender, RoutedEventArgs e)
        {
            var item = this.InputGestureList.SelectedValue as ShortCutElement2;
            if (item != null)
            {
                Flush();

                var conflicts = new List<CommandType>(item.Overlaps);
                conflicts.Insert(0, CommandType);
                var context = new ConflictDialogContext(CommandType, item.Gesture, conflicts);
                var dialog = new ConflictDialog(context);
                dialog.Owner = this;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                var result = dialog.ShowDialog();
                if (result == true)
                {
                    foreach(var conflictItem in context.Commands)
                    {
                        if (!conflictItem.IsChecked)
                        {
                            var newGesture = string.Join(",", this.Gestures[conflictItem.CommandType].Split(',').Where(i => i != item.Gesture));
                            this.Gestures[conflictItem.CommandType] = string.IsNullOrEmpty(newGesture) ? null : newGesture;
                        }
                    }
                    UpdateGestures();
                  }
            }
        }
    }
}
