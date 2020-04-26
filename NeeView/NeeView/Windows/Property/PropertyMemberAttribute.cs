using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Windows.Property
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyMemberAttribute : Attribute
    {
        public string Name;
        public string Title;
        public string Tips;
        public bool IsVisible = true;
        public string EmptyMessage;

        public PropertyMemberAttribute() { }
        public PropertyMemberAttribute(string name) { Name = name; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyRangeAttribute : PropertyMemberAttribute
    {
        public double Minimum;
        public double Maximum;
        public double TickFrequency;
        public bool IsEditable;
        public string Format;

        public PropertyRangeAttribute(double min, double max)
        {
            Minimum = min;
            Maximum = max;
        }

        public PropertyRangeAttribute(string name, double min, double max) : base(name)
        {
            Minimum = min;
            Maximum = max;
        }
    }

    /// <summary>
    /// double range: 0.0 - 1.0
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyPercentAttribute : PropertyMemberAttribute
    {
        public PropertyPercentAttribute(string name) : base(name)
        {
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyPathAttribute : PropertyMemberAttribute
    {
        public FileDialogType FileDialogType;
        public string Filter;
        public string Note;
        public string DefaultFileName;

        public PropertyPathAttribute(string name) : base(name)
        {
        }
    }
}
