using System;
using System.Globalization;

namespace NeeView.Media.Imaging.Metadata
{
    public class ExifDateTime
    {
        private string _value;

        public ExifDateTime(string value)
        {
            _value = value;
        }


        public static bool TryParse(string s, out DateTime dateTime)
        {
            return DateTime.TryParseExact(s?.Trim(), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dateTime);
        }

        private DateTime ToDateTime()
        {
            if (string.IsNullOrEmpty(_value)) return default;

            if (TryParse(_value, out var dateTime))
            {
                return dateTime;
            }
            else
            {
                return default;
            }
        }

        public override string ToString()
        {
            return ToDateTime().ToString();
        }
    }
}

