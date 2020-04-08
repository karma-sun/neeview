using CSharp.Japanese.Kanaxs;
using System;
using System.Collections;
using System.Collections.Generic;
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
            _customComparer = new CustomNaturalComparer();
        }


        public static IComparer<string> Comparer => Config.Current.System.IsNaturalSortEnabled ? _customComparer : _nativeComparer;


        public static int Compare(string x, string y)
        {
            return Comparer.Compare(x, y);
        }
    }

    /// <summary>
    /// Win32API での自然ソート
    /// </summary>
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

    /// <summary>
    /// NeeViewカスタムの自然ソート
    /// </summary>
    public class CustomNaturalComparer : IComparer<string>, IComparer
    {
        private static Regex _regexNum = new Regex(@"^[0-9]+(\.[0-9]+)?", RegexOptions.Compiled);
        private static string _kanjiOrderReverseList = "後下中前上萬阡陌拾玖捌漆陸伍肆参弐壱零万千百十九八七六五四三二一〇";

        public int Compare(object x, object y)
        {
            return Compare(x as string, y as string);
        }

        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            var defaultCompareValue = x.CompareTo(y);
            if (defaultCompareValue == 0) return 0;

            var nx = Normalize(x);
            var ny = Normalize(y);

            var length = Math.Min(nx.Length, ny.Length);

            for (int i = 0; i < length; ++i)
            {
                var cx = nx[i];
                var cy = ny[i];

                // 数値比較
                if (char.IsNumber(cx) && char.IsNumber(cy))
                {
                    var dsx = _regexNum.Match(nx.Substring(i)).Value;
                    var dsy = _regexNum.Match(ny.Substring(i)).Value;
                    var dnx = double.Parse(dsx);
                    var dny = double.Parse(dsy);
                    var numberCompare = dnx.CompareTo(dny);
                    if (numberCompare != 0) return numberCompare;

                    i += dsx.Length - 1;
                    continue;
                }

                if (cx == cy) continue;

                // 漢数字等の簡易比較
                if (IsKanji(cx) && IsKanji(cy))
                {
                    var mx = _kanjiOrderReverseList.IndexOf(cx);
                    var my = _kanjiOrderReverseList.IndexOf(cy);

                    if (mx != my)
                    {
                        // NOTE: _kanjiOrderReverseList は逆順のため、比較を反転
                        return my - mx;
                    }
                }

                return cx.CompareTo(cy);
            }

            if (nx.Length == ny.Length)
            {
                return defaultCompareValue;
            }
            else
            {
                return nx.Length - ny.Length;
            }
        }


        private bool IsKanji(char c)
        {
            //CJK統合漢字、CJK互換漢字、CJK統合漢字拡張Aの範囲にあるか調べる
            return ('\u4E00' <= c && c <= '\u9FCF') || ('\uF900' <= c && c <= '\uFAFF') || ('\u3400' <= c && c <= '\u4DBF');
        }

        private string Normalize(string src)
        {
            string s = src;

            // 濁点を１文字にまとめる
            s = KanaEx.ToPadding(s);

            try
            {
                // 正規化
                s = s.Normalize(NormalizationForm.FormKC);
            }
            catch (ArgumentException)
            {
                // 無効なコードポイントがある場合は正規化はスキップする
            }

            // アルファベットを大文字にする
            s = s.ToUpper();

            // ひらがなをカタカナにする ＋ 特定文字の正規化
            s = KanaEx.ToKatakanaWithNormalize(s);

            return s;
        }
    }
}
