using System.Collections.Generic;
using System.Text.RegularExpressions;




namespace NeeView
{
    public static class HtmlParseUtility
    {
        /// <summary>
        ///  imgタグ用正規表現
        /// </summary>
        private static Regex _imageTagRegex = new Regex(
            @"<img(?:\s+[^>]*\s+|\s+)src\s*=\s*(?:(?<quot>[""'])(?<url>.*?)\k<quot>|(?<url>[^\s>]+))[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// imgタグ抜き出し
        /// </summary>
        /// <returns>imgタグのURLリスト</returns>
        public static List<string> CollectImgSrc(string source)
        {
            var matchCollection = _imageTagRegex.Matches(source);
            var urls = new List<string>();
            foreach (System.Text.RegularExpressions.Match match in matchCollection)
            {
                urls.Add(match.Groups["url"].Value);
            }

            return urls;
        }
    }
}


