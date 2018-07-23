using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ファイルシステム規約に依存しないパス文字列ユーティリティ
    /// ファイル名に使用できない文字を含んだパスの解析用
    /// </summary>
    public static class LoosePath
    {
        public static char[] Separator = new char[] { '\\', '/' };


        //
        public static string TrimEnd(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            if (Separator.Contains(s.Last()))
            {
                s = s.TrimEnd(Separator);
                if (s.Last() == ':') s += "\\";
            }

            return s;
        }

        //
        public static string[] Split(string s)
        {
            if (string.IsNullOrEmpty(s)) return new string[0];
            return s.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        }

        //
        public static string GetFileName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Split(Separator, StringSplitOptions.RemoveEmptyEntries).Last();
        }

        // place部をディレクトリーとみなしたファイル名取得
        public static string GetFileName(string s, string place)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (string.IsNullOrEmpty(place)) return s;
            if (string.Compare(s, 0, place, 0, place.Length) != 0) throw new ArgumentException("s not contain place");
            return s.Substring(place.Length).TrimStart(Separator);
        }

        //
        public static string GetPathRoot(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var parts = s.Split(Separator, 2);
            return parts.First();
        }

        //
        public static string GetDirectoryName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            var parts = s.Split(Separator, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (parts.Count <= 1) return "";

            parts.RemoveAt(parts.Count - 1);
            var path = GetHeadSeparators(s) + string.Join("\\", parts);
            if (parts.Count == 1 && path.Last() == ':') path += "\\";

            return path;
        }

        //
        public static string GetExtension(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            string fileName = GetFileName(s);
            int index = fileName.LastIndexOf('.');

            return (index >= 0) ? fileName.Substring(index).ToLower() : "";
        }

        //
        public static string Combine(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
                return s2;
            else if (string.IsNullOrEmpty(s2))
                return s1;
            else
                return s1.TrimEnd(Separator) + "\\" + s2.TrimStart(Separator);
        }

        // ファイル名として使えない文字を置換
        public static string ValidFileName(string s)
        {
            string valid = s;
            char[] invalidch = System.IO.Path.GetInvalidFileNameChars();

            foreach (char c in invalidch)
            {
                valid = valid.Replace(c, '_');
            }
            return valid;
        }

        // セパレータ標準化
        public static string NormalizeSeparator(string s)
        {
            return s?.Replace('/', '\\');
        }

        // UNC判定
        public static bool IsUnc(string s)
        {
            var head = GetHeadSeparators(s);
            return head.Length == 2;
        }

        // パス先頭にあるセパレータ部を取得
        private static string GetHeadSeparators(string s)
        {
            var slashCount = 0;
            foreach (var c in s)
            {
                if (c == '\\' || c == '/')
                {
                    slashCount++;
                }
                else
                {
                    break;
                }
            }

            return slashCount > 0 ? new string('\\', slashCount) : "";
        }

        // 表示用のファイル名生成
        public static string GetDispName(string s)
        {
            if (s == null)
            {
                return "PC";
            }
            else
            {
                // ドライブ名なら終端に「￥」を付ける
                var name = LoosePath.GetFileName(s);
                if (s.Length <= 3 && name.Length == 2 && name[1] == ':')
                {
                    name += '\\';
                }
                return name;
            }
        }
    }
}
