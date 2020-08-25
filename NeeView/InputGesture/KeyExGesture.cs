using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    [TypeConverter(typeof(KeyExGestureConverter))]
    public class KeyExGesture : InputGesture
    {
        // メインキー
        public Key Key { get; private set; }

        // 修飾キー
        public ModifierKeys ModifierKeys { get; private set; }

        // 入力許可フラグ
        public static bool AllowSingleKey { get; set; }


        // コンストラクタ
        public KeyExGesture(Key key) : this(key, ModifierKeys.None)
        {
        }

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

            // 入力許可？ (Escキーは常に受け入れる)
            if (!AllowSingleKey && keyEventArgs.Key != Key.Escape) return false;

            // ALTが押されたときはシステムキーを通常キーとする
            Key key = keyEventArgs.Key;
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                key = keyEventArgs.Key == Key.System ? keyEventArgs.SystemKey : keyEventArgs.Key;
            }

            return this.Key == key && this.ModifierKeys == Keyboard.Modifiers;
        }

        // 
        private bool IsDefinedKey(Key key)
        {
            return Key.None <= key && key <= Key.OemClear;
        }
    }

}
