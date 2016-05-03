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
        static StretchModeToBooleanConverter _StretchModeToBooleanConverter = new StretchModeToBooleanConverter();
        static PageModeToBooleanConverter _PageModeToBooleanConverter = new PageModeToBooleanConverter();
        static BookReadOrderToBooleanConverter _BookReadOrderToBooleanConverter = new BookReadOrderToBooleanConverter();

        //
        public static Binding StretchMode(PageStretchMode mode)
        {
            return new Binding("StretchMode")
            {
                Converter = _StretchModeToBooleanConverter,
                ConverterParameter = mode.ToString()
            };
        }

        //
        public static Binding PageMode(PageMode mode)
        {
            return new Binding("BookSetting.PageMode")
            {
                Converter = _PageModeToBooleanConverter,
                ConverterParameter = mode.ToString(),
            };
        }

        //
        public static Binding BookReadOrder(PageReadOrder mode)
        {
            return new Binding("BookSetting.BookReadOrder")
            {
                Converter = _BookReadOrderToBooleanConverter,
                ConverterParameter = mode.ToString(),
            };
        }

        public static Binding IsSupportedWidePage()
        {
            return new Binding("BookSetting.IsSupportedWidePage");
        }
    }
}
