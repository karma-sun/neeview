using System;

namespace NeeView
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyMapLabelAttribute : Attribute
    {
        public string Label;

        public PropertyMapLabelAttribute(string label)
        {
            Label = label;
        }
    }
}