using System;
using System.Linq;

namespace NeeView.Media.Imaging.Metadata
{
    public class ExifGpsDegree
    {
        private static readonly char[] _references = "NEWS".ToCharArray();

        private string _value;
        private string _reference;
        private double _degree;

        public ExifGpsDegree(string value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ExifGpsDegree(string reference, double degree)
        {
            if (reference.Length != 1 || !_references.Contains(reference.First())) throw new ArgumentException(nameof(reference));
            if (degree < 0.0) throw new ArgumentException(nameof(degree));

            _reference = reference;
            _degree = degree;
        }


        public bool IsValid => !double.IsNaN(ToValue());

        private void Initialize()
        {
            if (_reference != null) return;

            try
            {
                var last = _value.Last();
                if (!_references.Contains(last))
                {
                    throw new FormatException();
                }

                var tokens = _value.Trim(last).Split(',');
                if (tokens.Length != 2)
                {
                    throw new FormatException();
                }

                _degree = double.Parse(tokens[0]) + double.Parse(tokens[1]) / 60.0;
                _reference = last.ToString();
            }
            catch
            {
                _degree = double.NaN;
                _reference = "";
            }
        }

        public double ToValue()
        {
            Initialize();

            if (double.IsNaN(_degree))
            {
                return _degree;
            }

            var sign = (_reference == "S" || _reference == "W") ? -1.0 : +1.0;
            return sign * _degree;
        }

        public string ToValueString(string format)
        {
            return string.Format(format, ToValue());
        }


        public string ToFormatString()
        {
            return ToFormatString("{0}°{1:00}'{2:00.0}\"{3}");
        }

        public string ToFormatString(string format)
        {
            Initialize();

            if (double.IsNaN(_degree))
            {
                return "";
            }

            var degree = Math.Truncate(_degree);
            var minutes = (_degree - degree) * 60.0;
            var minute = Math.Truncate(minutes);
            var second = (minutes - minute) * 60.0;

            return string.Format(format, degree, minute, second, _reference);
        }


        public override string ToString()
        {
            return ToFormatString();
        }
    }

}
