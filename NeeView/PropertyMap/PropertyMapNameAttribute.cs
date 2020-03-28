using System;

namespace NeeView
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyMapNameAttribute : Attribute
    {
        public string Name;

        public PropertyMapNameAttribute(string name)
        {
            Name = name;
        }
    }
}