// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
    // ホイールアクション
    public enum MouseWheelAction
    {
        None,
        WheelUp,
        WheelDown,
    }

    // 修飾マウスボタン
    [Flags]
    public enum ModifierMouseButtons
    {
        None = 0,
        LeftButton = (1 << 0),
        MiddleButton = (1 << 1),
        RightButton = (1 << 2),
        XButton1 = (1 << 3),
        XButton2 = (1 << 4),
    }

    /// <summary>
    /// マウスホイールアクション
    /// </summary>
    public class MouseWheelGesture : InputGesture
    {
        // マウスホイールアクション
        public MouseWheelAction MouseWheelAction { get; private set; }

        // 修飾キー
        public ModifierKeys ModifierKeys { get; private set; }

        // 修飾マウスボタン
        public ModifierMouseButtons ModifierMouseButtons { get; private set; }

        // コンストラクタ
        public MouseWheelGesture(MouseWheelAction wheelAction, ModifierKeys modifierKeys, ModifierMouseButtons modifierMouseButtons)
        {
            this.MouseWheelAction = wheelAction;
            this.ModifierKeys = modifierKeys;
            this.ModifierMouseButtons = modifierMouseButtons;
        }

        // 入力判定
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var mouseEventArgs = inputEventArgs as MouseWheelEventArgs;
            if (mouseEventArgs == null) return false;

            MouseWheelAction wheelAction = MouseWheelAction.None;
            if (mouseEventArgs.Delta > 0)
            {
                wheelAction = MouseWheelAction.WheelUp;
            }
            else if (mouseEventArgs.Delta < 0)
            {
                wheelAction = MouseWheelAction.WheelDown;
            }

            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;
            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.LeftButton;
            if (mouseEventArgs.RightButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.RightButton;
            if (mouseEventArgs.MiddleButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.MiddleButton;
            if (mouseEventArgs.XButton1 == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.XButton1;
            if (mouseEventArgs.XButton2 == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.XButton2;

            return this.MouseWheelAction == wheelAction && ModifierKeys == Keyboard.Modifiers && ModifierMouseButtons == modifierMouseButtons;
        }
    }

    /// <summary>
    /// マウスホイールアクション コンバータ
    /// </summary>
    public class MouseWheelGestureConverter
    {
        /// <summary>
        ///  文字列からマウスホイールアクションに変換する
        /// </summary>
        /// <param name="source">ジェスチャ文字列</param>
        /// <returns>MouseWheelGesture。変換に失敗したときは NotSupportedException 例外が発生</returns>
        public MouseWheelGesture ConvertFromString(string source)
        {
            var keys = source.Split('+');

            MouseWheelAction action = MouseWheelAction.None;
            ModifierKeys modifierKeys = ModifierKeys.None;
            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;

            if (!Enum.TryParse(keys.Last(), out action))
            {
                throw new NotSupportedException($"'{source}' キーと修飾キーの組み合わせは、MouseWheelGesture ではサポートされていません。");
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

                throw new NotSupportedException($"'{source}' キーと修飾キーの組み合わせは、MouseWheelGesture ではサポートされていません。");
            }

            return new MouseWheelGesture(action, modifierKeys, modifierMouseButtons);
        }


        /// <summary>
        ///  マウスホイールアクションから文字列に変換する
        /// </summary>
        public string ConvertToString(MouseWheelGesture gesture)
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

            text += "+" + gesture.MouseWheelAction;

            return text.TrimStart('+');
        }
    }
}
