using System;
using System.ComponentModel;
using System.Globalization;

namespace NeeView
{
    public class MouseHorizontalWheelActionConverter : TypeConverter
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
                    return (MouseHorizontalWheelActionConverter.IsDefinedMouseWheelAction((MouseHorizontalWheelAction)context.Instance));
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
                    return MouseHorizontalWheelAction.None;
                }

                switch (mouseActionToken)
                {
                    case "WHEELRIGHT": return MouseHorizontalWheelAction.WheelRight;
                    case "WHEELLEFT": return MouseHorizontalWheelAction.WheelLeft;
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
                MouseHorizontalWheelAction mouseActionValue = (MouseHorizontalWheelAction)value;
                if (MouseHorizontalWheelActionConverter.IsDefinedMouseWheelAction(mouseActionValue))
                {
                    switch (mouseActionValue)
                    {
                        case MouseHorizontalWheelAction.None: return string.Empty;
                        case MouseHorizontalWheelAction.WheelRight: return "WheelRight";
                        case MouseHorizontalWheelAction.WheelLeft: return "WheelLeft";
                    }
                }
                throw new InvalidEnumArgumentException(nameof(value), (int)mouseActionValue, typeof(MouseHorizontalWheelAction));
            }
            throw GetConvertToException(value, destinationType);
        }


        internal static bool IsDefinedMouseWheelAction(MouseHorizontalWheelAction mouseAction)
        {
            return (mouseAction >= MouseHorizontalWheelAction.None && mouseAction <= MouseHorizontalWheelAction.WheelLeft);
        }
    }
}
