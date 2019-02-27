using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// 文字列をInputGestureに変換する
    /// </summary>
    public static class InputGestureConverter
    {
        //
        private enum ConverterType
        {
            Key, Mouse, MouseWheel
        }

        //
        private static readonly Dictionary<ConverterType, Func<string, InputGesture>> _converter = new Dictionary<ConverterType, Func<string, InputGesture>>
        {
            [ConverterType.Key] = ConvertFromKeyGestureString,
            [ConverterType.Mouse] = ConvertFromMouseGestureString,
            [ConverterType.MouseWheel] = ConvertFromMouseWheelGestureString,
        };

        //
        private static InputGesture ConvertFromStringByOrder(string source, ConverterType type)
        {
            List<ConverterType> order = null;

            switch (type)
            {
                case ConverterType.MouseWheel:
                    order = new List<ConverterType> { ConverterType.MouseWheel, ConverterType.Mouse, ConverterType.Key };
                    break;
                case ConverterType.Mouse:
                    order = new List<ConverterType> { ConverterType.Mouse, ConverterType.MouseWheel, ConverterType.Key };
                    break;
                default:
                    order = new List<ConverterType> { ConverterType.Key, ConverterType.Mouse, ConverterType.MouseWheel };
                    break;
            }

            foreach (var t in order)
            {
                var gesture = _converter[t](source);
                if (gesture != null) return gesture;
            }

            Debug.WriteLine($"'The combination of {source} key and modifier key is not supported.");
            return null;
        }


        /// <summary>
        /// 文字列をInputGestureに変換する
        /// </summary>
        /// <param name="source">ショートカット定義文字列</param>
        /// <returns>InputGesture。変換出来なかった場合はnull</returns>
        public static InputGesture ConvertFromString(string source)
        {
            // なるべく例外が発生しないようにコンバート順を考慮する
            if (source.Contains("Wheel"))
            {
                return ConvertFromStringByOrder(source, ConverterType.MouseWheel);
            }
            else if (source.Contains("Click"))
            {
                return ConvertFromStringByOrder(source, ConverterType.Mouse);
            }
            else
            {
                return ConvertFromStringByOrder(source, ConverterType.Key);
            }
        }

        /// <summary>
        /// 文字列をInputGestureに変換する。KeyExGestureのみ。
        /// </summary>
        /// <param name="source">ショートカット定義文字列</param>
        /// <returns>InputGesture。変換出来なかった場合はnull</returns>
        public static InputGesture ConvertFromKeyGestureString(string source)
        {
            try
            {
                KeyGestureExConverter converter = new KeyGestureExConverter();
                return (InputGesture)converter.ConvertFromString(source);
            }
            catch (Exception e)
            {
                Debug.WriteLine("(Ignore this exception): " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// 文字列をInputGestureに変換する。MouseGestureのみ。
        /// </summary>
        /// <param name="source">ショートカット定義文字列</param>
        /// <returns>InputGesture。変換出来なかった場合はnull</returns>
        public static InputGesture ConvertFromMouseGestureString(string source)
        {
            try
            {
                MouseGestureConverter converter = new MouseGestureConverter();
                return (MouseGesture)converter.ConvertFromString(source);
            }
            catch (Exception e)
            {
                Debug.WriteLine("(Ignore this exception): " + e.Message);
            }

            try
            {
                MouseGestureExConverter converter = new MouseGestureExConverter();
                return (InputGesture)converter.ConvertFromString(source);
            }
            catch (Exception e)
            {
                Debug.WriteLine("(Ignore this exception): " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// 文字列をInputGestureに変換する。MouseWheelGestureのみ。
        /// </summary>
        /// <param name="source">ショートカット定義文字列</param>
        /// <returns>InputGesture。変換出来なかった場合はnull</returns>
        public static InputGesture ConvertFromMouseWheelGestureString(string source)
        {
            try
            {
                MouseWheelGestureConverter converter = new MouseWheelGestureConverter();
                return (InputGesture)converter.ConvertFromString(source);
            }
            catch (Exception e)
            {
                Debug.WriteLine("(Ignore this exception): " + e.Message);
            }

            return null;
        }
    }
}
