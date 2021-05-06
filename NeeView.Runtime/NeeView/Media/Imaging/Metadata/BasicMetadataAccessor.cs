using NeeView.Numetrics;
using NeeView.Text;
using System;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging.Metadata
{
    /// <summary>
    /// basic metadata accessor for JPG and TIFF
    /// </summary>
    public class BasicMetadataAccessor : BitmapMetadataAccessor
    {
        // Photo Metadata Policies

        private const string policy_Title = "System.Title";
        private const string policy_Subject = "System.Subject";

        private const string policy_DateTaken = "System.Photo.DateTaken";
        private const string policy_DateAcquired = "System.DateAcquired";

        private const string policy_FNumber = "System.Photo.FNumber";
        private const string policy_ExposureTime = "System.Photo.ExposureTime.Proxy";
        private const string policy_ISOSpeed = "System.Photo.ISOSpeed";
        private const string policy_ExposureBias = "System.Photo.ExposureBias";
        private const string policy_FocalLength = "System.Photo.FocalLength";
        private const string policy_MaxAperture = "System.Photo.MaxAperture";
        private const string policy_MeteringMode = "System.Photo.MeteringMode";
        private const string policy_SubjectDistance = "System.Photo.SubjectDistance";
        private const string policy_Flash = "System.Photo.Flash";
        private const string policy_FlashEnergy = "System.Photo.FlashEnergy";
        private const string policy_FocalLengthIn35mmFilm = "System.Photo.FocalLengthInFilm";

        private const string policy_LensManufacturer = "System.Photo.LensManufacturer";
        private const string policy_LensModel = "System.Photo.LensModel";
        private const string policy_FlashManufacturer = "System.Photo.FlashManufacturer";
        private const string policy_FlashModel = "System.Photo.FlashModel";
        private const string policy_CameraSerialNumber = "System.Photo.CameraSerialNumber";
        private const string policy_Contrast = "System.Photo.Contrast";
        private const string policy_Brightness = "System.Photo.Brightness";
        private const string policy_LightSource = "System.Photo.LightSource";
        private const string policy_ExposureProgram = "System.Photo.ExposureProgram";
        private const string policy_Saturation = "System.Photo.Saturation";
        private const string policy_Sharpness = "System.Photo.Sharpness";
        private const string policy_WhiteBalance = "System.Photo.WhiteBalance";
        private const string policy_PhotometricInterpretation = "System.Photo.PhotometricInterpretation";
        private const string policy_DigitalZoom = "System.Photo.DigitalZoom";
        private const string policy_Orientation = "System.Photo.Orientation";
        private const string policy_EXIFVersion = "System.Photo.EXIFVersion";

        private const string policy_Latitude = "System.GPS.Latitude.Proxy";
        private const string policy_Longitude = "System.GPS.Longitude.Proxy";
        private const string policy_Altitude = "System.GPS.Altitude";
        private const string policy_AltitudeRef = "System.GPS.AltitudeRef";



        // NOTE: _meta の寿命に注意。ソース が閉じられるとこのデータアクセスは保証されない
        private BitmapMetadata _metadata;


        public BasicMetadataAccessor(BitmapMetadata metadata)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }


        protected BitmapMetadata Metadata => _metadata;


        public override string GetFormat()
        {
            return _metadata.Format.ToUpper();
        }

        public override object GetValue(BitmapMetadataKey key)
        {
            if (_metadata is null) return null;

            switch (key)
            {
                // -- Description
                case BitmapMetadataKey.Title: return _metadata.Title;
                case BitmapMetadataKey.Subject: return _metadata.Subject;
                case BitmapMetadataKey.Rating: return new ExifRating(_metadata.Rating);
                case BitmapMetadataKey.Tags: return _metadata.Keywords;
                case BitmapMetadataKey.Comments: return _metadata.Comment;

                // -- Origin
                case BitmapMetadataKey.Author: return _metadata.Author;
                case BitmapMetadataKey.DateTaken: return GetDateTime(policy_DateTaken);
                case BitmapMetadataKey.ApplicatoinName: return _metadata.ApplicationName;
                case BitmapMetadataKey.DateAcquired: return GetDateTime(policy_DateAcquired);
                case BitmapMetadataKey.Copyright: return _metadata.Copyright;

                // -- Camera
                case BitmapMetadataKey.CameraMaker: return _metadata.CameraManufacturer;
                case BitmapMetadataKey.CameraModel: return _metadata.CameraModel;
                case BitmapMetadataKey.FNumber: return new FormatValue(_metadata.GetQuery(policy_FNumber), "f/{0:0.##}");
                case BitmapMetadataKey.ExposureTime: return new FormatValue(GetRational(policy_ExposureTime), "{0} s");
                case BitmapMetadataKey.ISOSpeed: return _metadata.GetQuery(policy_ISOSpeed);
                case BitmapMetadataKey.ExposureBias: return new FormatValue(_metadata.GetQuery(policy_ExposureBias), "{0:+0.#;-0.#;0} step");
                case BitmapMetadataKey.FocalLength: return new FormatValue(_metadata.GetQuery(policy_FocalLength), "{0:0.##} mm");
                case BitmapMetadataKey.MaxAperture: return new FormatValue(_metadata.GetQuery(policy_MaxAperture), "{0:0.##}");
                case BitmapMetadataKey.MeteringMode: return GetEnumValue<ExifMeteringMode>(policy_MeteringMode); ;
                case BitmapMetadataKey.SubjectDistance: return new FormatValue(_metadata.GetQuery(policy_SubjectDistance), "{0:0.##} m");
                case BitmapMetadataKey.FlashMode: return GetFlashMode(policy_Flash);
                case BitmapMetadataKey.FlashEnergy: return new FormatValue(_metadata.GetQuery(policy_FlashEnergy), "{0:0.##} bcps");
                case BitmapMetadataKey.FocalLengthIn35mmFilm: return new FormatValue(_metadata.GetQuery(policy_FocalLengthIn35mmFilm), "{0:0.##} mm");

                // -- Advanced photo
                case BitmapMetadataKey.LensMaker: return _metadata.GetQuery(policy_LensManufacturer);
                case BitmapMetadataKey.LensModel: return _metadata.GetQuery(policy_LensModel);
                case BitmapMetadataKey.FlashMaker: return _metadata.GetQuery(policy_FlashManufacturer);
                case BitmapMetadataKey.FlashModel: return _metadata.GetQuery(policy_FlashModel);
                case BitmapMetadataKey.CameraSerialNumber: return _metadata.GetQuery(policy_CameraSerialNumber);
                case BitmapMetadataKey.Contrast: return GetEnumValue<ExifContrast>(policy_Contrast);
                case BitmapMetadataKey.Brightness: return new FormatValue(_metadata.GetQuery(policy_Brightness), "{0:0.##}");
                case BitmapMetadataKey.LightSource: return GetEnumValue<ExifLightSource>(policy_LightSource);
                case BitmapMetadataKey.ExposureProgram: return GetEnumValue<ExifExposureProgram>(policy_ExposureProgram);
                case BitmapMetadataKey.Saturation: return GetEnumValue<ExifSaturation>(policy_Saturation);
                case BitmapMetadataKey.Sharpness: return GetEnumValue<ExifSharpness>(policy_Sharpness);
                case BitmapMetadataKey.WhiteBalance: return GetEnumValue<ExifWhiteBalance>(policy_WhiteBalance);
                case BitmapMetadataKey.PhotometricInterpretation: return GetEnumValue<ExifPhotometricInterpretation>(policy_PhotometricInterpretation);
                case BitmapMetadataKey.DigitalZoom: return new FormatValue(_metadata.GetQuery(policy_DigitalZoom), "{0:0.##}");
                case BitmapMetadataKey.Orientation: return GetEnumValue<ExifOrientation>(policy_Orientation);
                case BitmapMetadataKey.EXIFVersion: return _metadata.GetQuery(policy_EXIFVersion);

                // -- GPS
                case BitmapMetadataKey.GPSLatitude: return GetDegreeMeterSeconds(policy_Latitude);
                case BitmapMetadataKey.GPSLongitude: return GetDegreeMeterSeconds(policy_Longitude);
                case BitmapMetadataKey.GPSAltitude: return new FormatValue(GetAltitude(policy_Altitude, policy_AltitudeRef), "{0:0.#} m");

                default: return null;
            }
        }

        private object GetEnumValue<T>(string policy)
            where T : Enum
        {
            var value = _metadata.GetQuery(policy);
            switch (value)
            {
                case null:
                case string _:
                    return value;
                default:
                    //return Enum.ToObject(typeof(T), Convert.ToInt32(value));
                    return Enum.ToObject(typeof(T), value);
            }
        }



        private object GetFlashMode(string policy)
        {
            var value = _metadata.GetQuery(policy);
            switch (value)
            {
                case null:
                case string _:
                    return value;
                default:
                    return ExifFlashModeExtensions.ToExifFlashMode(Convert.ToInt32(value));
            }
        }

        private object GetDateTime(string policy)
        {
            var value = _metadata.GetQuery(policy);

            if (value is System.Runtime.InteropServices.ComTypes.FILETIME time)
            {
                return DateTime.FromFileTime((((long)time.dwHighDateTime) << 32) + (uint)time.dwLowDateTime);
            }
            else if (value is string s)
            {
                if (DateTime.TryParse(s, out DateTime dateTime))
                {
                    return dateTime.ToLocalTime();
                }
            }

            return value;
        }


        private object GetDegreeMeterSeconds(string policy)
        {
            var value = _metadata.GetQuery(policy);
            if (value is string s)
            {
                return new ExifGpsDegree(s);
            }

            return value;
        }

        private object GetRational(string policy)
        {
            var value = _metadata.GetQuery(policy);
            if (value is null) return null;

            switch (value)
            {
                case long svalue:
                    return BitmapMetadataUtilities.ExifI8ToRational(svalue);
                case ulong uvalue:
                    return BitmapMetadataUtilities.ExifUI8ToURational(uvalue);
                case string s:
                    return ConvertToRational(s);
                default:
                    return value;
            }
        }

        private object ConvertToRational(string value)
        {
            if (value is null)
            {
                return null;
            }

            if (URational.TryParse(value, out var uRational))
            {
                return uRational;
            }

            if (Rational.TryParse(value, out var rational))
            {
                return rational;
            }

            return value;
        }


        private object GetAltitude(string query, string queryRef)
        {
            var value = _metadata.GetQuery(query);
            if (value is double altitude)
            {
                if (_metadata.GetQuery(queryRef) is byte b && b == 0x01)
                {
                    return -altitude;
                }
                return altitude;
            }
            return value;
        }
    }

}
