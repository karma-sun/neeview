using System.Collections.Generic;
using System.Reflection;

namespace NeeView
{
    public class BookshelfAccessor
    {
        private BookshelfFolderList _bookshelf;

        public BookshelfAccessor(BookshelfFolderList bookshelf)
        {
            _bookshelf = bookshelf;
        }

        [WordNodeMember]
        public string Path
        {
            get { return _bookshelf.Place.SimplePath; }
            set { _bookshelf.RequestPlace(new QueryPath(value), null, FolderSetPlaceOption.UpdateHistory); }
        }


        internal WordNode CreateWordNode(string name)
        {
            // TODO: これ共通化できそう
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

            // NOTE: さらに階層が必要な場合
            // node.Children.Add(Config.CreateWordNode(nameof(Config)));

            return node;
        }
    }
}
