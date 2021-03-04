using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView.Media.Imaging.Metadata
{
    // NTOE: エクスプローラーのプロパティ準拠
    public enum BitmapMetadataKey
    {
        // Description
        Title,
        Subject,
        Rating,
        Tags,
        Comments,

        // -- Origin
        Author,
        DateTaken,
        ApplicatoinName,
        DateAcquired,
        Copyright,

        // -- Image
        // Dimensions // .. x ..
        // Width // .. pixels
        // Height // .. pixels
        // HorizontalResolution // .. dpi
        // VerticalResolution // .. dpi
        // BitDepth // ..
        // Compression
        // ResolutionUnit
        // ColorRepresentation
        // CompressedBitsPerPixel

        // -- Camera
        CameraMaker,
        CameraModel,
        FNumber,
        ExposureTime,
        ISOSpeed,
        ExposureBias,
        FocalLength,
        MaxAperture,
        MeteringMode,
        SubjectDistance,
        FlashMode,
        FlashEnergy,
        FocalLengthIn35mmFilm,

        // -- Advanced photo
        LensMaker,
        LensModel,
        FlashMaker,
        FlashModel,
        CameraSerialNumber,
        Contrast,
        Brightness,
        LightSource,
        ExposureProgram,
        Saturation,
        Sharpness,
        WhiteBalance,
        PhotometricInterpretation,
        DigitalZoom,
        EXIFVersion,

        // -- GPS
        GPSLatitude,
        GPSLongitude,
        GPSAltitude,
    }

    public enum BitmapMetadataGroup
    {
        Description,
        Origin,
        Image,
        Camera,
        AdvancedPhoto,
        GPS,
    }

    public static class BitmapMetadataKeyExtensions
    {
        private static Dictionary<BitmapMetadataKey, BitmapMetadataGroup> _groupMap = new Dictionary<BitmapMetadataKey, BitmapMetadataGroup>
        {
            // Description
            [BitmapMetadataKey.Title] = BitmapMetadataGroup.Description,
            [BitmapMetadataKey.Subject] = BitmapMetadataGroup.Description,
            [BitmapMetadataKey.Rating] = BitmapMetadataGroup.Description,
            [BitmapMetadataKey.Tags] = BitmapMetadataGroup.Description,
            [BitmapMetadataKey.Comments] = BitmapMetadataGroup.Description,

            // -- Origin
            [BitmapMetadataKey.Author] = BitmapMetadataGroup.Origin,
            [BitmapMetadataKey.DateTaken] = BitmapMetadataGroup.Origin,
            [BitmapMetadataKey.ApplicatoinName] = BitmapMetadataGroup.Origin,
            [BitmapMetadataKey.DateAcquired] = BitmapMetadataGroup.Origin,
            [BitmapMetadataKey.Copyright] = BitmapMetadataGroup.Origin,

            // -- Camera
            [BitmapMetadataKey.CameraMaker] = BitmapMetadataGroup.Camera,
            [BitmapMetadataKey.CameraModel] = BitmapMetadataGroup.Camera,
            [BitmapMetadataKey.FNumber] = BitmapMetadataGroup.Camera,
            [BitmapMetadataKey.ExposureTime] = BitmapMetadataGroup.Camera,
            [BitmapMetadataKey.ISOSpeed] = BitmapMetadataGroup.Camera,
            [BitmapMetadataKey.ExposureBias] = BitmapMetadataGroup.Camera,
            [BitmapMetadataKey.FocalLength] = BitmapMetadataGroup.Camera,
            [BitmapMetadataKey.MaxAperture] = BitmapMetadataGroup.Camera,
            [BitmapMetadataKey.MeteringMode] = BitmapMetadataGroup.Camera,
            [BitmapMetadataKey.SubjectDistance] = BitmapMetadataGroup.Camera,
            [BitmapMetadataKey.FlashMode] = BitmapMetadataGroup.Camera,
            [BitmapMetadataKey.FlashEnergy] = BitmapMetadataGroup.Camera,
            [BitmapMetadataKey.FocalLengthIn35mmFilm] = BitmapMetadataGroup.Camera,

            // -- Advanced photo
            [BitmapMetadataKey.LensMaker] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.LensModel] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.FlashMaker] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.FlashModel] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.CameraSerialNumber] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.Contrast] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.Brightness] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.LightSource] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.ExposureProgram] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.Saturation] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.Sharpness] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.WhiteBalance] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.PhotometricInterpretation] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.DigitalZoom] = BitmapMetadataGroup.AdvancedPhoto,
            [BitmapMetadataKey.EXIFVersion] = BitmapMetadataGroup.AdvancedPhoto,

            // -- GPS
            [BitmapMetadataKey.GPSLatitude] = BitmapMetadataGroup.GPS,
            [BitmapMetadataKey.GPSLongitude] = BitmapMetadataGroup.GPS,
            [BitmapMetadataKey.GPSAltitude] = BitmapMetadataGroup.GPS,
        };

        static BitmapMetadataKeyExtensions()
        {
#if DEBUG
            // check
            foreach(BitmapMetadataKey key in Enum.GetValues(typeof(BitmapMetadataKey)))
            {
                Debug.Assert(_groupMap.ContainsKey(key));
            }
#endif
        }

        public static BitmapMetadataGroup GetGroup(this BitmapMetadataKey key)
        {
            return _groupMap[key];
        }
    }
}


