using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// フォルダーアイコン表示方法
    /// </summary>
    public enum FolderIconLayout
    {
        [AliasName("@EnumFolderIconLayoutDefault")]
        Default,

        [AliasName("@EnumFolderIconLayoutExplorer")]
        Explorer,
    }

    /// <summary>
    /// フォルダーアイコン表示方法をBooleanに変換
    /// </summary>
    public class FolderIconLayoutToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is FolderIconLayout v0))
                return false;

            return v0 == FolderIconLayout.Explorer;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool v0))
                return FolderIconLayout.Default;

            return v0 ? FolderIconLayout.Explorer : FolderIconLayout.Default;
        }
    }

    /// <summary>
    /// フォルダーアイコン表示方法をVisibilityに変換
    /// </summary>
    public class FolderIconLayoutToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is FolderIconLayout v0))
                return Visibility.Collapsed;

            if (!(parameter is FolderIconLayout v1))
                return Visibility.Collapsed;

            return v0 == v1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
