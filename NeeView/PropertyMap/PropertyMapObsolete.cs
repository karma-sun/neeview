using System;

namespace NeeView
{
    public class PropertyMapObsolete : PropertyMapNode
    {
        public PropertyMapObsolete()
        {
        }

        public PropertyMapObsolete(string propertyName, Type propertyType, string message)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            Message = message;
        }

        public string PropertyName { get; set; }

        public Type PropertyType { get; set; }

        public string Message { get; set; }
    }

}