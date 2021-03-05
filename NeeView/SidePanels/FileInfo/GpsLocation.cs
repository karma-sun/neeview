using NeeView.Media.Imaging.Metadata;
using System;
using System.Diagnostics;

namespace NeeView
{
    public class GpsLocation
    {
        ////public static string GoogleMapFormatA => @"https://www.google.com/maps/@{LatDeg},{LonDeg},15z";
        ////public static string GoogleMapFormatB => @"https://www.google.com/maps/place/{Lat}+{Lon}/";


        ExifGpsDegree _latitude;
        ExifGpsDegree _longitude;

        public GpsLocation(ExifGpsDegree latitude, ExifGpsDegree longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
        }

        public void OpenMap(string format)
        {
            if (format is null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            if (!_latitude.IsValid || !_longitude.IsValid) return;

            var s = format;
            s = s.Replace("{Lat}", _latitude.ToFormatString());
            s = s.Replace("{Lon}", _longitude.ToFormatString());
            s = s.Replace("{LatDeg}", _latitude.ToValueString("{0:F5}"));
            s = s.Replace("{LonDeg}", _longitude.ToValueString("{0:F5}"));

            ExternalProcess.Start(s);
        }
    }
}
