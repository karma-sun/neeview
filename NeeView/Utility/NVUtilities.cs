// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace NeeView
{
    public static partial class NVUtility
    {
        /// <summary>
        ///  イメージタグ用正規表現
        /// </summary>
        private static Regex s_imageTagRegex = new Regex(
            @"<img(?:\s+[^>]*\s+|\s+)src\s*=\s*(?:(?<quot>[""'])(?<url>.*?)\k<quot>|(?<url>[^\s>]+))[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// imgタグ抜き出し
        /// </summary>
        /// <returns>imgタグのURLリスト</returns>
        public static List<string> ParseSourceUrl(string source)
        {
            var matchCollection = s_imageTagRegex.Matches(source);
            var urls = new List<string>();
            foreach (System.Text.RegularExpressions.Match match in matchCollection)
            {
                urls.Add(match.Groups["url"].Value);
            }

            return urls;
        }


        /// <summary>
        /// 本の場所をタイトル文字列に整形
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        public static string PlaceToTitle(string place)
        {
            if (place.StartsWith("http://") || place.StartsWith("https://"))
            {
                return new Uri(place).Host;
            }
            else if (place.StartsWith("data:"))
            {
                return "HTML埋め込み画像";
            }
            else
            {
                return LoosePath.GetFileName(place);
            }
        }


        /// <summary>
        /// ドラッグオブジェクトのダンプ
        /// </summary>
        /// <param name="data"></param>
        [Conditional("DEBUG")]
        public static void DumpDragData(System.Windows.IDataObject data)
        {
            Debug.WriteLine("----");
            foreach (var name in data.GetFormats(true))
            {
                try
                {
                    var obj = data.GetData(name);
                    if (obj is System.IO.MemoryStream)
                    {
                        Debug.WriteLine($"<{name}>: {obj} ({(obj as System.IO.MemoryStream).Length})");
                    }
                    else if (obj is string)
                    {
                        Debug.WriteLine($"<{name}>: string: {obj.ToString()}");
                    }
                    else
                    {
                        Debug.WriteLine($"<{name}>: {obj}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"<{name}>: [Exception]: {ex.Message}");
                }
            }
            Debug.WriteLine("----");
        }


        // ファイル名重複を回避する
        public static string CreateUniquePath(string source)
        {
            var path = source;

            var directory = Path.GetDirectoryName(path);
            var filename = Path.GetFileNameWithoutExtension(path);
            var extension = Path.GetExtension(path);
            int count = 1;

            var regex = new Regex(@"^(.+)\((\d+)\)$");
            var match = regex.Match(filename);
            if (match.Success)
            {
                filename = match.Groups[1].Value.Trim();
                count = int.Parse(match.Groups[2].Value);
            }

            // ファイル名作成
            while (File.Exists(path) || Directory.Exists(path))
            {
                count++;
                path = Path.Combine(directory, $"{filename} ({count}){extension}");
            }

            return path;
        }


        /// <summary>
        /// 画像フォーマット判定
        /// <param name="buff">判定するデータ</param>
        /// <returns>対応拡張子群。対応できない場合はnull</returns>
        /// </summary>
        public static string[] GetSupportImageExtensions(byte[] buff)
        {
            var extensions = GetDefaultSupportImageExtensions(buff);
            if (extensions == null) extensions = GetSusieSupportImageExtensions(buff);
            return extensions;
        }

        /// <summary>
        /// 画像フォーマット判定(標準)
        /// </summary>
        /// <param name="buff">判定するデータ</param>
        /// <returns>対応拡張子群。対応できない場合はnull</returns>
        public static string[] GetDefaultSupportImageExtensions(byte[] buff)
        {
            try
            {
                using (var stream = new MemoryStream(buff))
                {
                    var bitmap = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);
                    return bitmap.Decoder.CodecInfo.FileExtensions.ToLower().Split(',', ';');
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }


        /// <summary>
        /// 画像フォーマット判定(Susie)
        /// </summary>
        /// <param name="buff">判定するデータ</param>
        /// <returns>対応拡張子群。対応できない場合はnull</returns>
        public static string[] GetSusieSupportImageExtensions(byte[] buff)
        {
            try
            {
                if (!ModelContext.SusieContext.IsEnableSusie) return null;
                var plugin = ModelContext.Susie?.GetImagePlugin("dummy", buff, false);
                return plugin?.Extensions.ToArray();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }


#if false
        // Win32 APIのインポート
        [DllImport("USER32.DLL")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, UInt32 bRevert);
        [DllImport("USER32.DLL")]
        private static extern bool AppendMenu(IntPtr hMenu, UInt32 uFlags, UInt32 uIDNewItem, string lpNewItem);

        // オリジナルメニューを追加するための値
        private const UInt32 MF_BYCOMMAND = 0x00000000;
        private const UInt32 MF_STRING = 0x00000000;
        private const UInt32 MF_SEPARATOR = 0x00000800;

        public static void AppendMenu(Window window)
        {
            // タイトルバーのコンテキストメニューを取得
            var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            IntPtr hMenu = GetSystemMenu(hwnd, 0);

            // セパレータとメニューを追加
            AppendMenu(hMenu, MF_SEPARATOR, 0, string.Empty);
            AppendMenu(hMenu, MF_STRING, MF_BYCOMMAND, "バージョン情報");

        }
#endif


        public static string HtmlHelpHeader(string title)
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

        //
        public static string HtmlHelpFooter()
        {
            return @"</html>";
        }


        // from http://stackoverflow.com/questions/2683442/where-can-i-find-the-clamp-function-in-net
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }


        /// <summary>
        /// 子のビジュアル要素を見つける
        /// ... どこかからのコピペ。どこだったかな...
        /// </summary>
        /// <typeparam name="T">見つけたい要素の型</typeparam>
        /// <param name="obj">検索の起点</param>
        /// <returns>見つかった要素。見つからなかった場合はnull</returns>
        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child is T)
                {
                    return (T)child;
                }
                else
                {
                    child = FindVisualChild<T>(child);
                    if (child != null)
                    {
                        return (T)child;
                    }
                }
            }
            return null;
        }
    }
}

