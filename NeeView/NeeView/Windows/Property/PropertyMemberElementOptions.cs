using System;
using System.Collections.Generic;

namespace NeeView.Windows.Property
{
    public class PropertyMemberElementOptions
    {
        public static PropertyMemberElementOptions Default { get; } = new PropertyMemberElementOptions();

        /// <summary>
        /// 名前(上書き用)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 空欄時に表示する文字列
        /// </summary>
        public string EmptyValue { get; set; }

        /// <summary>
        /// EnumValueでの選択項目指定
        /// </summary>
        public Dictionary<Enum, string> EnumMap { get; set; }
    }
}
