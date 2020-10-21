using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView.Text
{
    /// <summary>
    /// 自然ソート
    /// </summary>
    public class NaturalComparer : IComparer<string>, IComparer
    {
        // 漢数字のソート順(逆順)
        private static Dictionary<char, int> _kanjiOrderMap;


        static NaturalComparer()
        {
            var kanjiOrder = "〇零一壱二弐三参四肆五伍六陸七漆八捌九玖十拾什上前中下後";
            _kanjiOrderMap = kanjiOrder.Select((e, index) => (e, index)).ToDictionary(e => e.e, e => e.index);
        }


        public int Compare(object x, object y)
        {
            return Compare(x as string, y as string);
        }

        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            if (object.ReferenceEquals(x, y)) return 0;

            var parserX = new StringTokenParser(x);
            var parserY = new StringTokenParser(y);

            var ix = parserX.GetEnumerator();
            var iy = parserY.GetEnumerator();

            var xresult = ix.MoveNext();
            var yresult = iy.MoveNext();

            while (xresult && yresult)
            {
                var tx = ix.Current;
                var ty = iy.Current;

                if (tx.IsNumber() && ty.IsNumber())
                {
                    var cval = tx.CompareTo(ty);
                    if (cval != 0)
                    {
                        return cval;
                    }
                }
                else
                {
                    var cx = tx.FirstChar;
                    var cy = ty.FirstChar;

                    if (cx != cy)
                    {
                        // 漢数字等を踏まえた漢字の簡易比較
                        if (KanaEmbedded.IsKanji(cx) && KanaEmbedded.IsKanji(cy))
                        {
                            var kx = _kanjiOrderMap.TryGetValue(cx, out var indexX) ? (char)('あ' + indexX) : cx;
                            var ky = _kanjiOrderMap.TryGetValue(cy, out var indexY) ? (char)('あ' + indexY) : cy;
                            return string.Compare(kx.ToString(), ky.ToString());
                        }

                        return cx - cy;
                    }

                    // 数字文字で等しいというのはありえない
                    Debug.Assert(!char.IsDigit(cx));
                }

                xresult = ix.MoveNext();
                yresult = iy.MoveNext();
            }

            if (xresult) return 1;
            if (yresult) return -1;

            Debug.Assert(xresult == false && yresult == false);
            return string.Compare(x, y);
        }
    }

}
