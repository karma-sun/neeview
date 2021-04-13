using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView.Windows.Property
{
    //
    public abstract class PropertyValue : BindableBase
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
    public class PropertyValue<T> : PropertyValue
    {
        public IValueSetter Setter { get; set; }

        public string Name => Setter.Name;

        public PropertyValue(IValueSetter setter)
        {
            Setter = setter;
            Setter.ValueChanged += (s, e) => RaisePropertyChanged(nameof(Value));
        }

        public T Value
        {
            get { return GetValue(); }
            set { SetValue(value); }
        }

        public virtual T GetValue()
        {
            return (T)Setter.GetValue();
        }

        public virtual void SetValue(object value)
        {
            Setter.SetValue(value);
        }


        public override string GetValueString()
        {
            return Value.ToString();
        }
    }

    //
    public class PropertyValue_Object : PropertyValue<object>
    {
        public PropertyValue_Object(PropertyMemberElement setter) : base(setter)
        {
        }
    }

    //
    public class PropertyValue_Boolean : PropertyValue<bool>
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
    public class PropertyValue_String : PropertyValue<string>
    {
        private PropertyMemberElement _setter;

        public string EmptyMessage { get; private set; }

        public PropertyValue_String(PropertyMemberElement setter) : base(setter)
        {
            _setter = setter;
            EmptyMessage = setter.EmptyMessage;
        }

        public override string GetValue()
        {
            var value = base.GetValue();
            if (_setter.Options.EmptyValue != null && string.IsNullOrEmpty(value))
            {
                return _setter.Options.EmptyValue;
            }
            else
            {
                return value;
            }
        }

        public override void SetValueFromString(string value)
        {
            Value = value;
        }
    }

    public class PropertyValue_StringMap : PropertyValue<string>
    {
        public Dictionary<string, string> Map { get; private set; }

        public string SelectedValue
        {
            get { return (string)Value; }
            set { Value = value; }
        }

        public PropertyValue_StringMap(PropertyMemberElement setter, IEnumerable<string> strings) : base(setter)
        {
            this.Map = setter.Options?.StringMap ?? strings.ToDictionary(e => e, e => e);
        }

        public override void SetValueFromString(string value)
        {
            Value = value;
        }
    }

    //
    public class PropertyValue_Integer : PropertyValue<int>
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
    public class PropertyValue_Double : PropertyValue<double>
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
    public class PropertyValue_Enum : PropertyValue<object>
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
            this.Map = setter.Options?.EnumMap ?? _type.VisibledAliasNameDictionary();
        }

        public override void SetValueFromString(string value)
        {
            Value = Enum.Parse(_type, value);
        }
    }


    //
    public class PropertyValue_Point : PropertyValue<Point>
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
    public class PropertyValue_Color : PropertyValue<Color>
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
    public class PropertyValue_Size : PropertyValue<Size>
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
    public class PropertyValue_TimeSpan : PropertyValue<TimeSpan>
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
    public class RangeProfile<T> : PropertyValue<T>
    {
        public RangeProfile(IValueSetter setter, double min, double max, double tickFrequency, bool isEditable, string format) : base(setter)
        {
            this.Minimum = min;
            this.Maximum = max;
            this.TickFrequency = tickFrequency;
            this.IsEditable = isEditable;
            this.Format = format;

            if (format != null)
            {
                this.Converter = new SafeValueConverter<T>();
                this.FormatConverter = new FormatValueConverter<T>() { Format = format };
            }

            this.WheelCalculator = new NumberDeltaValueCaclulator() { Scale = tickFrequency };
        }


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

        public IValueConverter Converter { get; private set; }

        public IValueConverter FormatConverter { get; private set; }

        public IValueDeltaCalculator WheelCalculator { get; private set; }


        /// <summary>
        /// 整数型ならば1以上の整数にキャスト
        /// </summary>
        private double CastValue(double source)
        {
            // TODO: 他の整数型にも対応
            var code = Type.GetTypeCode(typeof(T));
            if (code == TypeCode.Int32)
            {
                return _tickFrequency < 2.0 ? 1.0 : (int)source;
            }
            else
            {
                return source;
            }
        }
    }

    public class RangeProfile_Double : RangeProfile<double>
    {
        public RangeProfile_Double(IValueSetter setter, double min, double max, double tickFrequency, bool isEditable, string format) : base(setter, min, max, tickFrequency, isEditable, format)
        {
        }
    }

    public class RangeProfile_Integer : RangeProfile<int>
    {
        public RangeProfile_Integer(IValueSetter setter, double min, double max, double tickFrequency, bool isEditable, string format) : base(setter, min, max, tickFrequency, isEditable, format)
        {
        }
    }

    //
    public class PropertyValue_IntegerRange : PropertyValue_Integer
    {
        public RangeProfile_Integer Range { get; private set; }

        public PropertyValue_IntegerRange(PropertyMemberElement setter, RangeProfile_Integer range) : base(setter)
        {
            this.Range = range;
        }
    }

    //
    public class PropertyValue_EditableIntegerRange : PropertyValue_Integer
    {
        public RangeProfile_Integer Range { get; private set; }

        public PropertyValue_EditableIntegerRange(PropertyMemberElement setter, RangeProfile_Integer range) : base(setter)
        {
            this.Range = range;
        }
    }

    //
    public class PropertyValue_DoubleRange : PropertyValue_Double
    {
        public RangeProfile_Double Range { get; private set; }

        public PropertyValue_DoubleRange(PropertyMemberElement setter, RangeProfile_Double range) : base(setter)
        {
            this.Range = range;
        }
    }

    //
    public class PropertyValue_EditableDoubleRange : PropertyValue_Double
    {
        public RangeProfile_Double Range { get; private set; }

        public PropertyValue_EditableDoubleRange(PropertyMemberElement setter, RangeProfile_Double range) : base(setter)
        {
            this.Range = range;
        }
    }

    //
    public class PropertyValue_Percent : PropertyValue_Double
    {
        public RangeProfile_Double Range { get; private set; }

        public PropertyValue_Percent(PropertyMemberElement setter, RangeProfile_Double range) : base(setter)
        {
            this.Range = range;
        }
    }

    //
    public class PropertyValue_FilePath : PropertyValue_String
    {
        public FileDialogType FileDialogType { get; set; }
        public string Filter { get; set; }
        public string Note { get; set; }
        public string DefaultFileName { get; set; }

        public PropertyValue_FilePath(PropertyMemberElement setter, FileDialogType fileDialogType, string filter, string note, string defaultFileName) : base(setter)
        {
            FileDialogType = fileDialogType;
            Filter = filter;
            Note = note;
            DefaultFileName = defaultFileName;
        }
    }
}
