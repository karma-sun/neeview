using System;
using System.Collections.Generic;
using System.Reflection;

namespace NeeView
{
    public static class WordNodeHelper
    {
        public static WordNode CreateClassWordNode(string name, Type type)
        {
            var node = new WordNode(name);
            node.Children = new List<WordNode>();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<WordNodeMemberAttribute>();
                if (attribute != null && attribute.IsAutoCollect)
                {
                    node.Children.Add(new WordNode(property.Name));
                }
            }

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<WordNodeMemberAttribute>();
                if (attribute != null && attribute.IsAutoCollect)
                {
                    node.Children.Add(new WordNode(method.Name));
                }
            }

            return node;
        }

    }
}
