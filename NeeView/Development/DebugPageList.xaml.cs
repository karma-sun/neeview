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
    /// DebugPageList.xaml の相互作用ロジック
    /// </summary>
    public partial class DebugPageList : UserControl
    {
        public DebugPageList()
        {
            InitializeComponent();
            this.Root.DataContext = new DevPageListViewModel();

            BookOperation.Current.ViewContentsChanged += BookOperation_ViewContentsChanged;
        }

        private void BookOperation_ViewContentsChanged(object sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            ////var page = BookOperation.Current.Book?.GetViewPage();
            ////this.Root.ScrollIntoView(page);
            ///
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.PageListView.Items.Refresh();

            long totalMemory = GC.GetTotalMemory(true);
            long workingSet = System.Environment.WorkingSet;
            Debug.WriteLine($"WorkingSet: {totalMemory:#,0}");
            Debug.WriteLine($"WorkingSet: {workingSet:#,0}");
        }
    }

    public class DevPageListViewModel
    {
        public BookOperation BookOperation => BookOperation.Current;

        public DevPageListViewModel()
        {
        }
    }

    public class PageContentToPictureSourceMemoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BitmapContent content)
            {
                return string.Format("{0:#,0}", content.PictureSource?.GetMemorySize());
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PageContentToPictureMemoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BitmapContent content)
            {
                return string.Format("{0:#,0}", content.Picture?.GetMemorySize());
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
