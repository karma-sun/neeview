using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace NeeView
{
    public class KeyExConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string) && context?.Instance is Key key)
            {
                return IsDefinedKey(key);
            }
            else
            {
                return false;
            }
        }


        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
        {
            if (source is string)
            {
                string fullName = ((string)source).Trim();
                object key = GetKey(fullName, CultureInfo.InvariantCulture);
                if (key != null)
                {
                    return ((Key)key);
                }
                else
                {
                    throw new NotSupportedException($"Unsupported key literal: {fullName}");
                }
            }
            throw GetConvertFromException(source);
        }


        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }

            if (destinationType == typeof(string) && value != null)
            {
                Key key = (Key)value;
                if (key == Key.None)
                {
                    return string.Empty;
                }

                if (key >= Key.D0 && key <= Key.D9)
                {
                    return char.ToString((char)(int)(key - Key.D0 + '0'));
                }

                if (key >= Key.A && key <= Key.Z)
                {
                    return char.ToString((char)(int)(key - Key.A + 'A'));
                }

                string strKey = MatchKey(key, culture);
                if (strKey != null && (strKey.Length != 0 || strKey == string.Empty))
                {
                    return strKey;
                }
            }
            throw GetConvertToException(value, destinationType);
        }


        private object GetKey(string keyToken, CultureInfo culture)
        {
            if (keyToken == string.Empty)
            {
                return Key.None;
            }
            else
            {
                keyToken = keyToken.ToUpper(culture);
                if (keyToken.Length == 1 && char.IsLetterOrDigit(keyToken[0]))
                {
                    if (char.IsDigit(keyToken[0]) && (keyToken[0] >= '0' && keyToken[0] <= '9'))
                    {
                        return ((int)(Key)(Key.D0 + keyToken[0] - '0'));
                    }
                    else if (char.IsLetter(keyToken[0]) && (keyToken[0] >= 'A' && keyToken[0] <= 'Z'))
                    {
                        return ((int)(Key)(Key.A + keyToken[0] - 'A'));
                    }
                    else
                    {
                        throw new ArgumentException( $"Cannot convert string {keyToken} to type {typeof(Key)}");
                    }
                }
                else
                {
                    Key keyFound = (Key)(-1);
                    switch (keyToken)
                    {
                        case "ENTER": keyFound = Key.Return; break;
                        case "ESC": keyFound = Key.Escape; break;
                        case "PGUP": keyFound = Key.PageUp; break;
                        case "PGDN": keyFound = Key.PageDown; break;
                        case "PRTSC": keyFound = Key.PrintScreen; break;
                        case "INS": keyFound = Key.Insert; break;
                        case "DEL": keyFound = Key.Delete; break;
                        case "WINDOWS": keyFound = Key.LWin; break;
                        case "WIN": keyFound = Key.LWin; break;
                        case "LEFTWINDOWS": keyFound = Key.LWin; break;
                        case "RIGHTWINDOWS": keyFound = Key.RWin; break;
                        case "APPS": keyFound = Key.Apps; break;
                        case "APPLICATION": keyFound = Key.Apps; break;
                        case "BREAK": keyFound = Key.Cancel; break;
                        case "BACKSPACE": keyFound = Key.Back; break;
                        case "BKSP": keyFound = Key.Back; break;
                        case "BS": keyFound = Key.Back; break;
                        case "SHIFT": keyFound = Key.LeftShift; break;
                        case "LEFTSHIFT": keyFound = Key.LeftShift; break;
                        case "RIGHTSHIFT": keyFound = Key.RightShift; break;
                        case "CONTROL": keyFound = Key.LeftCtrl; break;
                        case "CTRL": keyFound = Key.LeftCtrl; break;
                        case "LEFTCTRL": keyFound = Key.LeftCtrl; break;
                        case "RIGHTCTRL": keyFound = Key.RightCtrl; break;
                        case "ALT": keyFound = Key.LeftAlt; break;
                        case "LEFTALT": keyFound = Key.LeftAlt; break;
                        case "RIGHTALT": keyFound = Key.RightAlt; break;
                        case "SEMICOLON": keyFound = Key.OemSemicolon; break;
                        case "PLUS": keyFound = Key.OemPlus; break;
                        case "COMMA": keyFound = Key.OemComma; break;
                        case "MINUS": keyFound = Key.OemMinus; break;
                        case "PERIOD": keyFound = Key.OemPeriod; break;
                        case "QUESTION": keyFound = Key.OemQuestion; break;
                        case "TILDE": keyFound = Key.OemTilde; break;
                        case "OPENBRACKETS": keyFound = Key.OemOpenBrackets; break;
                        case "PIPE": keyFound = Key.OemPipe; break;
                        case "CLOSEBRACKETS": keyFound = Key.OemCloseBrackets; break;
                        case "QUOTES": keyFound = Key.OemQuotes; break;
                        case "BACKSLASH": keyFound = Key.OemBackslash; break;
                        case "FINISH": keyFound = Key.OemFinish; break;
                        case "ATTN": keyFound = Key.Attn; break;
                        case "CRSEL": keyFound = Key.CrSel; break;
                        case "EXSEL": keyFound = Key.ExSel; break;
                        case "ERASEEOF": keyFound = Key.EraseEof; break;
                        case "PLAY": keyFound = Key.Play; break;
                        case "ZOOM": keyFound = Key.Zoom; break;
                        case "PA1": keyFound = Key.Pa1; break;
                        default: keyFound = (Key)Enum.Parse(typeof(Key), keyToken, true); break;
                    }

                    if ((int)keyFound != -1)
                    {
                        return keyFound;
                    }
                    return null;
                }
            }
        }


        private static string MatchKey(Key key, CultureInfo culture)
        {
            if (key == Key.None)
            {
                return string.Empty;
            }
            else
            {
                switch (key)
                {
                    case Key.Back: return "Backspace";
                    case Key.LineFeed: return "Clear";
                    case Key.Return: return "Enter";
                    case Key.Capital: return "CapsLock";
                    case Key.KanaMode: return "HangulMode";
                    case Key.HanjaMode: return "KanjiMode";
                    case Key.Escape: return "Esc";
                    case Key.Prior: return "PageUp";
                    case Key.Next: return "PageDown";
                    case Key.Snapshot: return "PrintScreen";
                    case Key.Oem1: return "OemSemicolon";
                    case Key.Oem2: return "OemQuestion";
                    case Key.Oem3: return "OemTilde";
                    case Key.Oem4: return "OemOpenBrackets";
                    case Key.Oem5: return "OemPipe";
                    case Key.Oem6: return "OemCloseBrackets";
                    case Key.Oem7: return "OemQuotes";
                    case Key.Oem102: return "OemBackslash";
                    case Key.OemAttn: return "DbeAlphanumeric";
                    case Key.OemFinish: return "DbeKatakana";
                    case Key.OemCopy: return "DbeHiragana";
                    case Key.OemAuto: return "DbeSbcsChar";
                    case Key.OemEnlw: return "DbeDbcsChar";
                    case Key.OemBackTab: return "DbeRoman";
                    case Key.Attn: return "DbeNoRoman";
                    case Key.CrSel: return "DbeEnterWordRegisterMode";
                    case Key.ExSel: return "DbeEnterImeConfigureMode";
                    case Key.EraseEof: return "DbeFlushString";
                    case Key.Play: return "DbeCodeInput";
                    case Key.Zoom: return "DbeNoCodeInput";
                    case Key.NoName: return "DbeDetermineString";
                    case Key.Pa1: return "DbeEnterDialogConversionMode";
                }
            }

            if (IsDefinedKey(key))
            {
                return key.ToString();
            }
            else
            {
                return null;
            }
        }


        public static bool IsDefinedKey(Key key)
        {
            return (int)key >= (int)Key.None && (int)key <= (int)Key.DeadCharProcessed;
        }
    }
}
