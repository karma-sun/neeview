using System;
using System.ComponentModel;
using System.Globalization;

namespace NeeView
{
    public class ModifierMouseButtonsConverter : TypeConverter
    {
        private const char ModifierDelimiter = '+';

        private static ModifierMouseButtons ModifierMouseButtonsFlag = ModifierMouseButtons.LeftButton | ModifierMouseButtons.MiddleButton | ModifierMouseButtons.RightButton | ModifierMouseButtons.XButton1 | ModifierMouseButtons.XButton2;


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
            if (destinationType == typeof(string))
            {
                if (context != null && context.Instance != null && context.Instance is ModifierMouseButtons)
                {
                    return (IsDefinedModifierMouseButtons((ModifierMouseButtons)context.Instance));
                }
            }
            return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
        {
            if (source is string str)
            {
                string modifiersToken = str.Trim();
                ModifierMouseButtons modifiers = GetModifierKeys(modifiersToken, CultureInfo.InvariantCulture);
                return modifiers;
            }
            throw GetConvertFromException(source);
        }


        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }

            if (destinationType == typeof(string))
            {
                ModifierMouseButtons modifiers = (ModifierMouseButtons)value;

                if (!IsDefinedModifierMouseButtons(modifiers))
                {
                    throw new InvalidEnumArgumentException(nameof(value), (int)modifiers, typeof(ModifierMouseButtons));
                }
                else
                {
                    string strModifiers = "";

                    if ((modifiers & ModifierMouseButtons.LeftButton) == ModifierMouseButtons.LeftButton)
                    {
                        strModifiers += MatchModifiers(ModifierMouseButtons.LeftButton);
                    }

                    if ((modifiers & ModifierMouseButtons.MiddleButton) == ModifierMouseButtons.MiddleButton)
                    {
                        if (strModifiers.Length > 0)
                            strModifiers += ModifierDelimiter;

                        strModifiers += MatchModifiers(ModifierMouseButtons.MiddleButton);
                    }

                    if ((modifiers & ModifierMouseButtons.RightButton) == ModifierMouseButtons.RightButton)
                    {
                        if (strModifiers.Length > 0)
                            strModifiers += ModifierDelimiter;

                        strModifiers += MatchModifiers(ModifierMouseButtons.RightButton);
                    }

                    if ((modifiers & ModifierMouseButtons.XButton1) == ModifierMouseButtons.XButton1)
                    {
                        if (strModifiers.Length > 0)
                            strModifiers += ModifierDelimiter;

                        strModifiers += MatchModifiers(ModifierMouseButtons.XButton1);
                    }

                    if ((modifiers & ModifierMouseButtons.XButton2) == ModifierMouseButtons.XButton2)
                    {
                        if (strModifiers.Length > 0)
                            strModifiers += ModifierDelimiter;

                        strModifiers += MatchModifiers(ModifierMouseButtons.XButton2);
                    }

                    return strModifiers;
                }
            }
            throw GetConvertToException(value, destinationType);
        }


        private ModifierMouseButtons GetModifierKeys(string modifiersToken, CultureInfo culture)
        {
            ModifierMouseButtons modifiers = ModifierMouseButtons.None;
            if (modifiersToken.Length != 0)
            {
                int offset = 0;
                do
                {
                    offset = modifiersToken.IndexOf(ModifierDelimiter);
                    string token = (offset < 0) ? modifiersToken : modifiersToken.Substring(0, offset);
                    token = token.Trim();
                    token = token.ToUpper(culture);

                    if (token == string.Empty)
                    {
                        break;
                    }

                    switch (token)
                    {
                        case "LEFTBUTTON":
                            modifiers |= ModifierMouseButtons.LeftButton;
                            break;

                        case "MIDDLEBUTTON":
                            modifiers |= ModifierMouseButtons.MiddleButton;
                            break;

                        case "RIGHTBUTTON":
                            modifiers |= ModifierMouseButtons.RightButton;
                            break;

                        case "XBUTTON1":
                            modifiers |= ModifierMouseButtons.XButton1;
                            break;

                        case "XBUTTON2":
                            modifiers |= ModifierMouseButtons.XButton2;
                            break;

                        default:
                            throw new NotSupportedException($"Unsupported modifier: {token}");
                    }

                    modifiersToken = modifiersToken.Substring(offset + 1);
                } while (offset != -1);
            }
            return modifiers;
        }

        public static bool IsDefinedModifierMouseButtons(ModifierMouseButtons modifierKeys)
        {
            return (modifierKeys == ModifierMouseButtons.None || (((int)modifierKeys & ~((int)ModifierMouseButtonsFlag)) == 0));
        }

        internal static string MatchModifiers(ModifierMouseButtons modifierKeys)
        {
            string modifiers = String.Empty;
            switch (modifierKeys)
            {
                case ModifierMouseButtons.LeftButton: modifiers = "LeftButton"; break;
                case ModifierMouseButtons.MiddleButton: modifiers = "MiddleButton"; break;
                case ModifierMouseButtons.RightButton: modifiers = "RightButton"; break;
                case ModifierMouseButtons.XButton1: modifiers = "XButton1"; break;
                case ModifierMouseButtons.XButton2: modifiers = "XButton2"; break;
            }
            return modifiers;
        }
    }
}
