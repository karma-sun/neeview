using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public enum FolderTreeLayout
    {
        [AliasName("@FolderTreeLayoutTop")]
        Top,
        [AliasName("@FolderTreeLayoutLeft")]
        Left,
    };

    public class FolderTreeLayoutToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FolderTreeLayout layout0 && parameter is FolderTreeLayout layout1)
            {
                return (layout0 == layout1);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Boolean flag && parameter is FolderTreeLayout layout1)
            {
                return flag ? layout1 : FolderTreeLayout.Top;
            }

            return FolderTreeLayout.Top;
        }
    }

}
