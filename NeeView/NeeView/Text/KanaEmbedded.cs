using System.Collections.Generic;

namespace NeeView.Text
{
    /// <summary>
    /// 自然順ソートに必要な文字正規化処理をカスタムしたもの
    /// KanaEx をベースに構築。char型の変換のみ
    /// </summary>
    public static class KanaEmbedded
    {
        private static Dictionary<char, char> _hankakuZenkakuKanaMap = new Dictionary<char, char>()
        {
            ['｡'] = '。',
            ['｢'] = '「',
            ['｣'] = '」',
            ['､'] = '、',
            ['･'] = '・',
            ['ｦ'] = 'ヲ',
            ['ｧ'] = 'ァ',
            ['ｨ'] = 'ィ',
            ['ｩ'] = 'ゥ',
            ['ｪ'] = 'ェ',
            ['ｫ'] = 'ォ',
            ['ｬ'] = 'ャ',
            ['ｭ'] = 'ュ',
            ['ｮ'] = 'ョ',
            ['ｯ'] = 'ッ',
            ['ｰ'] = 'ー',
            ['ｱ'] = 'ア',
            ['ｲ'] = 'イ',
            ['ｳ'] = 'ウ',
            ['ｴ'] = 'エ',
            ['ｵ'] = 'オ',
            ['ｶ'] = 'カ',
            ['ｷ'] = 'キ',
            ['ｸ'] = 'ク',
            ['ｹ'] = 'ケ',
            ['ｺ'] = 'コ',
            ['ｻ'] = 'サ',
            ['ｼ'] = 'シ',
            ['ｽ'] = 'ス',
            ['ｾ'] = 'セ',
            ['ｿ'] = 'ソ',
            ['ﾀ'] = 'タ',
            ['ﾁ'] = 'チ',
            ['ﾂ'] = 'ツ',
            ['ﾃ'] = 'テ',
            ['ﾄ'] = 'ト',
            ['ﾅ'] = 'ナ',
            ['ﾆ'] = 'ニ',
            ['ﾇ'] = 'ヌ',
            ['ﾈ'] = 'ネ',
            ['ﾉ'] = 'ノ',
            ['ﾊ'] = 'ハ',
            ['ﾋ'] = 'ヒ',
            ['ﾌ'] = 'フ',
            ['ﾍ'] = 'ヘ',
            ['ﾎ'] = 'ホ',
            ['ﾏ'] = 'マ',
            ['ﾐ'] = 'ミ',
            ['ﾑ'] = 'ム',
            ['ﾒ'] = 'メ',
            ['ﾓ'] = 'モ',
            ['ﾔ'] = 'ヤ',
            ['ﾕ'] = 'ユ',
            ['ﾖ'] = 'ヨ',
            ['ﾗ'] = 'ラ',
            ['ﾘ'] = 'リ',
            ['ﾙ'] = 'ル',
            ['ﾚ'] = 'レ',
            ['ﾛ'] = 'ロ',
            ['ﾜ'] = 'ワ',
            ['ﾝ'] = 'ン',
            ['ﾞ'] = '゛',
            ['ﾟ'] = '゜',
        };

        public static bool IsDigit(char c)
        {
            return ('0' <= c && c <= '9');
        }

        public static bool IsHiragana(char c)
        {
            //「ぁ」～「より」までと、「ー」「ダブルハイフン」をひらがなとする
            return ('\u3041' <= c && c <= '\u309F') ||
                c == '\u30FC' ||
                c == '\u30A0';
        }

        public static bool IsKatakana(char c)
        {
            //「ダブルハイフン」から「コト」までと、カタカナフリガナ拡張と、
            //濁点と半濁点を全角カタカナとする
            //中点と長音記号も含む
            return ('\u30A0' <= c && c <= '\u30FF') ||
                ('\u31F0' <= c && c <= '\u31FF') ||
                ('\u3099' <= c && c <= '\u309C');
        }

        public static bool IsKanji(char c)
        {
            // NOTE: サロゲートコード(CJK統合漢字拡張B)には対応していません
            return ('\u4E00' <= c && c <= '\u9FCF') ||
                ('\uF900' <= c && c <= '\uFAFF') ||
                ('\u3400' <= c && c <= '\u4DBF');
        }

