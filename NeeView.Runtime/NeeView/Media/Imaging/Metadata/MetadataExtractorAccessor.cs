using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileType;
using MetadataExtractor.Formats.Xmp;
using NeeView.Numetrics;
using NeeView.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging.Metadata
{
    /// <summary>
    /// basic metadata accessor for JPG and TIFF
    /// </summary>
    public class MetadataExtractorAccessor : BitmapMetadataAccessor
    {
        private IEnumerable<MetadataExtractor.Directory> _metadata;
        private List<ExifIfd0Directory> _ifd0;
        private List<ExifSubIfdDirectory> _subIfd;
        private List<GpsDirectory> _gps;
        private List<XmpDirectory> _xmp;
        private List<PanasonicRawIfd0Directory> _panasonicifd0;



        public MetadataExtractorAccessor(Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            _metadata = ImageMetadataReader.ReadMetadata(stream);

            _ifd0 = _metadata.OfType<ExifIfd0Directory>().ToList();
            _subIfd = _metadata.OfType<ExifSubIfdDirectory>().ToList();
            _gps = _metadata.OfType<GpsDirectory>().ToList();
            _xmp = _metadata.OfType<XmpDirectory>().ToList();
            _panasonicifd0 = _metadata.OfType<PanasonicRawIfd0Directory>().ToList();

            ////Dump();
        }


        [Conditional("DEBUG")]
        private void Dump()
        {
            foreach (var directory in _metadata)
            {
                if (directory is XmpDirectory xmpDirectory)
                {
                    foreach (var property in xmpDirectory.XmpMeta.Properties)
                    {
                        Debug.WriteLine($"{directory.Name} - {property.Path} ({property.Namespace}) = {property.Value}");
                    }
                }
                else
                {
                    foreach (var tag in directory.Tags)
                    {
                        Debug.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");
                    }
                }
            }

            if (_xmp != null)
            {
                foreach (var xmp in _xmp)
                {
                    Debug.WriteLine(xmp.XmpMeta.DumpObject());
                }
            }
        }

        public override string GetFormat()
        {
            var directory = _metadata.OfType<FileTypeDirectory>().FirstOrDefault();
            if (directory != null)
            {
                if (directory.ContainsTag(FileTypeDirectory.TagDetectedFileTypeName))
                {
                    return directory.GetDescription(FileTypeDirectory.TagDetectedFileTypeName);
                }
            }

            return "(Unknown)";
        }

        public override object GetValue(BitmapMetadataKey key)
        {
            if (_metadata is null) return null;

            switch (key)
            {
                // -- Description
                case BitmapMetadataKey.Title: return GetTitle();
                case BitmapMetadataKey.Subject: return GetSubject();
                case BitmapMetadataKey.Rating: return GetSimpleRatiing();
                case BitmapMetadataKey.Tags: return GetKeywords();
                case BitmapMetadataKey.Comments: return GeCommente();

                // -- Origin
                case BitmapMetadataKey.Author: return GetAuthor();
                case BitmapMetadataKey.DateTaken: return GetDateTaken();
                case BitmapMetadataKey.ApplicatoinName: return GetApplicationName();
                case BitmapMetadataKey.DateAcquired: return GetDateAcquired();
                case BitmapMetadataKey.Copyright: return GetCopyright();

                // -- Camera
                case BitmapMetadataKey.CameraMaker: return GetCameraManufacturer();
                case BitmapMetadataKey.CameraModel: return GetCameraModel();
                case BitmapMetadataKey.FNumber: return new FormatValue(GetFNumber(), "f/{0:0.##}");
                case BitmapMetadataKey.ExposureTime: return new FormatValue(GetExposureTime(), "{0} s");
                case BitmapMetadataKey.ISOSpeed: return GetISOSpeed();
                case BitmapMetadataKey.ExposureBias: return new FormatValue(GetExposureBias(), "{0:+0.#;-0.#;0} step");
                case BitmapMetadataKey.FocalLength: return new FormatValue(GetFocalLength(), "{0:0.##} mm");
                case BitmapMetadataKey.MaxAperture: return new FormatValue(GetMaxAperture(), "{0:0.##}");
                case BitmapMetadataKey.MeteringMode: return GetEnumValue<ExifMeteringMode>(GetMeteringMode());
                case BitmapMetadataKey.SubjectDistance: return new FormatValue(GetSubjectDistance(), "{0:0.##} m");
                case BitmapMetadataKey.FlashMode: return GetFlashMode(GetFlash());
                case BitmapMetadataKey.FlashEnergy: return new FormatValue(GetFlashEnergy(), "{0:0.##} bcps");
                case BitmapMetadataKey.FocalLengthIn35mmFilm: return new FormatValue(GetFocalLengthIn35mmFilm(), "{0:0.##} mm");

                // -- Advanced photo
                case BitmapMetadataKey.LensMaker: return GetLensManufacturer();
                case BitmapMetadataKey.LensModel: return GetLensModel();
                case BitmapMetadataKey.FlashMaker: return GetFlashManufacturer();
                case BitmapMetadataKey.FlashModel: return GetFlashModel();
                case BitmapMetadataKey.CameraSerialNumber: return GetCameraSerialNumber();
                case BitmapMetadataKey.Contrast: return GetEnumValue<ExifContrast>(GetContrast());
                case BitmapMetadataKey.Brightness: return new FormatValue(GetBrightness(), "{0:0.##}");
                case BitmapMetadataKey.LightSource: return GetEnumValue<ExifLightSource>(GetLightSource());
                case BitmapMetadataKey.ExposureProgram: return GetEnumValue<ExifExposureProgram>(GetExposureProgram());
                case BitmapMetadataKey.Saturation: return GetEnumValue<ExifSaturation>(GetSaturation());
                case BitmapMetadataKey.Sharpness: return GetEnumValue<ExifSharpness>(GetSharpness());
                case BitmapMetadataKey.WhiteBalance: return GetEnumValue<ExifWhiteBalance>(GetWhiteBalance());
                case BitmapMetadataKey.PhotometricInterpretation: return GetEnumValue<ExifPhotometricInterpretation>(GetPhotometricInterpretation());
                case BitmapMetadataKey.DigitalZoom: return new FormatValue(GetDigitalZoom(), "{0:0.##}");
                case BitmapMetadataKey.Orientation: return GetEnumValue<ExifOrientation>(GetOrientation());
                case BitmapMetadataKey.EXIFVersion: return GetEXIFVersion();

                // -- GPS
                case BitmapMetadataKey.GPSLatitude: return GetGPSLatitude();
                case BitmapMetadataKey.GPSLongitude: return GetGPSLongitude();
                case BitmapMetadataKey.GPSAltitude: return new FormatValue(GetGPSAltitude(), "{0:0.#} m");

                default: return null;
            }
        }


        #region Meta policies

        // based on https://docs.microsoft.com/en-us/windows/win32/wic/photo-metadata-policies
        // EXIF and XMP supported.

        public static class SchemaEx
        {
            // NOTE: MicrosoftPhoto ネームスペースは末尾パス有無バージョンがある。カオス。
            public static readonly string MicrosoftPhoto = "http://ns.microsoft.com/photo/1.0/";
            public static readonly string MicrosoftPhotoX = "http://ns.microsoft.com/photo/1.0";
        }

        private string GetTitle()
        {
            return GetString(_ifd0, ExifDirectoryBase.TagWinTitle)
                ?? GetString(_xmp, Schema.DublinCoreSpecificProperties, "title")
                ?? GetString(_subIfd, ExifDirectoryBase.TagUserComment)
                ?? GetString(_ifd0, ExifDirectoryBase.TagImageDescription)
                ?? GetString(_panasonicifd0, ExifDirectoryBase.TagImageDescription)
                ?? GetString(_xmp, Schema.DublinCoreSpecificProperties, "description")
                ?? GetString(_xmp, Schema.ExifSpecificProperties, "UserComment");
        }

        private string GetSubject()
        {
            return GetString(_ifd0, ExifDirectoryBase.TagWinSubject)
                ?? GetString(_ifd0, ExifDirectoryBase.TagImageDescription);
        }


        private ExifRating GetSimpleRatiing()
        {
            return new ExifRating(
                GetInteger(_ifd0, ExifDirectoryBase.TagRating)
                ?? GetInteger(_xmp, Schema.XmpProperties, "Rating")
                ?? 0);
        }

        private IReadOnlyCollection<string> GetKeywords()
        {
            return GetStringArray(_xmp, Schema.DublinCoreSpecificProperties, "subject")
                ?? GetString(_ifd0, ExifDirectoryBase.TagWinKeywords)?.Split(';');

            // NOTE: /ifd/{ushort=18247}(XP_DIP_XML) は非対応
        }

        private string GeCommente()
        {
            return GetString(_ifd0, ExifDirectoryBase.TagWinComment)
                ?? GetString(_ifd0, ExifDirectoryBase.TagUserComment)
                ?? GetString(_xmp, Schema.ExifSpecificProperties, "UserComment");
        }

        private string GetAuthor()
        {
            return GetString(_ifd0, ExifDirectoryBase.TagArtist)
                ?? GetString(_xmp, Schema.DublinCoreSpecificProperties, "creator")
                ?? GetString(_ifd0, ExifDirectoryBase.TagWinAuthor)
                ?? GetString(_xmp, Schema.ExifTiffProperties, "tiff:artist");
        }

        public DateTime? GetDateTaken()
        {
            return GetDateTime(_subIfd, ExifDirectoryBase.TagDateTimeOriginal)
                ?? GetDateTime(_xmp, Schema.XmpProperties, "CreateDate")
                ?? GetDateTime(_subIfd, ExifDirectoryBase.TagDateTimeDigitized)
                ?? GetDateTime(_xmp, Schema.ExifSpecificProperties, "DateTimeOriginal");
        }

        private string GetApplicationName()
        {
            return GetString(_ifd0, ExifDirectoryBase.TagSoftware)
                ?? GetString(_xmp, Schema.XmpProperties, "CreatorTool")
                ?? GetString(_xmp, Schema.XmpProperties, "creatortool")
                ?? GetString(_xmp, Schema.ExifTiffProperties, "Software")
                ?? GetString(_xmp, Schema.ExifTiffProperties, "software");
        }

        private DateTime? GetDateAcquired()
        {
            return GetDateTime(_xmp, SchemaEx.MicrosoftPhoto, "DateAcquired")
                ?? GetDateTime(_xmp, SchemaEx.MicrosoftPhotoX, "DateAcquired");
        }

        private string GetCopyright()
        {
            return GetString(_ifd0, ExifDirectoryBase.TagCopyright)
                ?? GetString(_xmp, Schema.DublinCoreSpecificProperties, "rights");
        }

        private string GetCameraManufacturer()
        {
            return GetString(_ifd0, ExifDirectoryBase.TagMake)
                ?? GetString(_panasonicifd0, PanasonicRawIfd0Directory.TagMake)
                ?? GetString(_xmp, Schema.ExifTiffProperties, "Make")
                ?? GetString(_xmp, Schema.ExifTiffProperties, "make");
        }

        private string GetCameraModel()
        {
            return GetString(_ifd0, ExifDirectoryBase.TagModel)
                ?? GetString(_panasonicifd0, PanasonicRawIfd0Directory.TagModel)
                ?? GetString(_xmp, Schema.ExifTiffProperties, "Model")
                ?? GetString(_xmp, Schema.ExifTiffProperties, "model");
        }

        private double? GetFNumber()
        {
            return GetDouble(_subIfd, ExifDirectoryBase.TagFNumber)
                ?? GetDouble(_xmp, Schema.ExifSpecificProperties, "FNumber");
        }

        private IRational GetExposureTime()
        {
            return GetRational(_subIfd, ExifDirectoryBase.TagExposureTime)
                ?? GetRational(_xmp, Schema.ExifSpecificProperties, "ExposureTime");
        }

        private int? GetISOSpeed()
        {
            return GetInteger(_subIfd, ExifDirectoryBase.TagIsoEquivalent)
                ?? GetInteger(_xmp, Schema.ExifSpecificProperties, "ISOSpeedRatings")
                ?? GetInteger(_xmp, Schema.ExifSpecificProperties, "ISOSpeed");
        }

        private double? GetExposureBias()
        {
            return GetDouble(_subIfd, ExifDirectoryBase.TagExposureBias)
                ?? GetDouble(_xmp, Schema.ExifSpecificProperties, "ExposureBiasValue");
        }

        private double? GetFocalLength()
        {
            return GetDouble(_subIfd, ExifDirectoryBase.TagFocalLength)
                ?? GetDouble(_xmp, Schema.ExifSpecificProperties, "FocalLength");
        }

        private double? GetMaxAperture()
        {
            return GetDouble(_subIfd, ExifDirectoryBase.TagMaxAperture)
                ?? GetDouble(_xmp, Schema.ExifSpecificProperties, "MaxApertureValue");
        }

        private int? GetMeteringMode()
        {
            return GetInteger(_subIfd, ExifDirectoryBase.TagMeteringMode)
                ?? GetInteger(_xmp, Schema.ExifSpecificProperties, "MeteringMode");
        }

        private double? GetSubjectDistance()
        {
            return GetDouble(_subIfd, ExifDirectoryBase.TagSubjectDistance)
                ?? GetDouble(_xmp, Schema.ExifSpecificProperties, "SubjectDistance");
        }

        private int? GetFlash()
        {
            return GetInteger(_subIfd, ExifDirectoryBase.TagFlash)
                ?? GetInteger(_xmp, Schema.ExifSpecificProperties, "Flash");
        }

        private double? GetFlashEnergy()
        {
            return GetDouble(_subIfd, ExifDirectoryBase.TagFlashEnergy)
                ?? GetDouble(_xmp, Schema.ExifSpecificProperties, "FlashEnergy");
        }

        private int? GetFocalLengthIn35mmFilm()
        {
            return GetInteger(_subIfd, ExifDirectoryBase.Tag35MMFilmEquivFocalLength)
                ?? GetInteger(_xmp, Schema.ExifSpecificProperties, "FocalLengthIn35mmFilm");
        }

        private string GetLensManufacturer()
        {
            return GetString(_subIfd, ExifDirectoryBase.TagLensMake)
                ?? GetString(_xmp, Schema.ExifSpecificProperties, "LensMake")
                ?? GetString(_xmp, SchemaEx.MicrosoftPhoto, "LensManufacturer")
                ?? GetString(_xmp, SchemaEx.MicrosoftPhotoX, "LensManufacturer");
        }

        private string GetLensModel()
        {
            return GetString(_subIfd, ExifDirectoryBase.TagLensModel)
                ?? GetString(_xmp, Schema.ExifSpecificProperties, "LensModel")
                ?? GetString(_xmp, SchemaEx.MicrosoftPhoto, "LensModel")
                ?? GetString(_xmp, SchemaEx.MicrosoftPhotoX, "LensModel");
        }

        private string GetFlashManufacturer()
        {
            return GetString(_xmp, SchemaEx.MicrosoftPhoto, "FlashManufacturer")
                ?? GetString(_xmp, SchemaEx.MicrosoftPhotoX, "FlashManufacturer");
        }

        private string GetFlashModel()
        {
            return GetString(_xmp, SchemaEx.MicrosoftPhoto, "FlashModel")
                ?? GetString(_xmp, SchemaEx.MicrosoftPhotoX, "FlashModel");
        }

        private string GetCameraSerialNumber()
        {
            return GetString(_subIfd, ExifDirectoryBase.TagBodySerialNumber)
                ?? GetString(_xmp, Schema.ExifSpecificProperties, "BodySerialNumber")
                ?? GetString(_xmp, SchemaEx.MicrosoftPhoto, "CameraSerialNumber")
                ?? GetString(_xmp, SchemaEx.MicrosoftPhotoX, "CameraSerialNumber");
        }

        private int? GetContrast()
        {
            return GetInteger(_subIfd, ExifDirectoryBase.TagContrast)
                ?? GetInteger(_xmp, Schema.ExifSpecificProperties, "Contrast");
        }

        private double? GetBrightness()
        {
            return GetDouble(_subIfd, ExifDirectoryBase.TagBrightnessValue)
                ?? GetDouble(_xmp, Schema.ExifSpecificProperties, "BrightnessValue");
        }

        private int? GetLightSource()
        {
            return GetInteger(_subIfd, ExifDirectoryBase.TagWhiteBalance)
                ?? GetInteger(_xmp, Schema.ExifSpecificProperties, "LightSource");
        }

        private int? GetExposureProgram()
        {
            return GetInteger(_subIfd, ExifDirectoryBase.TagExposureProgram)
                ?? GetInteger(_xmp, Schema.ExifSpecificProperties, "ExposureProgram");
        }

        private int? GetSaturation()
        {
            return GetInteger(_subIfd, ExifDirectoryBase.TagSaturation)
                ?? GetInteger(_xmp, Schema.ExifSpecificProperties, "Saturation");
        }

        private int? GetSharpness()
        {
            return GetInteger(_subIfd, ExifDirectoryBase.TagSharpness)
                ?? GetInteger(_xmp, Schema.ExifSpecificProperties, "Sharpness");
        }

        private int? GetWhiteBalance()
        {
            return GetInteger(_subIfd, ExifDirectoryBase.TagWhiteBalanceMode)
                ?? GetInteger(_xmp, Schema.ExifSpecificProperties, "WhiteBalance");
        }

        private int? GetPhotometricInterpretation()
        {
            return GetInteger(_ifd0, ExifDirectoryBase.TagPhotometricInterpretation)
                ?? GetInteger(_xmp, Schema.ExifTiffProperties, "PhotometricInterpretation");
        }

        private double? GetDigitalZoom()
        {
            return GetDouble(_subIfd, ExifDirectoryBase.TagDigitalZoomRatio)
                ?? GetDouble(_xmp, Schema.ExifSpecificProperties, "DigitalZoomRatio");
        }

        private int? GetOrientation()
        {
            return GetInteger(_ifd0, ExifDirectoryBase.TagOrientation)
                ?? GetInteger(_panasonicifd0, PanasonicRawIfd0Directory.TagOrientation)
                ?? GetInteger(_xmp, Schema.ExifTiffProperties, "Orientation");
        }

        private string GetEXIFVersion()
        {
            return GetString(_subIfd, ExifDirectoryBase.TagExifVersion)
                ?? GetString(_xmp, Schema.ExifSpecificProperties, "ExifVersion");
        }

        private ExifGpsDegree GetGPSLatitude()
        {
            return GetGPSLatitude(_gps)
                ?? GetGPSLatitude(_xmp);
        }

        private ExifGpsDegree GetGPSLongitude()
        {
            return GetGPSLongitude(_gps)
                ?? GetGPSLongitude(_xmp);
        }

        private double? GetGPSAltitude()
        {
            return GetGPSAltitude(_gps)
                ?? GetGPSAltitude(_xmp);
        }

        #endregion

        #region EXIF

        private T GetValueFromDirectories<T>(IEnumerable<MetadataExtractor.Directory> directories, int tagType, Func<MetadataExtractor.Directory, int, T> func)
        {
            if (directories is null) return default;

            foreach (var directory in directories)
            {
                var value = func(directory, tagType);
                if (value != null)
                {
                    return value;
                }
            }

            return default;
        }

        private string GetString(MetadataExtractor.Directory directory, int tagType)
        {
            if (directory is null) return null;

            if (directory.ContainsTag(tagType))
            {
                return directory.GetDescription(tagType);
            }

            return null;
        }

        private string GetString(IEnumerable<MetadataExtractor.Directory> directories, int tagType)
        {
            return GetValueFromDirectories(directories, tagType, GetString);
        }

        private int? GetInteger(MetadataExtractor.Directory directory, int tagType)
        {
            if (directory is null) return null;

            if (directory.TryGetInt32(tagType, out var value))
            {
                return value;
            }

            return null;
        }

        private int? GetInteger(IEnumerable<MetadataExtractor.Directory> directories, int tagType)
        {
            return GetValueFromDirectories(directories, tagType, GetInteger);
        }

        private double? GetDouble(MetadataExtractor.Directory directory, int tagType)
        {
            if (directory is null) return null;

            if (directory.TryGetDouble(tagType, out var value))
            {
                return value;
            }

            return null;
        }

        private double? GetDouble(IEnumerable<MetadataExtractor.Directory> directories, int tagType)
        {
            return GetValueFromDirectories(directories, tagType, GetDouble);
        }

        private IRational GetRational(MetadataExtractor.Directory directory, int tagType)
        {
            if (directory is null) return null;

            if (directory.TryGetRational(tagType, out var value))
            {
                if (value.Numerator >= 0 && value.Denominator >= 0)
                {
                    return new URational((uint)value.Numerator, (uint)value.Denominator);
                }
                else
                {
                    return new Numetrics.Rational((int)value.Numerator, (int)value.Denominator);
                }
            }

            return null;
        }

        private IRational GetRational(IEnumerable<MetadataExtractor.Directory> directories, int tagType)
        {
            return GetValueFromDirectories(directories, tagType, GetRational);
        }

        private DateTime? GetDateTime(MetadataExtractor.Directory directory, int tagType)
        {
            if (directory is null) return null;

            if (directory.ContainsTag(tagType))
            {
                return directory.GetDateTime(tagType).ToLocalTime();
            }

            return null;
        }

        private DateTime? GetDateTime(IEnumerable<MetadataExtractor.Directory> directories, int tagType)
        {
            return GetValueFromDirectories(directories, tagType, GetDateTime);
        }

        #endregion EXIF

        #region XMP

        private T GetValueFromDirectories<T>(IEnumerable<XmpDirectory> directories, string schema, string path, Func<XmpDirectory, string, string, T> func)
        {
            if (directories is null) return default;

            foreach (var directory in directories)
            {
                var value = func(directory, schema, path);
                if (value != null)
                {
                    return value;
                }
            }

            return default;
        }

        private string GetString(XmpDirectory directory, string schema, string path)
        {
            if (directory is null) return null;

            var property = directory.XmpMeta.GetProperty(schema, path);
            if (property != null)
            {
                if (property.Options.IsArray)
                {
                    return string.Join(Environment.NewLine, GetStringArray(directory, schema, path));
                }
                else
                {
                    return property.Value;
                }
            }

            return null;
        }

        private string GetString(IEnumerable<XmpDirectory> directories, string schema, string path)
        {
            return GetValueFromDirectories(directories, schema, path, GetString);
        }

        private IReadOnlyCollection<string> GetStringArray(XmpDirectory directory, string schema, string path)
        {
            if (directory is null) return null;

            var count = directory.XmpMeta.CountArrayItems(schema, path);
            if (count > 0)
            {
                return Enumerable.Range(1, count).Select(e => directory.XmpMeta.GetArrayItem(schema, path, e).Value).ToArray();
            }

            return null;
        }

        private IReadOnlyCollection<string> GetStringArray(IEnumerable<XmpDirectory> directories, string schema, string path)
        {
            return GetValueFromDirectories(directories, schema, path, GetStringArray);
        }

        private int? GetInteger(XmpDirectory directory, string schema, string path)
        {
            if (directory is null) return null;

            var property = directory.XmpMeta.GetProperty(schema, path);
            if (property != null)
            {
                return directory.XmpMeta.GetPropertyInteger(schema, path);
            }

            return null;
        }

        private int? GetInteger(IEnumerable<XmpDirectory> directories, string schema, string path)
        {
            return GetValueFromDirectories(directories, schema, path, GetInteger);
        }

        private double? GetDouble(XmpDirectory directory, string schema, string path)
        {
            if (directory is null) return null;

            var property = directory.XmpMeta.GetProperty(schema, path);
            if (property != null)
            {
                if (property.Value.Contains('/'))
                {
                    var rational = GetRational(directory, schema, path);
                    return rational.ToValue();
                }
                else
                {
                    return directory.XmpMeta.GetPropertyDouble(schema, path);
                }
            }

            return null;
        }

        private double? GetDouble(IEnumerable<XmpDirectory> directories, string schema, string path)
        {
            return GetValueFromDirectories(directories, schema, path, GetDouble);
        }


        private IRational GetRational(XmpDirectory directory, string schema, string path)
        {
            if (directory is null) return null;

            var property = directory.XmpMeta.GetProperty(schema, path);
            if (property != null)
            {
                return ConvertToRational(property.Value);
            }

            return null;
        }

        private IRational GetRational(IEnumerable<XmpDirectory> directories, string schema, string path)
        {
            return GetValueFromDirectories(directories, schema, path, GetRational);
        }

        private DateTime? GetDateTime(XmpDirectory directory, string schema, string path)
        {
            if (directory is null) return null;

            var property = directory.XmpMeta.GetProperty(schema, path);
            if (property != null)
            {
                return ConvertToDateTime(property.Value)?.ToLocalTime();
            }

            return null;
        }

        private DateTime? GetDateTime(IEnumerable<XmpDirectory> directories, string schema, string path)
        {
            return GetValueFromDirectories(directories, schema, path, GetDateTime);
        }

        private ExifGpsDegree GetGPSLatitude(IEnumerable<XmpDirectory> directories)
        {
            if (directories is null) return null;

            foreach (var directory in directories)
            {
                var property = directory.XmpMeta.GetProperty(Schema.ExifSpecificProperties, "GPSLatitude");
                if (property != null)
                {
                    return new ExifGpsDegree(property.Value);
                }
            }

            return null;
        }

        private ExifGpsDegree GetGPSLongitude(IEnumerable<XmpDirectory> directories)
        {
            if (directories is null) return null;

            foreach (var directory in directories)
            {
                var property = directory.XmpMeta.GetProperty(Schema.ExifSpecificProperties, "GPSLongitude");
                if (property != null)
                {
                    return new ExifGpsDegree(property.Value);
                }
            }

            return null;
        }

        private double? GetGPSAltitude(XmpDirectory directory)
        {
            var altitude = GetDouble(directory, Schema.ExifSpecificProperties, "GPSAltitude");
            if (altitude is null)
            {
                return null;
            }

            var altitudeRef = GetInteger(directory, Schema.ExifSpecificProperties, "GPSAltitudeRef");
            return (double)altitude * (altitudeRef == 1 ? -1.0 : 1.0);
        }

        private double? GetGPSAltitude(IEnumerable<XmpDirectory> directories)
        {
            if (directories is null) return null;

            foreach (var directory in directories)
            {
                var altitude = GetGPSAltitude(directory);
                if (altitude != null)
                {
                    return altitude;
                }
            }

            return null;
        }

        #endregion XMP

        #region EXIF.GPS

        private ExifGpsDegree GetGPSLatitude(IEnumerable<GpsDirectory> directories)
        {
            if (directories is null) return null;

            foreach (var directory in directories)
            {
                var location = directory.GetGeoLocation();
                if (location != null)
                {
                    var reference = location.Latitude < 0.0 ? "S" : "N";
                    var degree = Math.Abs(location.Latitude);
                    return new ExifGpsDegree(reference, degree);
                }
            }

            return null;
        }

        private ExifGpsDegree GetGPSLongitude(IEnumerable<GpsDirectory> directories)
        {
            if (directories is null) return null;

            foreach (var directory in directories)
            {
                var location = directory.GetGeoLocation();
                if (location != null)
                {
                    var reference = location.Longitude < 0.0 ? "W" : "E";
                    var degree = Math.Abs(location.Longitude);
                    return new ExifGpsDegree(reference, degree);
                }
            }

            return null;
        }


        private double? GetGPSAltitude(GpsDirectory directory)
        {
            if (directory is null) return null;

            var altitude = GetDouble(directory, GpsDirectory.TagAltitude);
            if (altitude is null)
            {
                return null;
            }

            var altitudeRef = GetInteger(directory, GpsDirectory.TagAltitudeRef);
            return (double)altitude * (altitudeRef == 1 ? -1.0 : 1.0);
        }

        private double? GetGPSAltitude(IEnumerable<GpsDirectory> directories)
        {
            if (directories is null) return null;

            foreach (var directory in directories)
            {
                var altitude = GetGPSAltitude(directory);
                if (altitude != null)
                {
                    return altitude;
                }
            }

            return null;
        }

        #endregion EXIF.GPS


        public static DateTime? ConvertToDateTime(string value)
        {
            if (ExifDateTime.TryParse(value, out var dateTime))
            {
                return dateTime;
            }
            else if (DateTime.TryParse(value, out dateTime))
            {
                return dateTime;
            }

            return null;
        }

        private IRational ConvertToRational(string value)
        {
            if (value is null)
            {
                return null;
            }

            if (URational.TryParse(value, out var uRational))
            {
                return uRational;
            }

            if (Numetrics.Rational.TryParse(value, out var rational))
            {
                return rational;
            }

            return null;
        }

        private object GetEnumValue<T>(object value)
            where T : Enum
        {
            switch (value)
            {
                case null:
                case string _:
                    return value;
                default:
                    return Enum.ToObject(typeof(T), value);
            }
        }

        private object GetFlashMode(object value)
        {
            switch (value)
            {
                case null:
                case string _:
                    return value;
                default:
                    return ExifFlashModeExtensions.ToExifFlashMode(Convert.ToInt32(value));
            }
        }

    }
}
