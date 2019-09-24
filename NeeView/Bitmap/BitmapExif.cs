using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// EXIF
    /// </summary>
    public class BitmapExif
    {
        #region Constructors

        public BitmapExif(BitmapMetadata metadata)
        {
            if (metadata == null) return;

            var exif = new ExifAccessor(metadata);

            DateTime = ExifDateFormatToDateTime(exif.DateTime);
            DateTimeOriginal = ExifDateFormatToDateTime(exif.DateTimeOriginal);
            Maker = exif.Maker;
            Model = exif.Model;
            ISOSpeedRatings = exif.ISOSpeedRatings;
            ExposureTime = exif.ExposureTime;
            FNumber = exif.FNumber;
            FocalLength = exif.FocalLength;

            ExposureTime?.Reduction();

            string shotInfo = "";
            {
                if (ExposureTime != null && ExposureTime.Numerator > 0)
                    shotInfo += ExposureTime.Denominator == 1 ? $"{ExposureTime.Numerator} " : $"{ExposureTime.Numerator}/{ExposureTime.Denominator} " + Properties.Resources.WordSec;
                if (FNumber != null && FNumber.Numerator > 0)
                    shotInfo += $" f/{FNumber.Value}";
                if (FocalLength != null && FocalLength.Numerator > 0)
                    shotInfo += $" {FocalLength.Value} mm";
            }
            if (!string.IsNullOrEmpty(shotInfo)) ShotInfo = shotInfo;
        }

        #endregion

        #region Properties

        public string ShotInfo { get; set; }
        public int ISOSpeedRatings { get; set; }
        public string Maker { get; set; }
        public string Model { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime DateTimeOriginal { get; set; }
        public Fraction ExposureTime { get; set; }
        public Fraction FNumber { get; set; }
        public Fraction FocalLength { get; set; }

        #endregion


        #region Methods

        private DateTime ExifDateFormatToDateTime(string src)
        {
            if (!string.IsNullOrEmpty(src))
            {
                try
                {
                    var tokens = src.Split(' ');
                    var newDateTime = tokens[0].Replace(':', '/') + " " + tokens[1];
                    return DateTime.Parse(newDateTime);
                }
                catch { }
            }

            return default;
        }

        #endregion
    }
}
