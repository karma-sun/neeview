// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public virtual string GetTypeString()
        {
            return "???";
        }

        public virtual void SetValueFromString(string value)
        {
            throw new NotSupportedException();
        }
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

        public override string GetTypeString()
        {
            return "真偽値";
        }

        public override void SetValueFromString(string value)
        {
            Value = bool.Parse(value);
        }
    }

    //
    public class PropertyValue_String : PropertyValue<string, PropertyMemberElement>
    {
        public PropertyValue_String(PropertyMemberElement setter) : base(setter)
        {
        }

        public override string GetTypeString()
        {
            return "文字列";
        }

        public override void SetValueFromString(string value)
        {
            Value = value;
        }
    }

    //
    public class PropertyValue_Integer : PropertyValue<int, PropertyMemberElement>
    {
        public PropertyValue_Integer(PropertyMemberElement setter) : base(setter)
        {
        }

        public override string GetTypeString()
        {
            return "整数値";
        }

        public override void SetValueFromString(string value)
        {
            Value = int.Parse(value);
        }
    }

    //
    public class PropertyValue_Double : PropertyValue<double, PropertyMemberElement>
    {
        public PropertyValue_Double(PropertyMemberElement setter) : base(setter)
        {
        }

        public override string GetTypeString()
        {
            return "実数値";
        }

        public override void SetValueFromString(string value)
        {
            Value = double.Parse(value);
        }
    }



    //
    public class PropertyValue_Enum : PropertyValue<object, PropertyMemberElement>
    {
        private Type _type;

        public List<string> Items { get; private set; }

        public string SelectedItem
        {
            get { return Value.ToString(); }
            set { Value = Enum.Parse(_type, value); }
        }

        public PropertyValue_Enum(PropertyMemberElement setter, Type enumType) : base(setter)
        {
            _type = enumType;
            this.Items = new List<string>(Enum.GetNames(_type));
        }

        public override string GetTypeString()
        {
            return "選択値";
        }

        public override void SetValueFromString(string value)
        {
            Value = Enum.Parse(_type, value);
        }
    }


    //
    public class PropertyValue_Point : PropertyValue<Point, PropertyMemberElement>
    {
        public PropertyValue_Point(PropertyMemberElement setter) : base(setter)
        {
        }

        public override string GetTypeString()
        {
            return "座標";
        }

        public override void SetValueFromString(string value)
        {
            Value = Point.Parse(value);
        }
    }

    //
    public class PropertyValue_Color : PropertyValue<Color, PropertyMemberElement>
    {
        public PropertyValue_Color(PropertyMemberElement setter) : base(setter)
        {
        }

        public override string GetTypeString()
        {
            return "カラー";
        }

        public override void SetValueFromString(string value)
        {
            Value = (Color)ColorConverter.ConvertFromString(value);
        }
    }

    //
    public class PropertyValue_IntegerRange : PropertyValue_Integer
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; }
        public int SmallChange => (Maximum - Minimum) / 10;
        public int LargeChange => (Maximum - Minimum) / 4;
        public int TickFrequency
        {
            get
            {
                var delta = (Maximum - Minimum) * 0.01;
                return delta < 2.0 ? 1 : (int)delta;
            }
        }

        public PropertyValue_IntegerRange(PropertyMemberElement setter, int min, int max) : base(setter)
        {
            Minimum = min;
            Maximum = max;
        }
    }

    //
    public class PropertyValue_DoubleRange : PropertyValue_Double
    {
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double SmallChange => (Maximum - Minimum) * 0.1;
        public double LargeChange => (Maximum - Minimum) * 0.25;
        public double TickFrequency => (Maximum - Minimum) * 0.01;

        public PropertyValue_DoubleRange(PropertyMemberElement setter, double min, double max) : base(setter)
        {
            Minimum = min;
            Maximum = max;
        }
    }

    //
    public class PropertyValue_FilePath : PropertyValue_String
    {
        public PropertyValue_FilePath(PropertyMemberElement setter) : base(setter)
        {
        }

        public override string GetTypeString()
        {
            return "ファイルの場所";
        }
    }
}