
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 本の状態
    /// </summary>
    public class BookAccessor
    {
        [WordNodeMember]
        public string Path => BookOperation.Current.Book?.Address;

        [WordNodeMember]
        public bool IsMedia => BookOperation.Current.Book?.IsMedia == true;

        [WordNodeMember]
        public bool IsNew => BookOperation.Current.Book?.IsNew == true;

        public BookConfigAccessor Config { get; } = new BookConfigAccessor();

        [WordNodeMember]
        public int PageSize
        {
            get
            {
                var book = BookOperation.Current.Book;
                return (book != null) ? book.Pages.Count : 0;
            }
        }

        [WordNodeMember]
        public int ViewPageSize
        {
            get
            {
                var book = BookOperation.Current.Book;
                return (book != null) ? book.Viewer.ViewPageCollection.Collection.Count : 0;
            }
        }

        // NOTE: index is 1 start
        [WordNodeMember]
        public PageAccessor Page(int index)
        {
            var book = BookOperation.Current.Book;
            if (book != null)
            {
                var id = book.Pages.ClampPageNumber(index - 1);
                if (id == index - 1)
                {
                    return new PageAccessor(book.Pages[id]);
                }
            }
            return null;
        }

        [WordNodeMember]
        public PageAccessor ViewPage(int index)
        {
            var book = BookOperation.Current.Book;
            if (book != null)
            {
                if (index >= 0 && index < book.Viewer.ViewPageCollection.Collection.Count)
                {
                    return new PageAccessor(book.Viewer.ViewPageCollection.Collection[index].Page);
                }
            }
            return null;
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = new WordNode(name);
            node.Children = new List<WordNode>();

            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<WordNodeMemberAttribute>() != null)
                {
                    node.Children.Add(new WordNode(method.Name));
                }
            }

            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<WordNodeMemberAttribute>() != null)
                {
                    node.Children.Add(new WordNode(property.Name));
                }
            }

            node.Children.Add(Config.CreateWordNode(nameof(Config)));

            return node;
        }
    }
}
