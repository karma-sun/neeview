using NeeView.Media.Imaging.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView
{
    public static class BitmapMetadataKeyExtensions
    {
        private static Dictionary<BitmapMetadataKey, InformationGroup> _groupMap = new Dictionary<BitmapMetadataKey, InformationGroup>
        {
            // Description
            [BitmapMetadataKey.Title] = InformationGroup.Description,
            [BitmapMetadataKey.Subject] = InformationGroup.Description,
            [BitmapMetadataKey.Rating] = InformationGroup.Description,
            [BitmapMetadataKey.Tags] = InformationGroup.Description,
            [BitmapMetadataKey.Comments] = InformationGroup.Description,

            // -- Origin
            [BitmapMetadataKey.Author] = InformationGroup.Origin,
            [BitmapMetadataKey.DateTaken] = InformationGroup.Origin,
            [BitmapMetadataKey.ApplicatoinName] = InformationGroup.Origin,
            [BitmapMetadataKey.DateAcquired] = InformationGroup.Origin,
            [BitmapMetadataKey.Copyright] = InformationGroup.Origin,

            // -- Camera
            [BitmapMetadataKey.CameraMaker] = InformationGroup.Camera,
            [BitmapMetadataKey.CameraModel] = InformationGroup.Camera,
            [BitmapMetadataKey.FNumber] = InformationGroup.Camera,
            [BitmapMetadataKey.ExposureTime] = InformationGroup.Camera,
            [BitmapMetadataKey.ISOSpeed] = InformationGroup.Camera,
            [BitmapMetadataKey.ExposureBias] = InformationGroup.Camera,
            [BitmapMetadataKey.FocalLength] = InformationGroup.Camera,
            [BitmapMetadataKey.MaxAperture] = InformationGroup.Camera,
            [BitmapMetadataKey.MeteringMode] = InformationGroup.Camera,
            [BitmapMetadataKey.SubjectDistance] = InformationGroup.Camera,
            [BitmapMetadataKey.FlashMode] = InformationGroup.Camera,
            [BitmapMetadataKey.FlashEnergy] = InformationGroup.Camera,
            [BitmapMetadataKey.FocalLengthIn35mmFilm] = InformationGroup.Camera,

            // -- Advanced photo
            [BitmapMetadataKey.LensMaker] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.LensModel] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.FlashMaker] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.FlashModel] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.CameraSerialNumber] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.Contrast] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.Brightness] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.LightSource] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.ExposureProgram] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.Saturation] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.Sharpness] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.WhiteBalance] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.PhotometricInterpretation] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.DigitalZoom] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.Orientation] = InformationGroup.AdvancedPhoto,
            [BitmapMetadataKey.EXIFVersion] = InformationGroup.AdvancedPhoto,

            // -- GPS
            [BitmapMetadataKey.GPSLatitude] = InformationGroup.Gps,
            [BitmapMetadataKey.GPSLongitude] = InformationGroup.Gps,
            [BitmapMetadataKey.GPSAltitude] = InformationGroup.Gps,
        };

        static BitmapMetadataKeyExtensions()
        {
#if DEBUG
            // check
            foreach (BitmapMetadataKey key in Enum.GetValues(typeof(BitmapMetadataKey)))
            {
                Debug.Assert(_groupMap.ContainsKey(key));
            }
#endif
        }

        public static InformationGroup ToInformationGroup(this BitmapMetadataKey key)
        {
            return _groupMap[key];
        }
    }
}
