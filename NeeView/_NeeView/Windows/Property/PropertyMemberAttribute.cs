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
        public string Name { get; set; }

        public string Title { get; set; }

        public string Tips { get; set; }

        public bool IsVisible { get; set; } = true;

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
        public double TickFrequency { get; set; }
        public bool IsEditable { get; set; }
        public string Format { get; set; }

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
        public bool IsDirectory { get; set; }
        public string Filter { get; set; }

        public PropertyPathAttribute(string name) : base(name)
        {
        }

        public override PropertyMemberElement CreateContent(object source, PropertyInfo info)
        {
            return new PropertyMemberElement(source, info, this);
        }
    }
}
