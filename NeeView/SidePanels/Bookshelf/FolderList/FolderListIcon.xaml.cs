using System;
using System.Collections.Generic;
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
    /// FolderListIcon.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListIcon : UserControl
    {

        public bool IsKeepArea
        {
            get { return (bool)GetValue(IsKeepAreaProperty); }
            set { SetValue(IsKeepAreaProperty, value); }
        }

        public static readonly DependencyProperty IsKeepAreaProperty =
            DependencyProperty.Register("IsKeepArea", typeof(bool), typeof(FolderListIcon), new PropertyMetadata(false, IsKeepAreaPropertyChanged));

        private static void IsKeepAreaPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FolderListIcon control)
            {
                control.Spacer.Visibility = control.IsKeepArea ? Visibility.Visible : Visibility.Collapsed;
            }
        }




        /// <summary>
        /// constructor
        /// </summary>
        public FolderListIcon()
        {
            InitializeComponent();
        }

    }

    [ValueConversion(typeof(FolderItemIconOverlay), typeof(Visibility))]
    public class FolderItemIconOverlayToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FolderItemIconOverlay overlay)
            {
                if (overlay != FolderItemIconOverlay.None && overlay != FolderItemIconOverlay.Uninitialized)
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(FolderItemIconOverlay), typeof(ImageSource))]
    public class FolderItemIconOverlayToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FolderItemIconOverlay overlay)
            {
                switch (overlay)
                {
                    case FolderItemIconOverlay.Checked:
                        return MainWindow.Current.Resources["ic_done_24px"];
                    case FolderItemIconOverlay.Star:
                        return MainWindow.Current.Resources["ic_grade_24px"];
                    case FolderItemIconOverlay.Disable:
                        return App.Current.Resources["ic_clear_24px"];
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