        public static bool IsCJKSymbol(char c)
        {
            return ('\u3000' <= c && c <= '\u303F');
        }

        public static char ToHankakuChar(char c)
        {
            // ！(0xFF01) ～ ～(0xFF5E)
            if ('！' <= c && c <= '～')
            {
                return (char)(c - 0xFEE0);
            }
            // 全角スペース(0x3000) -> 半角スペース(0x0020)
            else if (c == '　')
            {
                return ' ';
            }
            else if (c == '￥')
            {
                return '\\';
            }
            else if (c == '”' || c == '“')
            {
                return '"';
            }
            else if (c == '’' || c == '‘')
            {
                return '\'';
            }

            return c;
        }

        public static bool IsHankakuKanaChar(char c)
        {
            // ｡(0xFF61) ～ ﾟ(0xFF9F)
            return ('｡' <= c && c <= 'ﾟ');
        }

        public static char ToZenkakuKanaChar(char c)
        {
            if (!IsHankakuKanaChar(c))
            {
                return c;
            }

            if (_hankakuZenkakuKanaMap.TryGetValue(c, out char zc))
            {
                return zc;
            }
            else
            {
                return c;
            }
        }

        public static char ToKatakanaChar(char c)
        {
            // ぁ(0x3041) ～ ゖ(0x3096)
            // ゝ(0x309D) ゞ(0x309E)
            if (('ぁ' <= c && c <= 'ゖ') ||
                ('ゝ' <= c && c <= 'ゞ'))
            {
                return (char)(c + 0x0060);
            }
            else
            {
                return c;
            }
        }

        public static bool IsDakuten(char c)
        {
            return (c == '゛' || c == '\u3099');
        }

        public static bool IsHandakuten(char c)
        {
            return (c == '゜' || c == '\u309A');
        }

        public static char ToDakutenChar(char c)
        {
            // 濁点

            int mod2 = c % 2;
            int mod3 = c % 3;

            // か(0x304B) ～ ち(0x3061)
            // カ(0x30AB) ～ チ(0x30C1)
            // つ(0x3064) ～ と(0x3068)
            // ツ(0x30C4) ～ ト(0x30C8)
            // は(0x306F) ～ ほ(0x307B)
            // ハ(0x30CF) ～ ホ(0x30DB)
            // ゝ(0x309D) ヽ(0x30FD)
            if (('か' <= c && c <= 'ち' && mod2 == 1) ||
                ('カ' <= c && c <= 'チ' && mod2 == 1) ||
                ('つ' <= c && c <= 'と' && mod2 == 0) ||
                ('ツ' <= c && c <= 'ト' && mod2 == 0) ||
                ('は' <= c && c <= 'ほ' && mod3 == 0) ||
                ('ハ' <= c && c <= 'ホ' && mod3 == 0) ||
                c == 'ゝ' || c == 'ヽ')
            {
                return (char)(c + 1);
            }
            // う(0x3046) ウ(0x30A6) -> ゔヴ
            else if (c == 'う' || c == 'ウ')
            {
                return (char)(c + 0x004E);
            }
            // ワ(0x30EF)ヰヱヲ(0x30F2) -> ヷヸヹヺ
            else if ('ワ' <= c && c <= 'ヲ')
            {
                return (char)(c + 8);
            }
            else
            {
                return c;
            }
        }

        public static char ToHandakutenChar(char c)
        {
            // ゜(0x309C)

            int mod3 = c % 3;

            // は(0x306F) ～ ほ(0x307B)
            // ハ(0x30CF) ～ ホ(0x30DB)
            if (('は' <= c && c <= 'ほ' && mod3 == 0) ||
                ('ハ' <= c && c <= 'ホ' && mod3 == 0))
            {
                return (char)(c + 2);
            }
            else
            {
                return c;
            }
        }


        public static char ToUpper(char c)
        {
            // 標準ToUpperは意外と重いのである程度最適してみる
            if ('a' <= c && c <= 'z')
            {
                return (char)(c + ('A' - 'a'));
            }
            if (c < '\u007F')
            {
                return c;
            }

            if (IsHiragana(c) || IsKatakana(c) || IsKanji(c) || IsCJKSymbol(c))
            {
                return c;
            }

            return char.ToUpperInvariant(c);
        }

    }

}
