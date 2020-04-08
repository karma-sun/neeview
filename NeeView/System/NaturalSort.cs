using System.Collections;
using System.Collections.Generic;

namespace NeeView
{
    public static class NaturalSort
    {
        private static IComparer<string> _nativeComparer;
        private static IComparer<string> _customComparer;

        static NaturalSort()
        {
            _nativeComparer = new NativeNaturalComparer();
            _customComparer = new CustomNaturalComparer();
        }

        public static IComparer<string> Comparer => Config.Current.System.IsNaturalSortEnabled ? _customComparer : _nativeComparer;

        public static int Compare(string x, string y)
        {
            return Comparer.Compare(x, y);
        }
    }

    public class NativeNaturalComparer : IComparer<string>, IComparer
    {
        public int Compare(string x, string y)
        {
            return NativeMethods.StrCmpLogicalW(x, y);
        }

        public int Compare(object x, object y)
        {
            return Compare(x as string, y as string);
        }
    }

    public class CustomNaturalComparer : IComparer<string>, IComparer
    {
        public int Compare(string x, string y)
        {
            // TODO:
            return NativeMethods.StrCmpLogicalW(x, y);
        }

        public int Compare(object x, object y)
        {
            return Compare(x as string, y as string);
        }
    }

}
