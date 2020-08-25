using System;
using System.ComponentModel;
using System.Globalization;

namespace NeeView
{
    public class MouseExActionConverter : TypeConverter
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
            if (destinationType == typeof(string))
            {
                if (context != null && context.Instance != null)
                {
                    return (MouseExActionConverter.IsDefinedMouseAction((MouseExAction)context.Instance));
                }
            }
            return false;
        }


        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
        {
            if (source != null && source is string)
            {
                string mouseActionToken = ((string)source).Trim();
                mouseActionToken = mouseActionToken.ToUpper(CultureInfo.InvariantCulture);
                if (mouseActionToken == String.Empty)
                {
                    return MouseExAction.None;
                }

                switch (mouseActionToken)
                {
                    case "LEFTCLICK": return MouseExAction.LeftClick;
                    case "RIGHTCLICK": return MouseExAction.RightClick;
                    case "MIDDLECLICK": return MouseExAction.MiddleClick;
                    case "WHEELCLICK": return MouseExAction.WheelClick;
                    case "LEFTDOUBLECLICK": return MouseExAction.LeftDoubleClick;
                    case "RIGHTDOUBLECLICK": return MouseExAction.RightDoubleClick;
                    case "MIDDLEDOUBLECLICK": return MouseExAction.MiddleDoubleClick;
                    case "XBUTTON1CLICK": return MouseExAction.XButton1Click;
                    case "XBUTTON1DOUBLECLICK": return MouseExAction.XButton1DoubleClick;
                    case "XBUTTON2CLICK": return MouseExAction.XButton2Click;
                    case "XBUTTON2DOUBLECLICK": return MouseExAction.XButton2DoubleClick;
                }
            }
            throw GetConvertFromException(source);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException(nameof(destinationType));

            if (destinationType == typeof(string) && value != null)
            {
                MouseExAction mouseActionValue = (MouseExAction)value;
                if (MouseExActionConverter.IsDefinedMouseAction(mouseActionValue))
                {
                    switch (mouseActionValue)
                    {
                        case MouseExAction.None: return string.Empty;
                        case MouseExAction.LeftClick: return "LeftClick";
                        case MouseExAction.RightClick: return "RightClick";
                        case MouseExAction.MiddleClick: return "MiddleClick";
                        case MouseExAction.WheelClick: return "WheelClick";
                        case MouseExAction.LeftDoubleClick: return "LeftDoubleClick";
                        case MouseExAction.RightDoubleClick: return "RightDoubleClick";
                        case MouseExAction.MiddleDoubleClick: return "MiddleDoubleClick";
                        case MouseExAction.XButton1Click: return "XButton1Click";
                        case MouseExAction.XButton1DoubleClick: return "XButton1DoubleClick";
                        case MouseExAction.XButton2Click: return "XButton2Click";
                        case MouseExAction.XButton2DoubleClick: return "XButton2DoubleClick";
                    }
                }
                throw new InvalidEnumArgumentException(nameof(value), (int)mouseActionValue, typeof(MouseExAction));
            }
            throw GetConvertToException(value, destinationType);
        }


        internal static bool IsDefinedMouseAction(MouseExAction mouseAction)
        {
            return (mouseAction >= MouseExAction.None && mouseAction <= MouseExAction.XButton2DoubleClick);
        }
    }
}
