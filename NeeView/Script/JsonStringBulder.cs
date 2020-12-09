using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NeeView
{
    /// <summary>
    /// object to json string builder
    /// NeeView.Jint special
    /// </summary>
    public class JsonStringBulder
    {
        private IndentStringBuilder _builder = new IndentStringBuilder();

        public override string ToString()
        {
            return _builder.ToString();
        }

        public JsonStringBulder AppendObject(object source)
        {
            AppendObject(_builder, source, false);
            return this;
        }

        private IndentStringBuilder AppendObject(IndentStringBuilder builder, object source, bool isNest)
        {
            if (source is null)
            {
                return builder.Append("null");
            }

            var type = source.GetType();

            if (source is Enum enm)
            {
                return builder.Append(Convert.ToInt32(enm).ToString());
            }
            else if (source is bool boolean)
            {
                return builder.Append(boolean ? "true" : "false");
            }
            else if (source is string str)
            {
                return builder.Append(isNest ? "\"" + JavaScriptStringEncode(str) + "\"" : str);
            }
            else if (source is object[] objects)
            {
                return AppendCollection(builder, objects, true);
            }
            else if (source is IDictionary<string, object> genericDictionary) // for ExpandoObject
            {
                return AppendGenericDictionary(builder, genericDictionary, true);
            }
            else if (source is IDictionary dictionary)
            {
                return AppendDictionary(builder, dictionary, true);
            }
            else if (source is IList && source is IEnumerable collection)
            {
                return AppendCollection(builder, collection, true);
            }
            else if (source is PropertyMap propertyMap)
            {
                return AppendDictionary(builder, propertyMap.ToDictionary(e => e.Key, e => propertyMap.GetValue(e.Value)), true);
            }
            else if (type.IsClass && !IsDelegate(type))
            {
                try
                {
                    var dic = type
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(e => e.CanRead && e.GetCustomAttribute<ObsoleteAttribute>() is null)
                        .ToDictionary(e => e.Name, e => e.GetValue(source));
                    return AppendDictionary(builder, dic, true);
                }
                catch
                {
                }
            }

            return builder.Append(source?.ToString());

        }

        private static bool IsDelegate(Type type)
        {
            return type.IsSubclassOf(typeof(Delegate)) || type.Equals(typeof(Delegate));
        }


        private string JavaScriptStringEncode(string src)
        {
            var builder = new StringBuilder();
            foreach(var c in src)
            {
                switch(c)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\"':
                        builder.Append("\\\"");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            return builder.ToString();
        }

        private IndentStringBuilder AppendCollection(IndentStringBuilder builder, IEnumerable source, bool isNest)
        {
            var section = new CollectionSection(builder, '[', ']');

            section.Open();

            foreach (var item in source)
            {
                section.Increment();
                AppendObject(builder, item, true);
            }

            section.Close();

            return builder;
        }


        private IndentStringBuilder AppendGenericDictionary(IndentStringBuilder builder, IDictionary<string, object> source, bool isNest)
        {
            var section = new CollectionSection(builder, '{', '}');

            section.Open();

            foreach (var item in source)
            {
                section.Increment();
                AppendKeyValuePair(builder, item, true);
            }

            section.Close();

            return builder;
        }


        IndentStringBuilder AppendKeyValuePair(IndentStringBuilder builder, KeyValuePair<string, object> source, bool isNest)
        {
            builder.Append("\"" + source.Key + "\": ");
            AppendObject(builder, source.Value, true);
            return builder;
        }


        private IndentStringBuilder AppendDictionary(IndentStringBuilder builder, IDictionary source, bool isNest)
        {
            var section = new CollectionSection(builder, '{', '}');

            section.Open();

            foreach (DictionaryEntry item in source)
            {
                section.Increment();
                AppendKeyValuePair(builder, item, true);
            }

            section.Close();

            return builder;
        }

        private IndentStringBuilder AppendKeyValuePair(IndentStringBuilder builder, DictionaryEntry source, bool isNest)
        {
            builder.Append("\"" + source.Key + "\": ");
            AppendObject(builder, source.Value, true);
            return builder;
        }


        private class CollectionSection
        {
            private IndentStringBuilder _builder;
            private char _openChar;
            private char _closeChar;
            private int _count;

            public CollectionSection(IndentStringBuilder builder, char openChar, char closeChar)
            {
                _builder = builder;
                _openChar = openChar;
                _closeChar = closeChar;
            }

            public void Open()
            {
                _builder.Append(_openChar.ToString());
            }

            public void Close()
            {
                if (_count != 0)
                {
                    _builder.IndentDown();
                }

                _builder.Append(_closeChar.ToString());
            }

            public void Increment()
            {
                if (_count == 0)
                {
                    _builder.IndentUp();
                }
                else
                {
                    _builder.Append(",").AppendLine();
                }
                _count++;
            }
        }
    }
}
