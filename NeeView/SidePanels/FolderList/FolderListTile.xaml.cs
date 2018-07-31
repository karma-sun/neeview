using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// FolderListTile.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListTile : UserControl
    {
        public FolderListTile()
        {
            InitializeComponent();
        }
    }

    [ValueConversion(typeof(IThumbnail), typeof(Thickness))]
    public class ThumbnailToTileMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ConstThumbnail thumbnail)
            {
                var width = ThumbnailProfile.Current.TileWidth;
                if (width > 64)
                {
                    var margin = (width - 64) * 0.25;
                    return new Thickness(margin);
                }
            }

            return new Thickness();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}
