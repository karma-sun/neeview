using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;


namespace NeeView
{
    public static class BindingGenerator
    {
        private static readonly StretchModeToBooleanConverter _stretchModeToBooleanConverter = new StretchModeToBooleanConverter();
        private static readonly PageModeToBooleanConverter _pageModeToBooleanConverter = new PageModeToBooleanConverter();
        private static readonly BookReadOrderToBooleanConverter _bookReadOrderToBooleanConverter = new BookReadOrderToBooleanConverter();
        private static readonly BackgroundStyleToBooleanConverter _backgroundStyleToBooleanConverter = new BackgroundStyleToBooleanConverter();
        private static readonly FolderOrderToBooleanConverter _folderOrderToBooleanConverter = new FolderOrderToBooleanConverter();
        private static readonly SortModeToBooleanConverter _sortModeToBooleanConverter = new SortModeToBooleanConverter();
        private static readonly AnytToFalseConverter _anyToFalseConverter = new AnytToFalseConverter();

        public static Binding BindingBookHub(string path)
        {
            return new Binding(path) { Source = BookHub.Current };
        }

        public static Binding BindingBookSetting(string path)
        {
            return new Binding(nameof(BookSettingPresenter.LatestSetting) + "." + path) { Source = BookSettingPresenter.Current };
        }

        public static Binding StretchMode(PageStretchMode mode)
        {
            return new Binding(nameof(ViewConfig.StretchMode))
            {
                Converter = _stretchModeToBooleanConverter,
                ConverterParameter = mode.ToString(),
                Source = Config.Current.View
            };
        }

        public static Binding Background(BackgroundType mode)
        {
            return new Binding(nameof(BackgroundConfig.BackgroundType))
            {
                Converter = _backgroundStyleToBooleanConverter,
                ConverterParameter = mode.ToString(),
                Source = Config.Current.Layout.Background
            };
        }

        public static Binding FolderOrder(FolderOrder mode)
        {
            return new Binding(nameof(BookshelfFolderList.FolderOrder))
            {
                Converter = _folderOrderToBooleanConverter,
                ConverterParameter = mode.ToString(),
                Mode = BindingMode.OneWay,
                Source = BookshelfFolderList.Current
            };
        }

        public static Binding PageMode(PageMode mode)
        {
            var binding = BindingBookSetting(nameof(Book.Memento.PageMode));
            binding.Converter = _pageModeToBooleanConverter;
            binding.ConverterParameter = mode.ToString();
            return binding;
        }

        public static Binding BookReadOrder(PageReadOrder mode)
        {
            var binding = BindingBookSetting(nameof(Book.Memento.BookReadOrder));
            binding.Converter = _bookReadOrderToBooleanConverter;
            binding.ConverterParameter = mode.ToString();
            return binding;
        }

        public static Binding SortMode(PageSortMode mode)
        {
            var binding = BindingBookSetting(nameof(Book.Memento.SortMode));
            binding.Converter = _sortModeToBooleanConverter;
            binding.ConverterParameter = mode.ToString();
            return binding;
        }
    }
}
