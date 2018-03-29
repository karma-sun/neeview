using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView.Windows.Property
{
    //
    public abstract class PropertyValue
    {
        public virtual string GetValueString()
        {
            throw new NotSupportedException();
        }

        public virtual void SetValueFromString(string value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 表示形式を指定する文字列
        /// </summary>
        public string VisualType { get; set; }
    }


    //
    public class PropertyValue<T, S> : PropertyValue
        where S : IValueSetter
    {
        public S Setter { get; set; }

        public string Name => Setter.Name;

        public PropertyValue(S setter)
        {
            Setter = setter;
        }

        public T Value
        {
            get { return (T)Setter.GetValue(); }
            set { Setter.SetValue(value); }
        }

        public override string GetValueString()
        {
            return Value.ToString();
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

        public override void SetValueFromString(string value)
        {
            Value = double.Parse(value);
        }
    }



    //
    public class PropertyValue_Enum : PropertyValue<object, PropertyMemberElement>
    {
        private Type _type;

        public Dictionary<Enum, string> Map { get; private set; }

        public Enum SelectedValue
        {
            get { return (Enum)Value; }
            set { Value = value; }
        }

        public PropertyValue_Enum(PropertyMemberElement setter, Type enumType) : base(setter)
        {
            _type = enumType;
            this.Map = _type.AliasNameDictionary();
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

        public override void SetValueFromString(string value)
        {
            Value = (Color)ColorConverter.ConvertFromString(value);
        }
    }

    //
    public class PropertyValue_Size : PropertyValue<Size, PropertyMemberElement>
    {
        public PropertyValue_Size(PropertyMemberElement setter) : base(setter)
        {
        }

        public override void SetValueFromString(string value)
        {
            Value = Size.Parse(value);
        }

        public override string GetValueString()
        {
            return $"{Value.Width}x{Value.Height}";
        }
    }

    //
    public class PropertyValue_TimeSpan : PropertyValue<TimeSpan, PropertyMemberElement>
    {
        public PropertyValue_TimeSpan(PropertyMemberElement setter) : base(setter)
        {
        }

        public override void SetValueFromString(string value)
        {
            Value = TimeSpan.Parse(value);
        }
    }


    /// <summary>
    /// スライダー用パラメータ
    /// </summary>
    public class RangeProfile
    {
        #region Fields

        private bool _isInteger;

        #endregion

        #region Constructors

        public RangeProfile(bool isInteger, double min, double max)
        {
            _isInteger = isInteger;
            this.Minimum = min;
            this.Maximum = max;
        }

        public RangeProfile(bool isInteger, double min, double max, double tickFrequency, bool isEditable, string format)
        {
            _isInteger = isInteger;
            this.Minimum = min;
            this.Maximum = max;
            this.TickFrequency = tickFrequency;
            this.IsEditable = isEditable;
            this.Format = format;
        }

        #endregion

        #region Properties

        public double Minimum { get; private set; }
        public double Maximum { get; private set; }
        public double SmallChange => CastValue((Maximum - Minimum) * 0.1);
        public double LargeChange => CastValue((Maximum - Minimum) * 0.25);

        private double _tickFrequency;
        public double TickFrequency
        {
            get { return _tickFrequency <= 0.0 ? CastValue((Maximum - Minimum) * 0.01) : _tickFrequency; }
            private set { _tickFrequency = value; }
        }

        /// <summary>
        /// スライダーだけでなく直接値の編集が可能
        /// </summary>
        public bool IsEditable { get; private set; }

        /// <summary>
        /// 表示文字列フォーマット
        /// </summary>
        public string Format { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// 整数型ならば1以上の整数にキャスト
        /// </summary>
        private double CastValue(double source)
        {
            if (_isInteger)
            {
                return _tickFrequency < 2.0 ? 1.0 : (int)source;
            }
            else
            {
                return source;
            }
        }

        #endregion
    }
    
    //
    public class PropertyValue_IntegerRange : PropertyValue_Integer
    {
        public RangeProfile Range { get; private set; }

        public PropertyValue_IntegerRange(PropertyMemberElement setter, RangeProfile range) : base(setter)
        {
            this.Range = range;
        }
    }

    //
    public class PropertyValue_DoubleRange : PropertyValue_Double
    {
        public RangeProfile Range { get; private set; }

        public PropertyValue_DoubleRange(PropertyMemberElement setter, RangeProfile range) : base(setter)
        {
            this.Range = range;
        }
    }

    //
    public class PropertyValue_FilePath : PropertyValue_String
    {
        public bool IsDirectory { get; set; }
        public string Filter { get; set; }

        public PropertyValue_FilePath(PropertyMemberElement setter, bool isDirectory, string filter) : base(setter)
        {
            IsDirectory = isDirectory;
            Filter = filter;
        }
    }
}
