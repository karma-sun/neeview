using System;

namespace NeeView
{
    public class PropertyMapObsolete : PropertyMapNode
    {
        public PropertyMapObsolete()
        {
        }

        public PropertyMapObsolete(string propertyName, Type propertyType, string message, ObsoleteAttribute obsolete, AlternativeAttribute alternative)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            Message = message;
            Obsolete = obsolete;
            Alternative = alternative;
        }

        public string PropertyName { get; set; }

        public Type PropertyType { get; set; }

        public string Message { get; set; }

        public ObsoleteAttribute Obsolete { get; set; }

        public AlternativeAttribute Alternative { get; set; }


        public string CreateObsoleteMessage()
        {
            return RefrectionTools.CreateObsoleteMessage(PropertyName, Obsolete, Alternative);
        }
    }

}