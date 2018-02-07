// from Stack Overflow 
// URL: http://ja.stackoverflow.com/questions/23202/bitmapdecoder-%E3%81%8C%E3%82%B5%E3%83%9D%E3%83%BC%E3%83%88%E3%81%97%E3%81%A6%E3%81%84%E3%82%8B%E7%94%BB%E5%83%8F%E3%83%95%E3%82%A1%E3%82%A4%E3%83%AB%E3%81%AE%E7%A8%AE%E9%A1%9E%E6%8B%A1%E5%BC%B5%E5%AD%90%E3%82%92%E5%85%A8%E3%81%A6%E5%8F%96%E5%BE%97%E3%81%97%E3%81%9F%E3%81%84
// License: CC BY-SA 3.0

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public static class WicDecoders
    {
        /// <summary>
        /// WICデコーダーリスト取得
        /// </summary>
        /// <returns>デコーダー名とその対応拡張子の辞書</returns>
        public static Dictionary<string, string> ListUp()
        {
            var dictionary = new Dictionary<string, string>();

            //string root = @"SOFTWARE\WOW6432Node\Classes\CLSID\";
            string root = @"SOFTWARE\Classes\";

            // WICBitmapDecodersの一覧を開く
            var decoders = Registry.LocalMachine.OpenSubKey(root + @"CLSID\{7ED96837-96F0-4812-B211-F13C24117ED3}\Instance");
            foreach (var clsId in decoders.GetSubKeyNames())
            {
                try
                {
                    // コーデックのレジストリを開く
                    var codec = Registry.LocalMachine.OpenSubKey(root + @"CLSID\" + clsId);
                    string name = codec.GetValue("FriendlyName").ToString();
                    string extensions = codec.GetValue("FileExtensions").ToString().ToLower();
                    dictionary.Add(name, extensions);
                }
                catch { }
            }

            return dictionary;
        }
    }
}
