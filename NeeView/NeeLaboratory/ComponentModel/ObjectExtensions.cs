using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeLaboratory.ComponentModel
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// 汎用SWAP
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        /// <summary>
        /// Deep Copy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(T source)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using (var mem = new MemoryStream())
            {
                serializer.WriteObject(mem, source);
                mem.Position = 0;
                return (T)serializer.ReadObject(mem);
            }
        }

        /// <summary>
        /// DevaultValue属性でプロパティを初期化する
        /// from: https://stackoverflow.com/questions/2329868/net-defaultvalue-attribute
        /// </summary>
        /// <param name="obj"></param>
        public static void InitializePropertyDefaultValues(this object obj)
        {
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                var d = prop.GetCustomAttribute<DefaultValueAttribute>();
                if (d != null)
                {
                    prop.SetValue(obj, d.Value);
                }
            }
        }

        /// <summary>
        /// Creates the Equals() method. public properties only.
        /// from https://www.brad-smith.info/blog/archives/385
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Func<object, object, bool> MakeEqualsMethod(Type type)
        {
            ParameterExpression pThis = Expression.Parameter(typeof(object), "x");
            ParameterExpression pThat = Expression.Parameter(typeof(object), "y");

            // cast to the subclass type
            UnaryExpression pCastThis = Expression.Convert(pThis, type);
            UnaryExpression pCastThat = Expression.Convert(pThat, type);

            // compound AND expression using short-circuit evaluation
            Expression last = null;
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.GetCustomAttribute(typeof(EqualsIgnoreAttribute)) != null)
                    continue;

                Debug.WriteLine($"MakeEqualMethod: {type.Name}.{property.Name}");

                BinaryExpression equals = Expression.Equal(
                    Expression.Property(pCastThis, property),
                    Expression.Property(pCastThat, property)
                );

                if (last == null)
                    last = equals;
                else
                    last = Expression.AndAlso(last, equals);
            }

            // call Object.Equals if second parameter doesn't match type
            last = Expression.Condition(
                Expression.TypeIs(pThat, type),
                last,
                Expression.Equal(pThis, pThat)
            );

            // compile method
            return Expression.Lambda<Func<object, object, bool>>(last, pThis, pThat).Compile();
        }
    }

    /// <summary>
    /// MakeEqualsMethod の対象外とする属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EqualsIgnoreAttribute : Attribute
    {
    }
}
