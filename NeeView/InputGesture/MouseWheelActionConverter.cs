using System;
using System.ComponentModel;
using System.Globalization;

namespace NeeView
{
    public class MouseWheelActionConverter : TypeConverter
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
                    return (MouseWheelActionConverter.IsDefinedMouseWheelAction((MouseWheelAction)context.Instance));
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
                if (mouseActionToken == string.Empty)
                {
                    return MouseWheelAction.None;
                }

                switch (mouseActionToken)
                {
                    case "WHEELUP": return MouseWheelAction.WheelUp;
                    case "WHEELDOWN": return MouseWheelAction.WheelDown;
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
                MouseWheelAction mouseActionValue = (MouseWheelAction)value;
                if (MouseWheelActionConverter.IsDefinedMouseWheelAction(mouseActionValue))
                {
                    switch (mouseActionValue)
                    {
                        case MouseWheelAction.None: return string.Empty;
                        case MouseWheelAction.WheelUp: return "WheelUp";
                        case MouseWheelAction.WheelDown: return "WheelDown";
                    }
                }
                throw new InvalidEnumArgumentException(nameof(value), (int)mouseActionValue, typeof(MouseWheelAction));
            }
            throw GetConvertToException(value, destinationType);
        }


        internal static bool IsDefinedMouseWheelAction(MouseWheelAction mouseAction)
        {
            return (mouseAction >= MouseWheelAction.None && mouseAction <= MouseWheelAction.WheelDown);
        }
    }
}
