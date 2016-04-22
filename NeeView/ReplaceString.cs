﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// キーワード置換
    /// </summary>
    public class ReplaceString
    {
        /// <summary>
        /// キーワード置換ユニット
        /// </summary>
        private class ReplaceUnit
        {
            // 置換有効/無効
            public bool IsEnable { get; set; }
            // キーワード正規表現
            public Regex Regex { get; set; }
            // 置換文字列
            public string ReplaceString { get; set; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="key">キーワード</param>
            /// <param name="replaceString">置換文字列</param>
            public ReplaceUnit(string key, string replaceString)
            {
                Regex = new Regex("\\" + key + "\\b");
                ReplaceString = replaceString;
            }

            /// <summary>
            /// 置換
            /// IsEnableに関係なく置換を行います
            /// </summary>
            /// <param name="s">入力文字列</param>
            /// <returns>置換された文字列</returns>
            public string Replace(string s)
            {
                return Regex.Replace(s, ReplaceString);
            }

            //
            public override string ToString()
            {
                return Regex?.ToString() ?? base.ToString();
            }
        }

        // キーワード辞書
        private Dictionary<string, ReplaceUnit> _Dictionary;

        // 置換フィルタ
        private string _Filter;
        private bool _IsDartyFilter;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ReplaceString()
        {
            _Dictionary = new Dictionary<string, ReplaceUnit>();
            _Filter = "";
            _IsDartyFilter = true;
        }

        /// <summary>
        /// キーワード設定
        /// </summary>
        /// <param name="key">キーワード</param>
        /// <param name="replaceString">置換文字列</param>
        public void Set(string key, string replaceString)
        {
            if (_Dictionary.ContainsKey(key))
            {
                _Dictionary[key].ReplaceString = replaceString;
            }
            else
            {
                _Dictionary[key] = new ReplaceUnit(key, replaceString);
                _IsDartyFilter = true;
            }
        }

        /// <summary>
        /// フィルターを設定。この文字列に含まれるキーワードのみ置換を行う
        /// </summary>
        /// <param name="filter">フィルター文字列</param>
        public void SetFilter(string filter)
        {
            _Filter = filter;
            _IsDartyFilter = true;
        }

        /// <summary>
        /// フィルターから各キーワードの有効無効を設定
        /// フィルターが空の時は全キーワード有効
        /// </summary>
        private void UpdateFilter()
        {
            if (_IsDartyFilter)
            {
                _IsDartyFilter = false;
                foreach (var regexUnit in _Dictionary.Values)
                {
                    regexUnit.IsEnable = string.IsNullOrEmpty(_Filter) || regexUnit.Regex.IsMatch(_Filter);
                }
            }
        }

        /// <summary>
        /// 置換実行
        /// </summary>
        /// <param name="s">置換する文字列</param>
        /// <returns>置換された文字列</returns>
        public string Replace(string s)
        {
            UpdateFilter();
            foreach (var regexUnit in _Dictionary.Values.Where(e => e.IsEnable))
            {
                s = regexUnit.Replace(s);
            }
            return s;
        }
    }

}