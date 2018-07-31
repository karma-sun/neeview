using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// フォルダーリストの表示方法
    /// </summary>
    public enum PanelListItemStyle
    {
        Normal, // テキストのみ
        Content, // コンテンツ
        Banner, // バナー
        Tile, // タイル
    };

    public static class PanelListItemStyleExtensions
    {
        public static bool HasThumbnail(this PanelListItemStyle my)
        {
            return (my == PanelListItemStyle.Content || my == PanelListItemStyle.Banner);
        }
    }

    //
    public class PanelListItemStyleToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is PanelListItemStyle v0))
                return false;

            if (!(parameter is PanelListItemStyle v1))
                return false;

            return v0 == v1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}
