using System;
using System.Diagnostics;
using System.Reflection;

namespace NeeView
{
    public static class ObjectTools
    { 
        /// <summary>
        /// インスタンスのプロパティを上書き
        /// TODO: 配列や辞書の対応
        /// </summary>
        public static void Merge(object a1, object a2)
        {
            if (a1 == null && a2 == null) return;

            var type = a1.GetType();
            if (type != a2.GetType()) throw new ArgumentException();
            if (!type.IsClass) throw new ArgumentException();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var v1 = property.GetValue(a1);
                var v2 = property.GetValue(a2);

                if (v1 == null && v2 == null)
                {
                }
                else if (property.GetSetMethod(false) == null)
                {
                    Debug.WriteLine($"{property.Name} is readonly");
                }
                else if (property.PropertyType.IsValueType || property.PropertyType == typeof(string) || property.PropertyType.GetCustomAttribute(typeof(PropertyMergeReferenceCopyAttribute)) != null)
                {
                    property.GetSetMethod(false)?.Invoke(a1, new object[] { v2 });
                }

                else
                {
                    if (v1 == null)
                    {
                        v1 = Activator.CreateInstance(type);
                        property.SetValue(a1, v1);
                    }
                    Merge(v1, v2);
                }
            }
        }

    }
}