// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
    /// CommandParameterプロパティの表示用属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DispNameAttribute : Attribute
    {
        /// <summary>
        /// 表示名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 説明文
        /// </summary>
        public string Tips { get; set; }

        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">ラベル文字列</param>
        public DispNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
