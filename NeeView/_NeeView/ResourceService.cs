using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace NeeView
{
    internal static class ResourceService
    {
        private static Regex _regexKey = new Regex(@"@\w+");


        /// <summary>
        /// @で始まる文字列はリソースキーとしてその値を返す。
        /// そうでない場合はそのまま返す。
        /// </summary>
        public static string GetString(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key[0] != '@')
            {
                return key;
            }
            else
            {
                var text = GetResourceString(key);
                if (text != null)
                {
                    return Replace(text);
                }
                else
                {
                    Debug.WriteLine($"Error: Not found resource key: {key.Substring(1)}");
                    return key;
                }
            }
        }

        /// <summary>
        /// @で始まる文字列をリソースキーとして文字列を入れ替える。
        /// </summary>
        public static string Replace(string s)
        {
            return _regexKey.Replace(s, m => GetResourceString(m.Value) ?? m.Value);
        }

        /// <summary>
        /// リソースキーからリソース文字列取得
        /// </summary>
        /// <param name="key">@で始まるリソースキー</param>
        /// <returns>存在しない場合はnull</returns>
        public static string GetResourceString(string key)
        {
            if (key[0] != '@') return null;
            return Properties.Resources.ResourceManager.GetString(key.Substring(1), Properties.Resources.Culture);
        }

        /// <summary>
        /// 連結単語文字列を生成
        /// </summary>
        public static string Join(IEnumerable<string> tokens)
        {
            return string.Join(Properties.Resources.TokenSeparator, tokens.Select(e => string.Format(Properties.Resources.TokenFormat, e)));
        }
    }
}
