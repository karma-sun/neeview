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
    /// 文字列をInputGestureに変換する
    /// </summary>
    public static class InputGestureConverter
    {
        private static KeyGestureConverter _keyGestureConverter = new KeyGestureConverter();
        private static KeyExGestureConverter _keyExGestureConverter = new KeyExGestureConverter();
        private static MouseGestureConverter _mouseGestureConverter = new MouseGestureConverter();
        private static MouseExGestureConverter _mouseExGestureConverter = new MouseExGestureConverter();
        private static MouseWheelGestureConverter _mouseWheelGestureConverter = new MouseWheelGestureConverter();
        private static MouseHorizontalWheelGestureConverter _mouseHorizontalWheelGestureConverter = new MouseHorizontalWheelGestureConverter();


        private enum ConverterType
        {
            Key,
            Mouse,
            MouseWheel,
            MouseHorizontalWheel
        }


        private static readonly Dictionary<ConverterType, Func<string, InputGesture>> _converter = new Dictionary<ConverterType, Func<string, InputGesture>>
        {
            [ConverterType.Key] = ConvertFromKeyGestureString,
            [ConverterType.Mouse] = ConvertFromMouseGestureString,
            [ConverterType.MouseWheel] = ConvertFromMouseWheelGestureString,
            [ConverterType.MouseHorizontalWheel] = ConvertFromMouseHorizontalWheelGestureString,
        };


        private static InputGesture ConvertFromStringByOrder(string source, ConverterType type)
        {
            List<ConverterType> order = null;

            switch (type)
            {
                case ConverterType.MouseWheel:
                case ConverterType.MouseHorizontalWheel:
                    order = new List<ConverterType> { ConverterType.MouseWheel, ConverterType.MouseHorizontalWheel, ConverterType.Mouse, ConverterType.Key };
                    break;
                case ConverterType.Mouse:
                    order = new List<ConverterType> { ConverterType.Mouse, ConverterType.MouseWheel, ConverterType.MouseHorizontalWheel, ConverterType.Key };
                    break;
                default:
                    order = new List<ConverterType> { ConverterType.Key, ConverterType.Mouse, ConverterType.MouseWheel, ConverterType.MouseHorizontalWheel };
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
                KeyExGestureConverter converter = new KeyExGestureConverter();
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
                MouseExGestureConverter converter = new MouseExGestureConverter();
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

        /// <summary>
        /// 文字列をInputGestureに変換する。MouseHorizontalWheelGestureのみ。
        /// </summary>
        /// <param name="source">ショートカット定義文字列</param>
        /// <returns>InputGesture。変換出来なかった場合はnull</returns>
        public static InputGesture ConvertFromMouseHorizontalWheelGestureString(string source)
        {
            try
            {
                MouseHorizontalWheelGestureConverter converter = new MouseHorizontalWheelGestureConverter();
                return (InputGesture)converter.ConvertFromString(source);
            }
            catch (Exception e)
            {
                Debug.WriteLine("(Ignore this exception): " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// InputGestureを文字列にする
        /// </summary>
        public static string ConvertToString(InputGesture gesture)
        {
            switch (gesture)
            {
                case KeyGesture e:
                    return _keyGestureConverter.ConvertToString(e);

                case KeyExGesture e:
                    return _keyExGestureConverter.ConvertToString(e);

                case MouseGesture e:
                    return _mouseGestureConverter.ConvertToString(e);

                case MouseExGesture e:
                    return _mouseExGestureConverter.ConvertToString(e);

                case MouseWheelGesture e:
                    return _mouseWheelGestureConverter.ConvertToString(e);

                case MouseHorizontalWheelGesture e:
                    return _mouseHorizontalWheelGestureConverter.ConvertToString(e);

                default:
                    throw new NotSupportedException($"Not supported gesture type: {gesture.GetType()}");
            }

        }
    }
}
