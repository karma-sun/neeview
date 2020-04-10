using NeeView.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace NeeView
{
    /// <summary>
    /// 自然順ソート
    /// </summary>
    public static class NaturalSort
    {
        private static IComparer<string> _nativeComparer;
        private static IComparer<string> _customComparer;

        static NaturalSort()
        {
            _nativeComparer = new NativeNaturalComparer();
            _customComparer = new NaturalComparer();
        }


        public static IComparer<string> Comparer => Config.Current.System.IsNaturalSortEnabled ? _customComparer : _nativeComparer;


        public static int Compare(string x, string y)
        {
            return Comparer.Compare(x, y);
        }
    }

}
