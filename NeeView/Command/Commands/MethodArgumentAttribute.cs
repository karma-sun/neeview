using System;

namespace NeeView
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class MethodArgumentAttribute : Attribute
    {
        public string Note;

        public MethodArgumentAttribute(string note)
        {
            Note = note;
        }
    }
}
