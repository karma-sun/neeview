using System;

namespace NeeView
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class MethodArgumentAttribute : Attribute
    {
        public Type Type;
        public string Note;

        public MethodArgumentAttribute(Type type, string note)
        {
            Type = type;
            Note = note;
        }
    }
}
