using System;
using System.Reflection;

namespace NeeView
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class MethodArgumentAttribute : Attribute
    {
        public string Note;

        public MethodArgumentAttribute()
        {
        }

        public MethodArgumentAttribute(string note)
        {
            Note = note;
        }
    }


    public static class MethodArgumentAttributeExtensions
    {
        private static string GetResourceKey(MethodInfo method, string postfix = null)
        {
            return $"@{method.DeclaringType.Name}.{method.Name}{postfix}";
        }

        public static string GetMethodNote(MethodInfo property, MethodArgumentAttribute attribute)
        {
            if (attribute is null)
            {
                return null;
            }

            var resourceKey = attribute.Note ?? GetResourceKey(property, "#Remarks");
            var resourceValue = ResourceService.GetResourceString(resourceKey, true);

            return resourceValue;
        }

        public static string GetMethodNote(MethodInfo property)
        {
            return GetMethodNote(property, property.GetCustomAttribute<MethodArgumentAttribute>());
        }
    }
}
