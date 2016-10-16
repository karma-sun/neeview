﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        private static char[] s_sepalator = new char[] { '\\', '/' };

        //
        public static string GetFileName(string s)
        {
            if (s == null) return "";
            return s.Split(s_sepalator, StringSplitOptions.RemoveEmptyEntries).Last();
        }

        //
        public static string GetPathRoot(string s)
        {
            if (s == null) return "";
            var parts = s.Split(s_sepalator, 2);
            return parts.First();
        }

        //
        public static string GetDirectoryName(string s)
        {
            if (s == null) return "";
            var parts = s.Split(s_sepalator, StringSplitOptions.RemoveEmptyEntries).ToList();
            parts.RemoveAt(parts.Count - 1);
            return string.Join("\\", parts);
        }

        //
        public static string GetExtension(string s)
        {
            if (s == null) return "";
            string fileName = GetFileName(s);
            int index = fileName.LastIndexOf('.');

            return (index >= 0) ? fileName.Substring(index).ToLower() : "";
        }

        //
        public static string Combine(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
                return s2;
            else
                return s1.TrimEnd(s_sepalator) + "\\" + s2.TrimStart(s_sepalator);
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
    }
}
