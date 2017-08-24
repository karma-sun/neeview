// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
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
    /// <summary>
    /// 拡張キージェスチャ
    /// 単キー対応
    /// </summary>
    public class KeyExGesture : InputGesture
    {
        // メインキー
        public Key Key { get; private set; }

        // 修飾キー
        public ModifierKeys ModifierKeys { get; private set; }

        // 単キー入力許可フラグ
        public static bool AllowSingleKey { get; set; }

        // コンストラクタ
        public KeyExGesture(Key key, ModifierKeys modifierKeys)
        {
            if (!IsDefinedKey(key)) throw new NotSupportedException();
            Key = key;
            ModifierKeys = modifierKeys;
        }

        // 入力判定
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var keyEventArgs = inputEventArgs as KeyEventArgs;
            if (keyEventArgs == null) return false;

            // 単キー入力特殊条件では無効にする
            if (!AllowSingleKey && this.ModifierKeys == ModifierKeys.None) return false;

            return this.Key == keyEventArgs.Key && this.ModifierKeys == Keyboard.Modifiers;
        }

        // 
        private bool IsDefinedKey(Key key)
        {
            return Key.None <= key && key <= Key.OemClear;
        }
    }


    /// <summary>
    /// 拡張キージェスチャ コンバータ
    /// </summary>
    public class KeyGestureExConverter
    {
        /// <summary>
        ///  文字列から拡張キージェスチャに変換する
        /// </summary>
        /// <param name="source">キージェスチャ文字列</param>
        /// <returns>KeyExGesture。変換に失敗したときは NotSupportedException 例外が発生</returns>
        public KeyExGesture ConvertFromString(string source)
        {
            var keys = source.Split('+');

            var code = keys.Last();
            if (char.IsNumber(code, 0))
            {
                code = "D" + code;
            }

            Key action = Key.None;
            if (!Enum.TryParse(code, out action))
            {
                throw new NotSupportedException($"'{source}' キーと修飾キーの組み合わせは、KeyGestureEx ではサポートされていません。");
            }

            ModifierKeys modifierKeys = ModifierKeys.None;
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

                throw new NotSupportedException($"'{source}' キーと修飾キーの組み合わせは、KeyGestureEx ではサポートされていません。");
            }

            return new KeyExGesture(action, modifierKeys);
        }

        /// <summary>
        /// 拡張キージェスチャを文字列に変換
        /// </summary>
        /// <param name="gesture"></param>
        /// <returns></returns>
        public string ConvertToString(InputGesture gesture)
        {
            var keyExGesture = gesture as KeyExGesture;
            if (keyExGesture == null) throw new NotSupportedException();

            string text = "";

            foreach (ModifierKeys key in Enum.GetValues(typeof(ModifierKeys)))
            {
                if ((keyExGesture.ModifierKeys & key) != ModifierKeys.None)
                {
                    text += "+" + ((key == ModifierKeys.Control) ? "Ctrl" : key.ToString());
                }
            }

            switch (keyExGesture.Key)
            {
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                    text += "+" + keyExGesture.Key.ToString().TrimStart('D');
                    break;
                default:
                    text += "+" + keyExGesture.Key.ToString();
                    break;
            }

            return text.TrimStart('+');
        }
    }
}
