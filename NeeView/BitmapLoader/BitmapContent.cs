// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView
{
    // 基本情報
    public class FileBasicInfo
    {
        public long FileSize { get; set; } = -1;
        public DateTime? LastWriteTime { get; set; }
        public string Archiver { get; set; }
        public string Decoder { get; set; }

        public BitmapMetadata Metadata { get; set; }
    }

    // 画像情報
    public class BitmapContent
    {
        public BitmapSource Source { get; set; }
        public FileBasicInfo Info { get; set; }
    }

    //
    public class BitmapContentExif
    {
        public string ShotInfo { get; set; }
        public int ISOSpeedRatings { get; set; }
        public string Maker { get; set; }
        public string Model { get; set; }
        public DateTime? LastWriteTime { get; set; }

        public Fraction ExposureTime { get; set; }
        public Fraction FNumber { get; set; }
        public Fraction FocalLength { get; set; }

        //
        public BitmapContentExif(BitmapMetadata metadata)
        {
            if (metadata == null) return;

            var exif = new ExifAccessor(metadata);

            var dateTime = exif.DateTime;
            if (!string.IsNullOrEmpty(dateTime))
            {
                try
                {
                    var tokens = dateTime.Split(' ');
                    var newDateTime = tokens[0].Replace(':', '/') + " " + tokens[1];
                    LastWriteTime = DateTime.Parse(newDateTime);
                }
                catch { }
            }

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
                    shotInfo += ExposureTime.Denominator == 1 ? $"{ExposureTime.Numerator} 秒。" : $"{ExposureTime.Numerator}/{ExposureTime.Denominator} 秒。";
                if (FNumber != null && FNumber.Numerator > 0)
                    shotInfo += $" f/{FNumber.Value}";
                if (FocalLength != null && FocalLength.Numerator > 0)
                    shotInfo += $" {FocalLength.Value} mm";
            }
            if (!string.IsNullOrEmpty(shotInfo)) ShotInfo = shotInfo;
        }
    }
}
