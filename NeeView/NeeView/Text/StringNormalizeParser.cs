using System.Collections;
using System.Collections.Generic;

namespace NeeView.Text
{
    /// <summary>
    /// 正規化した文字を返すEnumerator
    /// </summary>
    public class StringNormalizeParser : IEnumerable<char>
    {
        private string _source;

        public StringNormalizeParser(string source)
        {
            _source = source;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<char> GetEnumerator()
        {
            if (string.IsNullOrEmpty(_source)) yield break;

            char n0 = '\0';

            foreach (var c in _source)
            {
                var n1 = ToNormalizedChar(c);

                if (n0 == '\0')
                {
                    n0 = n1;
                    continue;
                }
                else
                {
                    if (KanaEmbedded.IsDakuten(n1))
                    {
                        var n = KanaEmbedded.ToDakutenChar(n0);
                        if (n != n0)
                        {
                            n0 = n;
                            continue;
                        }
                    }
                    else if (KanaEmbedded.IsHandakuten(n1))
                    {
                        var n = KanaEmbedded.ToHandakutenChar(n0);
                        if (n != n0)
                        {
                            n0 = n;
                            continue;
                        }
                    }

                    yield return n0;
                    n0 = n1;
                }
            }

            if (n0 != '\0')
            {
                yield return n0;
                n0 = '\0';
            }
        }

        private static char ToNormalizedChar(char src)
        {
            var c = src;
            c = KanaEmbedded.ToZenkakuKanaChar(c);
            c = KanaEmbedded.ToKatakanaChar(c);
            c = KanaEmbedded.ToHankakuChar(c);
            c = CharExtensions.ToUpperFast(c);
            return c;
        }
    }

}
