// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// "数値x数値" という文字列で数値を表す
    /// </summary>
    public class SizeString
    {
        /// <summary>
        /// "数値x数値"
        /// </summary>
        public string Value
        {
            get { return _value; }
            private set
            {
                if (_value != value)
                {
                    _value = value;

                    var match = _regex.Match(this.Value);
                    if (!match.Success) throw new ArgumentException();
                    this.Width = int.Parse(match.Groups[1].Value);
                    this.Height = int.Parse(match.Groups[2].Value);
                }
            }
        }

        private string _value;

        //
        public int Width { get; private set; }
        public int Height { get; private set; }


        /// <summary>
        /// フォーマット正規表現
        /// </summary>
        private Regex _regex = new Regex(@"^(\d+)x(\d+)$");

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="value"></param>
        public SizeString(string value)
        {
            Value = value;
        }

        /// <summary>
        /// 有効判定
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return IsValid(this.Value);
        }

        /// <summary>
        /// 有効判定
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return (_regex.IsMatch(value));
        }

        /// <summary>
        /// 正規化。無効の場合はdefaultValueを適用
        /// </summary>
        /// <param name="defaultValue"></param>
        public void Verify(string defaultValue)
        {
            Debug.Assert(IsValid(defaultValue));

            if (!IsValid())
            {
                this.Value = defaultValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Size ToSize()
        {
            return new Size(Width, Height);
        }
    }
}
