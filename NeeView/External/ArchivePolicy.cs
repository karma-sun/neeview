using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    // 圧縮ファイルの時の動作
    public enum ArchivePolicy
    {
        [AliasName]
        None,

        [AliasName]
        SendArchiveFile,

        [AliasName]
        SendArchivePath, // ver 33.0

        [AliasName]
        SendExtractFile,
    }


    public static class ArchivePolicyExtensions
    {
        public static string ToSampleText(this ArchivePolicy self)
        {
            switch (self)
            {
                case ArchivePolicy.None:
                    return @"not run.";
                case ArchivePolicy.SendArchiveFile:
                    return @"C:\Archive.zip";
                case ArchivePolicy.SendArchivePath:
                    return @"C:\Archive.zip\File.jpg";
                case ArchivePolicy.SendExtractFile:
                    return @"ExtractToTempFolder\File.jpg";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }


    public class ArchivePolicyToSampleStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ArchivePolicy policy)
            {
                return Properties.Resources.Word_Example + ", " + policy.ToSampleText();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ArchivePolicyToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ArchivePolicy policy)
            {
                return policy.ToAliasName();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
