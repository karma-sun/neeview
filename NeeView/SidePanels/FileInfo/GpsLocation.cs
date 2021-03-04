using NeeView.Media.Imaging.Metadata;
using System;
using System.Diagnostics;

namespace NeeView
{
    public class GpsLocation
    {
        public static string GoogleMapFormatA => @"https://www.google.com/maps/@$LatDeg,$LonDeg,15z";
        public static string GoogleMapFormatB => @"https://www.google.com/maps/place/$LatDms+$LonDms/";


        ExifGpsDegree _latitude;
        ExifGpsDegree _longitude;

        public GpsLocation(ExifGpsDegree latitude, ExifGpsDegree longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
        }

        public bool OpenMap()
        {
            return OpenMap(null);
        }

        public bool OpenMap(string format)
        {
            if (!_latitude.IsValid || !_longitude.IsValid) return false;

            var s = format ?? GoogleMapFormatB;
            s = s.Replace("$LatDeg", _latitude.ToValueString("{0:F5}"));
            s = s.Replace("$LonDeg", _longitude.ToValueString("{0:F5}"));
            s = s.Replace("$LatDms", _latitude.ToFormatString("{0}°{1:00}'{2:00.#}\"{3}"));
            s = s.Replace("$LonDms", _longitude.ToFormatString("{0}°{1:00}'{2:00.#}\"{3}"));

            try
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = s,
                    UseShellExecute = true,
                };

                Debug.WriteLine(startInfo.FileName);
                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
