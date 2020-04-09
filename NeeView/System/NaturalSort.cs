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
        private static Regex _regexNum = new Regex(@"^[0-9]+(\.[0-9]+)*", RegexOptions.Compiled);
        private static string _kanjiOrderReverseList = "後下中前上萬万仟阡千佰陌百什拾十玖九捌八漆七陸六伍五肆四参三弐二壱一零〇";

        public int Compare(object x, object y)
        {
            return Compare(x as string, y as string);
        }

        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            var defaultCompareValue = x.CompareTo(y);
            if (defaultCompareValue == 0)
            {
                return 0;
            }

            var normalX = Normalize(x);
            var normalY = Normalize(y);

            var length = Math.Min(normalX.Length, normalY.Length);
            for (int i = 0; i < length; ++i)
            {
                var cx = normalX[i];
                var cy = normalY[i];

                // 数値比較
                // ピリオドで区切られた数値をその単位で比較。桁数の揃っていない小数では逆順になるが少数判断できないため許容する
                if (IsDigit(cx) && IsDigit(cy))
                {
                    var numbersX = _regexNum.Match(normalX.Substring(i)).Value;
                    var numbersY = _regexNum.Match(normalY.Substring(i)).Value;

                    var numberTokensX = numbersX.Split('.');
                    var numberTokensY = numbersY.Split('.');

                    for (int n = 0; n < numberTokensX.Length && n < numberTokensY.Length; ++n)
                    {
                        var numX = int.Parse(numberTokensX[n]);
                        var numY = int.Parse(numberTokensY[n]);
                        if (numX != numY)
                        {
                            return numX - numY;
                        }
                    }

                    if (numberTokensX.Length != numberTokensY.Length)
                    {
                        return numberTokensX.Length - numberTokensY.Length;
                    }

                    if (numbersX.Length != numbersY.Length)
                    {
                        return numbersX.Length - numbersY.Length;
                    }

                    i += numbersX.Length - 1;
                    continue;
                }

                if (cx == cy) continue;

                // 漢数字等の簡易比較
                // テーブルに登録されている漢字の序列を優先する
                if (IsKanji(cx) && IsKanji(cy))
                {
                    var indexX = _kanjiOrderReverseList.IndexOf(cx);
                    var indexY = _kanjiOrderReverseList.IndexOf(cy);

                    if (indexX != indexY)
                    {
                        // NOTE: _kanjiOrderReverseList は逆順のため、比較を反転
                        return indexY - indexX;
                    }
                }

                return cx.CompareTo(cy);
            }

            if (normalX.Length == normalY.Length)
            {
                return defaultCompareValue;
            }
            else
            {
                return normalX.Length - normalY.Length;
            }
        }

        private bool IsDigit(char c)
        {
            return ('\u0030' <= c && c <= '\u0039');
        }

        // NOTE: サロゲートコード(CJK統合漢字拡張B)には対応していません
        private bool IsKanji(char c)
        {
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
