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
        Orientation,
        EXIFVersion,

        // -- GPS
        GPSLatitude,
        GPSLongitude,
        GPSAltitude,
    }

}


