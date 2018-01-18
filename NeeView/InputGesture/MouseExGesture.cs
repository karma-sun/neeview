// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    // 拡張マウスアクション
    public enum MouseExAction
    {
        None,
        LeftClick,
        RightClick,
        MiddleClick,
        WheelClick,
        LeftDoubleClick,
        RightDoubleClick,
        MiddleDoubleClick,
        XButton1Click,
        XButton1DoubleClick,
        XButton2Click,
        XButton2DoubleClick,
    }

    /// <summary>
    /// 拡張マウスアクション
    /// 拡張ボタン対応
    /// </summary>
    public class MouseExGesture : InputGesture
    {
        // メインアクション
        public MouseExAction MouseExAction { get; private set; }

        // 修飾キー
        public ModifierKeys ModifierKeys { get; private set; }

        // 修飾マウスボタン
        public ModifierMouseButtons ModifierMouseButtons { get; private set; }

        // コンストラクタ
        public MouseExGesture(MouseExAction action, ModifierKeys modifierKeys, ModifierMouseButtons modifierMouseButtons)
        {
            this.MouseExAction = action;
            this.ModifierKeys = modifierKeys;
            this.ModifierMouseButtons = modifierMouseButtons;
        }

        // 入力判定
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var mouseEventArgs = inputEventArgs as MouseButtonEventArgs;
            if (mouseEventArgs == null) return false;

            MouseExAction action = MouseExAction.None;

            switch (mouseEventArgs.ChangedButton)
            {
                case MouseButton.Left:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.LeftDoubleClick : MouseExAction.LeftClick;
                    break;
                case MouseButton.Right:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.RightDoubleClick : MouseExAction.RightClick;
                    break;
                case MouseButton.Middle:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.MiddleDoubleClick : MouseExAction.MiddleClick;
                    break;
                case MouseButton.XButton1:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.XButton1DoubleClick : MouseExAction.XButton1Click;
                    break;
                case MouseButton.XButton2:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.XButton2DoubleClick : MouseExAction.XButton2Click;
                    break;
            }

            if (action == MouseExAction.None) return false;

            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;
            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.Left)
                modifierMouseButtons |= ModifierMouseButtons.LeftButton;
            if (mouseEventArgs.RightButton == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.Right)
                modifierMouseButtons |= ModifierMouseButtons.RightButton;
            if (mouseEventArgs.MiddleButton == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.Middle)
                modifierMouseButtons |= ModifierMouseButtons.MiddleButton;
            if (mouseEventArgs.XButton1 == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.XButton1)
                modifierMouseButtons |= ModifierMouseButtons.XButton1;
            if (mouseEventArgs.XButton2 == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.XButton2)
                modifierMouseButtons |= ModifierMouseButtons.XButton2;

            return this.MouseExAction == action && this.ModifierMouseButtons == modifierMouseButtons && ModifierKeys == Keyboard.Modifiers;
        }
    }


    /// <summary>
    /// 拡張マウスアクション コンバータ
    /// </summary>
    public class MouseGestureExConverter
    {
        /// <summary>
        ///  文字列から拡張マウスアクションに変換する
        /// </summary>
        /// <param name="source">ジェスチャ文字列</param>
        /// <returns>MouseExGesture。変換に失敗したときは NotSupportedException 例外が発生</returns>
        public MouseExGesture ConvertFromString(string source)
        {
            var keys = source.Split('+');

            MouseExAction action = MouseExAction.None;
            ModifierKeys modifierKeys = ModifierKeys.None;
            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;

            if (!Enum.TryParse(keys.Last(), out action))
            {
                throw new NotSupportedException($"'{source}' キーと修飾キーの組み合わせは、MouseExGesture ではサポートされていません。");
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

                ModifierMouseButtons modifierMouseButtonsOne;
                if (Enum.TryParse<ModifierMouseButtons>(key, out modifierMouseButtonsOne))
                {
                    modifierMouseButtons |= modifierMouseButtonsOne;
                    continue;
                }

                throw new NotSupportedException($"'{source}' キーと修飾キーの組み合わせは、MouseExGesture ではサポートされていません。");
            }

            return new MouseExGesture(action, modifierKeys, modifierMouseButtons);
        }


        /// <summary>
        ///  拡張マウスアクションから文字列に変換する
        /// </summary>
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

            foreach (ModifierMouseButtons button in Enum.GetValues(typeof(ModifierMouseButtons)))
            {
                if ((gesture.ModifierMouseButtons & button) != ModifierMouseButtons.None)
                {
                    text += "+" + button.ToString();
                }
            }

            text += "+" + gesture.MouseExAction;

            return text.TrimStart('+');
        }
    }
}
