﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
        private static StretchModeToBooleanConverter s_stretchModeToBooleanConverter = new StretchModeToBooleanConverter();
        private static PageModeToBooleanConverter s_pageModeToBooleanConverter = new PageModeToBooleanConverter();
        private static BookReadOrderToBooleanConverter s_bookReadOrderToBooleanConverter = new BookReadOrderToBooleanConverter();
        private static BackgroundStyleToBooleanConverter s_backgroundStyleToBooleanConverter = new BackgroundStyleToBooleanConverter();
        private static FolderOrderToBooleanConverter s_folderOrderToBooleanConverter = new FolderOrderToBooleanConverter();
        private static SortModeToBooleanConverter s_sortModeToBooleanConverter = new SortModeToBooleanConverter();

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
        public static Binding BindingWithSource(string path, object source)
        {
            return new Binding(path) { Source = source };
        }


        //
        public static Binding BindingPreference(string path)
        {
            return new Binding(path) { Source = PreferenceAccessor.Current };
        }

        //
        public static Binding BindingBookHub(string path)
        {
            return new Binding(nameof(MainWindowVM.BookHub) + "." + path);
        }

        //
        public static Binding BindingBookSetting(string path)
        {
            return new Binding(nameof(MainWindowVM.BookSetting) + "." + path);
        }


        //
        public static Binding StretchMode(PageStretchMode mode)
        {
            return new Binding(nameof(MainWindowVM.StretchMode))
            {
                Converter = s_stretchModeToBooleanConverter,
                ConverterParameter = mode.ToString()
            };
        }

        //
        public static Binding Background(BackgroundStyle mode)
        {
            return new Binding(nameof(MainWindowVM.Background))
            {
                Converter = s_backgroundStyleToBooleanConverter,
                ConverterParameter = mode.ToString()
            };
        }


        //
        public static Binding FolderOrder(FolderOrder mode)
        {
            // TODO: 強引すぎー
            return new Binding("FolderListPanel.FolderList.DockPanel.DataContext.FolderCollection.Folder.FolderOrder")
            {
                Converter = s_folderOrderToBooleanConverter,
                ConverterParameter = mode.ToString()
            };

#if false
            return new Binding("FolderCollection.Folder.FolderOrder")
            {
                Source = (App.Current.MainWindow as MainWindow).FolderList.DockPanel.DataContext, // 強引だな..
                Converter = s_folderOrderToBooleanConverter,
                ConverterParameter = mode.ToString()
            };
#endif
        }


        //
        public static Binding PageMode(PageMode mode)
        {
            return new Binding("BookSetting.PageMode")
            {
                Converter = s_pageModeToBooleanConverter,
                ConverterParameter = mode.ToString(),
            };
        }

        //
        public static Binding BookReadOrder(PageReadOrder mode)
        {
            return new Binding("BookSetting.BookReadOrder")
            {
                Converter = s_bookReadOrderToBooleanConverter,
                ConverterParameter = mode.ToString(),
            };
        }

        //
        public static Binding SortMode(PageSortMode mode)
        {
            return new Binding("BookSetting.SortMode")
            {
                Converter = s_sortModeToBooleanConverter,
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
                Source = MouseInputManager.Current.Drag,
                Mode = BindingMode.OneWay
            };
        }

        //
        public static Binding IsFlipVertical()
        {
            return new Binding("IsFlipVertical")
            {
                Source = MouseInputManager.Current.Drag,
                Mode = BindingMode.OneWay
            };
        }
    }
}
