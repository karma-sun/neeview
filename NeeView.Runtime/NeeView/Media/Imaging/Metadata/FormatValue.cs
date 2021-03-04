using NeeView.Numetrics;

namespace NeeView.Media.Imaging.Metadata
{
    public class FormatValue
    {
        private object _value;
        private string _format;
        private FormatValueAttribute _attributes;


        public FormatValue(object value) : this(value, null, FormatValueAttribute.None)
        {
        }

        public FormatValue(object value, string format) : this(value, format, FormatValueAttribute.None)
        {
        }

        public FormatValue(object value, string format, FormatValueAttribute attributes)
        {
            _value = value;
            _format = format;
            _attributes = attributes;
        }


        public bool IsEmpty => _value is null;


        private object ValidateValue()
        {
            if (_value is IRational rational)
            {
                if ((_attributes & FormatValueAttribute.Numetrical) == FormatValueAttribute.Numetrical)
                {
                    return rational.ToValue();
                }
                if ((_attributes & FormatValueAttribute.Reduction) == FormatValueAttribute.Reduction)
                {
                    return rational.Reduction();
                }
            }
            else if (_value is ExifGpsDegree dms)
            {
                if ((_attributes & FormatValueAttribute.Numetrical) == FormatValueAttribute.Numetrical)
                {
                    return dms.ToValue();
                }
            }

            return _value;
        }

        public string ToFormatString()
        {
            if (_value is null)
            {
                return null;
            }
            else if (_format != null)
            {
                return string.Format(_format, ValidateValue());
            }
            else
            {
                return ValidateValue()?.ToString();
            }
        }

        public override string ToString()
        {
            return ToFormatString();
        }
    }

}
