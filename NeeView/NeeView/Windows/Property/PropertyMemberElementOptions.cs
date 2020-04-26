using System;
using System.Collections.Generic;

namespace NeeView.Windows.Property
{
    public class PropertyMemberElementOptions
    {
        public static PropertyMemberElementOptions Default { get; } = new PropertyMemberElementOptions();

        public string EmptyValue { get; set; }

        /// <summary>
        /// EnumValueでの選択項目指定
        /// </summary>
        public Dictionary<Enum, string> EnumMap { get; set; }
    }
}
