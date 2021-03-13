namespace NeeView
{
    public class PropertyMapObsolete : PropertyMapNode
    {
        public PropertyMapObsolete()
        {
        }

        public PropertyMapObsolete(string propertyName, string message)
        {
            PropertyName = propertyName;
            Message = message;
        }

        public string PropertyName { get; set; }

        public string Message { get; set; }
    }

}