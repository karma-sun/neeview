using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Resources;
using System.Linq;

namespace NeeView
{
    public static class HtmlHelpUtility
    {
        public static string GetSearchHelp()
        {
            Uri fileUri = new Uri($"/Resources/{App.Current.Language.GetCultureName()}/SearchOptions.html", UriKind.Relative);
            StreamResourceInfo info = System.Windows.Application.GetResourceStream(fileUri);
            using (StreamReader sr = new StreamReader(info.Stream))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// ヘルプ用HTMLヘッダ生成
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string CraeteHeader(string title)
        {
            string stylesheet = "";
            Uri fileUri = new Uri("/Resources/Style.css", UriKind.Relative);
            StreamResourceInfo info = System.Windows.Application.GetResourceStream(fileUri);
            using (StreamReader sr = new StreamReader(info.Stream))
            {
                stylesheet = sr.ReadToEnd();
                stylesheet = new Regex(@"\s+").Replace(stylesheet, " ");
            }

            string s = "<!DOCTYPE html>\n" +
                @"<html><head>" +
                @"<meta charset=""utf-8"">" +
                "<style>" + stylesheet + "</style>" +
                @"<title>" + title + "</title></head>";

            return s;
        }

        /// <summary>
        /// ヘルプ用HTMLフッタ生成
        /// </summary>
        /// <returns></returns>
        public static string CreateFooter()
        {
            return @"</html>";
        }
    }
}


