// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeLaboratory.Property
{
    //
    public abstract class PropertyValue
    {
    }


    //
    public class PropertyValue<T, S> : PropertyValue where S : IValueSetter
    {
        public S Setter { get; set; }

        public PropertyValue(S setter)
        {
            Setter = setter;
        }

        public T Value
        {
            get { return (T)Setter.GetValue(); }
            set { Setter.SetValue(value); }
        }
    }

    //
    public class PropertyValue_Object : PropertyValue<object, PropertyMemberElement>
    {
        public PropertyValue_Object(PropertyMemberElement setter) : base(setter)
        {
        }
    }

    //
    public class PropertyValue_Boolean : PropertyValue<bool, PropertyMemberElement>
    {
        public PropertyValue_Boolean(PropertyMemberElement setter) : base(setter)
        {
        }
    }

    //
    public class PropertyValue_String : PropertyValue<string, PropertyMemberElement>
    {
        public PropertyValue_String(PropertyMemberElement setter) : base(setter)
        {
        }
    }

    //
    public class PropertyValue_Integer : PropertyValue<int, PropertyMemberElement>
    {
        public PropertyValue_Integer(PropertyMemberElement setter) : base(setter)
        {
        }
    }

    //
    public class PropertyValue_Double : PropertyValue<double, PropertyMemberElement>
    {
        public PropertyValue_Double(PropertyMemberElement setter) : base(setter)
        {
        }
    }

    //
    public class PropertyValue_Point : PropertyValue<Point, PropertyMemberElement>
    {
        public PropertyValue_Point(PropertyMemberElement setter) : base(setter)
        {
        }
    }

    //
    public class PropertyValue_Color : PropertyValue<Color, PropertyMemberElement>
    {
        public PropertyValue_Color(PropertyMemberElement setter) : base(setter)
        {
        }
    }


}
