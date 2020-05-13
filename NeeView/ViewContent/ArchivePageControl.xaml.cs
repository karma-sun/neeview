using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
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
    /// ArchivePageControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ArchivePageControl : UserControl
    {
        public static readonly RoutedCommand OpenCommand = new RoutedCommand("OpenCommand", typeof(ArchivePageControl));

        private ArchivePageViewModel _vm;
        private readonly Stopwatch _doubleTapStopwatch = new Stopwatch();
        private Point _lastTapLocation;


        public ArchivePageControl()
        {
            InitializeComponent();
        }

        public ArchivePageControl(ArchiveContent content) : this()
        {
            _vm = new ArchivePageViewModel(content);
            this.Root.DataContext = _vm;
        }


        public SolidColorBrush DefaultBrush
        {
            get { return (SolidColorBrush)GetValue(DefaultBrushProperty); }
            set { SetValue(DefaultBrushProperty, value); }
        }

        public static readonly DependencyProperty DefaultBrushProperty =
            DependencyProperty.Register("DefaultBrush", typeof(SolidColorBrush), typeof(ArchivePageControl), new PropertyMetadata(Brushes.White, DefaultBrushChanged));

        private static void DefaultBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArchivePageControl control)
            {
                control.Resources["DefaultBrush"] = e.NewValue;
            }
        }


        private void OpenBookButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void OpenBookButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice == null && e.ClickCount == 2)
            {
                _vm.OpenBook();
                e.Handled = true;
            }
        }

        private void OpenBookButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }


        // from https://stackoverflow.com/questions/27637295/double-click-touch-down-event-in-wpf
        private bool IsDoubleTap(StylusDownEventArgs e)
        {
            var points = e.GetStylusPoints(this);
            if (points.Count != 1)
            {
                return false;
            }

            Point currentTapPosition = points.First().ToPoint();
            bool isTapsAreCloseInDistance = (currentTapPosition - _lastTapLocation).LengthSquared < 40.0 * 40.0;
            _lastTapLocation = currentTapPosition;

            var elapsedMilliseconds = _doubleTapStopwatch.ElapsedMilliseconds;
            _doubleTapStopwatch.Restart();
            bool isTapsAreCloseInTime = (elapsedMilliseconds != 0 && elapsedMilliseconds < System.Windows.Forms.SystemInformation.DoubleClickTime);

            return isTapsAreCloseInDistance && isTapsAreCloseInTime;
        }

        private void OpenBookButton_PreviewStylusDown(object sender, StylusDownEventArgs e)
        {
            if (IsDoubleTap(e))
            {
                _vm.OpenBook();
                e.Handled = true;
            }
        }

        private void OpenBookButton_PreviewStylusUp(object sender, StylusEventArgs e)
        {
            e.Handled = true;
        }
    }


    /// <summary>
    /// ArchivePageControl ViewModel
    /// </summary>
    public class ArchivePageViewModel
    {
        private ArchiveContent _content;


        public ArchivePageViewModel(ArchiveContent content)
        {
            _content = content;
        }

        
        public Thumbnail Thumbnail  => _content.Thumbnail;

        public string Name => _content.Entry.EntryName?.TrimEnd('\\').Replace("\\", " > ");


        public void OpenBook()
        {
            BookHub.Current.RequestLoad(this, _content.Entry.SystemPath, null, BookLoadOption.IsBook | BookLoadOption.SkipSamePlace, true);
        }
    }


    /// <summary>
    /// 入力値に-0.5をかけた値にする。
    /// Canvasのセンタリング計算用
    /// </summary>
    public class DoubleToMinusHalf : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value * -0.5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
