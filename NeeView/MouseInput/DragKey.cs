using System;
using System.Linq;
using System.Windows.Input;

namespace NeeView
{
    public class DragKey : IEquatable<DragKey>
    {
        public MouseButtonBits MouseButtonBits;
        public ModifierKeys ModifierKeys;


        public DragKey()
        {
        }

        public DragKey(MouseButtonBits bits, ModifierKeys modifiers)
        {
            MouseButtonBits = bits;
            ModifierKeys = modifiers;
        }

        public DragKey(string gesture)
        {
            if (string.IsNullOrWhiteSpace(gesture)) return;

            try
            {
                var key = DragKeyConverter.ConvertFromString(gesture);
                MouseButtonBits = key.MouseButtonBits;
                ModifierKeys = key.ModifierKeys;
            }
            catch (Exception)
            { }
        }

        #region IEquatable

        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            DragKey p = obj as DragKey;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (MouseButtonBits == p.MouseButtonBits) && (ModifierKeys == p.ModifierKeys);
        }

        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Equals(DragKey p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (MouseButtonBits == p.MouseButtonBits) && (ModifierKeys == p.ModifierKeys);
        }

        /// <summary>
        /// ハッシュ値
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return MouseButtonBits.GetHashCode() ^ ModifierKeys.GetHashCode();
        }

        /// <summary>
        /// 比較演算子
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(DragKey a, DragKey b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return (a.MouseButtonBits == b.MouseButtonBits) && (a.ModifierKeys == b.ModifierKeys);
        }

        /// <summary>
        /// 比較演算子
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(DragKey a, DragKey b)
        {
            return !(a == b);
        }

        #endregion

        public bool IsValid => MouseButtonBits != MouseButtonBits.None;

        public override string ToString()
        {
            return DragKeyConverter.ConvertToString(this);
        }
    }



    /// <summary>
    /// マウスドラッグ コンバータ
    /// </summary>
    public class DragKeyConverter
    {
        /// <summary>
        ///  文字列からマウスドラッグアクションに変換する
        /// </summary>
        /// <param name="source">ジェスチャ文字列</param>
        /// <returns>DragKey。変換に失敗したときは NotSupportedException 例外が発生</returns>
        public static DragKey ConvertFromString(string source)
        {
            // ex. LeftButton
            // ex. Ctrl+XButton1+LeftButton

            // １操作のみサポート
            source = source.Split(',').First();

            // ～Drag → ～Button
            source = source.Replace("Drag", "Button");

            var keys = source.Split('+');

            ModifierKeys modifierKeys = ModifierKeys.None;
            MouseButtonBits mouseButtonBits = MouseButtonBits.None;

            foreach (var key in keys)
            {
                switch (key)
                {
                    case "Ctrl":
                        modifierKeys |= ModifierKeys.Control;
                        continue;
                }

                if (Enum.TryParse<ModifierKeys>(key, out ModifierKeys modifierKeysOne))
                {
                    modifierKeys |= modifierKeysOne;
                    continue;
                }

                if (Enum.TryParse<MouseButtonBits>(key, out MouseButtonBits bit))
                {
                    mouseButtonBits |= bit;
                    continue;
                }

                throw new NotSupportedException(string.Format(Properties.Resources.NotSupportedKeyException_Message, source, "DragKey"));
            }

            //
            if (mouseButtonBits == MouseButtonBits.None)
            {
                throw new NotSupportedException(string.Format(Properties.Resources.NotSupportedKeyException_Message, source, "DragKey"));
            }

            return new DragKey(mouseButtonBits, modifierKeys);
        }

        /// <summary>
        ///  マウスドラッグアクションから文字列に変換する
        /// </summary>
        public static string ConvertToString(DragKey gesture)
        {
            if (!gesture.IsValid) return "";

            string text = "";

            foreach (ModifierKeys key in Enum.GetValues(typeof(ModifierKeys)))
            {
                if ((gesture.ModifierKeys & key) != ModifierKeys.None)
                {
                    text += "+" + ((key == ModifierKeys.Control) ? "Ctrl" : key.ToString());
                }
            }

            text += "+" + string.Join("+", gesture.MouseButtonBits.ToString().Split(',').Select(e => e.Trim()).Reverse());

            return text.TrimStart('+');
        }
    }

}
