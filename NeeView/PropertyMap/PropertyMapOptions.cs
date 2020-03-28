using System.Collections.Generic;

namespace NeeView
{
    public class PropertyMapOptions
    {
        public IList<PropertyMapConverter> Converters { get; } = new List<PropertyMapConverter>();
    }
}