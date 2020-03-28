using System;

namespace NeeView
{
    public static class TypeExtensions
    {
        public static string ToManualString(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Int32:
                    return "int";
                case TypeCode.Double:
                    return "double";
                case TypeCode.String:
                    return "string";
            }

            return type.ToString();
        }
    }
}