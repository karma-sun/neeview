using System;
using System.Collections.Generic;
using System.Reflection;

namespace NeeView
{
    public class WordNodeMemberAttribute : Attribute
    {
    }

    public static class WordNodeHelper
    {
        public static WordNode CreateClassWordNode(string name, Type type)
        {
            var node = new WordNode(name);
            node.Children = new List<WordNode>();

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<WordNodeMemberAttribute>() != null)
                {
                    node.Children.Add(new WordNode(method.Name));
                }
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<WordNodeMemberAttribute>() != null)
                {
                    node.Children.Add(new WordNode(property.Name));
                }
            }

            return node;
        }

    }
}
