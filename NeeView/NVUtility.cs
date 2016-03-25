// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

// TODO: 整備

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public static partial class NVUtility
    {
        /// <summary>
        /// 超手抜きパーサ
        /// それっぽいimgタグ抜き出し
        /// </summary>
        /// <returns></returns>
        public static List<string> ParseSourceUrl(string source)
        {
            //TextBox1.Text内で正規表現と一致する対象をすべて検索
            System.Text.RegularExpressions.MatchCollection mc =
                System.Text.RegularExpressions.Regex.Matches(
                    source,
                    @"<img(?:\s+[^>]*\s+|\s+)src\s*=\s*(?:(?<quot>[""'])(?<url>.*?)\k<quot>|" +
                        @"(?<url>[^\s>]+))[^>]*>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    | System.Text.RegularExpressions.RegexOptions.Singleline);

            var urls = new List<string>();
            foreach (System.Text.RegularExpressions.Match m in mc)
            {
                //正規表現に一致したグループを表示
                //Debug.WriteLine("URL:{0}", m.Groups["url"].Value);
                urls.Add(m.Groups["url"].Value);
            }

            return urls;
        }



        // 本の場所をタイトルに変換
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

    }
}

