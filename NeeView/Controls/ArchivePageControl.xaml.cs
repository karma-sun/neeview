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
        #region Fields

        public ArchivePageContent _content;

        #endregion

        #region Construtors

        public ArchivePageControl()
        {
            InitializeComponent();
        }

        #endregion

        #region RoutedCommand

        public static readonly RoutedCommand OpenCommand = new RoutedCommand("OpenCommand", typeof(ArchivePageControl));

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty DefaultBrushProperty =
            DependencyProperty.Register(
            "DefaultBrush",
            typeof(Brush),
            typeof(ArchivePageControl),
            new FrameworkPropertyMetadata(Brushes.White));

        public Brush DefaultBrush
        {
            get { return (Brush)GetValue(DefaultBrushProperty); }
            set { SetValue(DefaultBrushProperty, value); }
        }

        #endregion

        #region Methods

        public ArchivePageControl(ArchivePageContent content) : this()
        {
            _content = content;

            this.OpenBookButton.CommandBindings.Add(new CommandBinding(OpenCommand, Open_Executed));

            this.Icon.DataContext = content.Thumbnail;
            this.FileNameTextBlock.Text = content.Entry.EntryName?.TrimEnd('\\').Replace("\\", " > ");
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenBook();
        }

        private void OpenBookButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void OpenBookButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenBook();
            e.Handled = true;
        }

        private void OpenBookButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement)?.Focus();
            e.Handled = true;
        }

        private void OpenBookButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.OpenBookButton.ContextMenu.PlacementTarget = sender as UIElement;
            this.OpenBookButton.ContextMenu.IsOpen = true;
            e.Handled = true;
        }

        private void OpenBook()
        {
            var option = BookLoadOption.IsBook | BookLoadOption.SkipSamePlace;
            BookHub.Current.RequestLoad(_content.Entry.SystemPath, null, option, true);
        }

        #endregion
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
