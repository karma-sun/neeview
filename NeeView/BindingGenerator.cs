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
        private static StretchModeToBooleanConverter s_stretchModeToBooleanConverter = new StretchModeToBooleanConverter();
        private static PageModeToBooleanConverter s_pageModeToBooleanConverter = new PageModeToBooleanConverter();
        private static BookReadOrderToBooleanConverter s_bookReadOrderToBooleanConverter = new BookReadOrderToBooleanConverter();
        private static BackgroundStyleToBooleanConverter s_backgroundStyleToBooleanConverter = new BackgroundStyleToBooleanConverter();
        private static FolderOrderToBooleanConverter s_folderOrderToBooleanConverter = new FolderOrderToBooleanConverter();
        private static SortModeToBooleanConverter s_sortModeToBooleanConverter = new SortModeToBooleanConverter();
        private static AnytToFalseConverter _anyToFalseConverter = new AnytToFalseConverter();


        //
        public static Binding BindingPreference(string path)
        {
            return new Binding(path) { Source = PreferenceAccessor.Current };
        }

        //
        public static Binding BindingBookHub(string path)
        {
            return new Binding(path) { Source = BookHub.Current }; //  nameof(MainWindowVM.BookHub) + "." + path);
        }

        //
        public static Binding BindingBookSetting(string path)
        {
            return new Binding(nameof(BookHub.BookMemento) + "." + path) { Source = BookHub.Current };
        }



        //
        public static Binding StretchMode(PageStretchMode mode)
        {
            return new Binding(nameof(ContentCanvas.StretchMode))
            {
                Converter = s_stretchModeToBooleanConverter,
                ConverterParameter = mode.ToString(),
                Source = ContentCanvas.Current
            };
        }

        //
        public static Binding Background(BackgroundStyle mode)
        {
            return new Binding(nameof(ContentCanvasBrush.Background))
            {
                Converter = s_backgroundStyleToBooleanConverter,
                ConverterParameter = mode.ToString(),
                Source = ContentCanvasBrush.Current
            };
        }


        //
        public static Binding FolderOrder(FolderOrder mode)
        {
            // TODO: 現状機能していない。FolderListから取得できるようにする
            return new Binding(nameof(FolderList.FolderOrder))
            {
                ////Converter = s_folderOrderToBooleanConverter,
                Converter = _anyToFalseConverter,
                ConverterParameter = mode.ToString(),
                Mode = BindingMode.OneWay,
                Source = Models.Current.FolderList
            };
        }


        //
        public static Binding PageMode(PageMode mode)
        {
            var binding = BindingBookSetting(nameof(Book.Memento.PageMode));
            binding.Converter = s_pageModeToBooleanConverter;
            binding.ConverterParameter = mode.ToString();
            return binding;
        }

        //
        public static Binding BookReadOrder(PageReadOrder mode)
        {
            var binding = BindingBookSetting(nameof(Book.Memento.BookReadOrder));
            binding.Converter = s_bookReadOrderToBooleanConverter;
            binding.ConverterParameter = mode.ToString();
            return binding;
        }

        //
        public static Binding SortMode(PageSortMode mode)
        {
            var binding = BindingBookSetting(nameof(Book.Memento.SortMode));
            binding.Converter = s_sortModeToBooleanConverter;
            binding.ConverterParameter = mode.ToString();
            return binding;
        }


        //
        public static Binding IsFlipHorizontal()
        {
            return new Binding("IsFlipHorizontal")
            {
                Source = MouseInput.Current.Drag,
                Mode = BindingMode.OneWay
            };
        }

        //
        public static Binding IsFlipVertical()
        {
            return new Binding("IsFlipVertical")
            {
                Source = MouseInput.Current.Drag,
                Mode = BindingMode.OneWay
            };
        }
    }
}
