using System;
using System.Diagnostics;
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
    /// MenuBar : View
    /// </summary>
    public partial class MenuBarView : UserControl
    {
        #region Fields

        private MenuBarViewModel _vm;

        #endregion

        #region Constructors

        public MenuBarView()
        {
            InitializeComponent();

            if (Config.Current.IsCanaryPackage)
            {
                this.Watermark.Visibility = Visibility.Visible;
                this.Watermark.Background = Brushes.Orange;
                this.WatermarkText.Text = "Canary";
            }
            else if (Config.Current.IsBetaPackage)
            {
                this.Watermark.Visibility = Visibility.Visible;
                this.Watermark.Background = Brushes.Purple;
                this.WatermarkText.Text = "Beta";
            }
        }

        #endregion

        #region DependencyProperties

        public MenuBar Source
        {
            get { return (MenuBar)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(MenuBar), typeof(MenuBarView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MenuBarView)?.Initialize();
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            _vm = new MenuBarViewModel(this, this.Source);
            this.Root.DataContext = _vm;
        }

        // 単キーのショートカット無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }

        #endregion
    }


    public class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue)
            {
                return isTrue ? 1.0 : 0.5;
            }
            else
            {
                return 0.5f;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
