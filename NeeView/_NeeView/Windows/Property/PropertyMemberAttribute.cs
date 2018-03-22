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

        public override PropertyMemberElement CreateContent(object source, PropertyInfo info)
        {
            return new PropertyMemberElement(source, info, this);
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyPathAttribute : PropertyMemberAttribute
    {
        public bool IsDirectory;
        public string Filter;

        public PropertyPathAttribute(string name) : base(name)
        {
        }

        public override PropertyMemberElement CreateContent(object source, PropertyInfo info)
        {
            return new PropertyMemberElement(source, info, this);
        }
    }
}
