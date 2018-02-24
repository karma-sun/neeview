using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    public static class ImeKeyUtility
    {
        /// <summary>
        /// 管理するIMEキー
        /// </summary>
        static List<Key> _imeKeys = new List<Key> { Key.ImeConvert, Key.ImeNonConvert };

        /// <summary>
        /// 入力定義にIMEキーが入っている？
        /// </summary>
        public static bool HasImeKey(this InputGesture gesture)
        {
            if (gesture is KeyGesture keyGesture)
            {
                return _imeKeys.Contains(keyGesture.Key);
            }
            else if (gesture is KeyExGesture keyExGesture)
            {
                return _imeKeys.Contains(keyExGesture.Key);
            }

            return false;
        }

        /// <summary>
        /// IMEキー？
        /// </summary>
        public static bool IsImeKey(this Key key)
        {
            return _imeKeys.Contains(key);
        }
    }
}
