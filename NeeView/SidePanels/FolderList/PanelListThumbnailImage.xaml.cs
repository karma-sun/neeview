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
    /// PanelListThumbnailImage.xaml の相互作用ロジック
    /// </summary>
    public partial class PanelListThumbnailImage : UserControl
    {
        public PanelListThumbnailImage()
        {
            InitializeComponent();
            this.Root.DataContext = this;
        }


        public IThumbnail Thumbnail
        {
            get { return (IThumbnail)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        public static readonly DependencyProperty ThumbnailProperty =
            DependencyProperty.Register("Thumbnail", typeof(IThumbnail), typeof(PanelListThumbnailImage), new PropertyMetadata(null));

    }

    /// <summary>
    /// 画像がベクターである場合、余白を付加する
    /// </summary>
    public class ThumbnailToMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is ConstThumbnail thumbnail && thumbnail.BitmapSource is DrawingImage && values[1] is int width)
            {
                if (width > 64)
                {
                    var margin = (width - 64) * 0.25;
                    return new Thickness(margin);
                }
            }

            return new Thickness();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 画像がベクターである場合、ストレッチはUniform限定
    /// </summary>
    public class ThumbnailToStretchConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[1] is Stretch stretch)
            {
                if (values[0] is ConstThumbnail thumbnail && thumbnail.BitmapSource is DrawingImage)
                {
                    return Stretch.Uniform;
                }

                return stretch;
            }

            return Stretch.Uniform;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
