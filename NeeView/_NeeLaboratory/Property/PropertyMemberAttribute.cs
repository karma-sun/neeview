// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NeeLaboratory.Property
{
    [Flags]
    public enum PropertyMemberFlag
    {
        None,
        
        /// <summary>
        /// 詳細設定で表示される
        /// </summary>
        Details,
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyMemberAttribute : Attribute
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public string Tips { get; set; }

        public PropertyMemberFlag Flags { get; set; } = PropertyMemberFlag.Details;

        public PropertyMemberAttribute() { }
        public PropertyMemberAttribute(string name) { Name = name; }

        public virtual PropertyMemberElement CreateContent(object source, PropertyInfo info)
        {
            return new PropertyMemberElement(source, info, this);
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyRangeAttribute : PropertyMemberAttribute
    {
        public double Minimum { get; set; }
        public double Maximum { get; set; }

        public double TickFrequency => (Maximum - Minimum) * 0.01;

        public PropertyRangeAttribute(double min, double max)
        {
            Minimum = min;
            Maximum = max;
        }

        public override PropertyMemberElement CreateContent(object source, PropertyInfo info)
        {
            return new PropertyMemberElement(source, info, this);
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyEnumAttribute : PropertyMemberAttribute
    {
        public PropertyEnumAttribute(string name) : base(name)
        {
        }

        public override PropertyMemberElement CreateContent(object source, PropertyInfo info)
        {
            return new PropertyMemberElement(source, info, this);
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyPathAttribute : PropertyMemberAttribute
    {
        public override PropertyMemberElement CreateContent(object source, PropertyInfo info)
        {
            return new PropertyMemberElement(source, info, this);
        }
    }
}
