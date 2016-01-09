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
    // 拡張マウスアクション
    public enum MouseExAction
    {
        None,
        XButton1Click,
        XButton2Click,
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


        // コンストラクタ
        public MouseExGesture(MouseExAction action, ModifierKeys modifierKeys)
        {
            this.MouseExAction = action;
            this.ModifierKeys = modifierKeys;
        }

        // 入力判定
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var mouseEventArgs = inputEventArgs as MouseEventArgs;
            if (mouseEventArgs == null) return false;

            MouseExAction action = MouseExAction.None;

            if (mouseEventArgs.XButton1 == MouseButtonState.Pressed)
            {
                action = MouseExAction.XButton1Click;
            }
            else if (mouseEventArgs.XButton2 == MouseButtonState.Pressed)
            {
                action = MouseExAction.XButton2Click;
            }

            return this.MouseExAction == action && ModifierKeys == Keyboard.Modifiers;
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

                throw new NotSupportedException($"'{source}' キーと修飾キーの組み合わせは、MouseExGesture ではサポートされていません。");
            }

            return new MouseExGesture(action, modifierKeys);
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

            text += "+" + gesture.MouseExAction;

            return text.TrimStart('+');
        }
    }
}
