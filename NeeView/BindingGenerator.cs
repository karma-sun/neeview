// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;


namespace NeeView
{
    //
    public static class BindingGenerator
    {
        private static StretchModeToBooleanConverter _stretchModeToBooleanConverter = new StretchModeToBooleanConverter();
        private static PageModeToBooleanConverter _pageModeToBooleanConverter = new PageModeToBooleanConverter();
        private static BookReadOrderToBooleanConverter _bookReadOrderToBooleanConverter = new BookReadOrderToBooleanConverter();
        private static BackgroundStyleToBooleanConverter _backgroundStyleToBooleanConverter = new BackgroundStyleToBooleanConverter();
        private static FolderOrderToBooleanConverter _folderOrderToBooleanConverter = new FolderOrderToBooleanConverter();
        private static SortModeToBooleanConverter _sortModeToBooleanConverter = new SortModeToBooleanConverter();

        //
        public static Binding Binding(string path)
        {
            return new Binding(path);
        }

        //
        public static Binding Binding(string path, BindingMode mode)
        {
            return new Binding(path) { Mode = mode };
        }

        //
        public static Binding BindingBookHub(string path)
        {
            return new Binding("BookHub." + path);
        }


        //
        public static Binding BindingBookSetting(string path)
        {
            return new Binding("BookSetting." + path);
        }


        //
        public static Binding StretchMode(PageStretchMode mode)
        {
            return new Binding("StretchMode")
            {
                Converter = _stretchModeToBooleanConverter,
                ConverterParameter = mode.ToString()
            };
        }

        //
        public static Binding Background(BackgroundStyle mode)
        {
            return new Binding("Background")
            {
                Converter = _backgroundStyleToBooleanConverter,
                ConverterParameter = mode.ToString()
            };
        }


        //
        public static Binding FolderOrder(FolderOrder mode)
        {
            return new Binding("FolderCollection.Folder.FolderOrder")
            {
                Source = (App.Current.MainWindow as MainWindow).FolderList.DockPanel.DataContext, // 強引だな..
                Converter = _folderOrderToBooleanConverter,
                ConverterParameter = mode.ToString()
            };
        }


        //
        public static Binding PageMode(PageMode mode)
        {
            return new Binding("BookSetting.PageMode")
            {
                Converter = _pageModeToBooleanConverter,
                ConverterParameter = mode.ToString(),
            };
        }

        //
        public static Binding BookReadOrder(PageReadOrder mode)
        {
            return new Binding("BookSetting.BookReadOrder")
            {
                Converter = _bookReadOrderToBooleanConverter,
                ConverterParameter = mode.ToString(),
            };
        }

        //
        public static Binding SortMode(PageSortMode mode)
        {
            return new Binding("BookSetting.SortMode")
            {
                Converter = _sortModeToBooleanConverter,
                ConverterParameter = mode.ToString(),
            };
        }


        //
        public static Binding IsBookmark()
        {
            return new Binding("IsBookmark")
            {
                Mode = BindingMode.OneWay
            };
        }

        //
        public static Binding IsPagemark()
        {
            return new Binding("IsPagemark")
            {
                Mode = BindingMode.OneWay
            };
        }

        //
        public static Binding IsFlipHorizontal()
        {
            return new Binding("IsFlipHorizontal")
            {
                Source = (App.Current.MainWindow as MainWindow).MouseDragController, // 強引だな..
                Mode = BindingMode.OneWay
            };
        }

        //
        public static Binding IsFlipVertical()
        {
            return new Binding("IsFlipVertical")
            {
                Source = (App.Current.MainWindow as MainWindow).MouseDragController, // 強引だな..
                Mode = BindingMode.OneWay
            };
        }
    }
}
