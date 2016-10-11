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
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyMemberAttribute : Attribute
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public string Tips { get; set; }

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
        public double Min { get; set; }
        public double Max { get; set; }

        public double TickFrequency => (Max - Min) * 0.01;

        public PropertyRangeAttribute(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public override PropertyMemberElement CreateContent(object source, PropertyInfo info)
        {
            return new PropertyRangeElement(source, info, this);
        }
    }

}
