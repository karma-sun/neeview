using NeeView.Collections.Generic;

namespace NeeView
{
    public class PagemarkItemAccessor
    {
        private TreeListNode<IPagemarkEntry> _source;

        public PagemarkItemAccessor(TreeListNode<IPagemarkEntry> source)
        {
            _source = source;
        }

        internal TreeListNode<IPagemarkEntry> Source => _source;

        [WordNodeMember]
        public bool IsExpanded
        {
            get { return _source.IsExpanded; }
            set { _source.IsExpanded = value; }
        }

        [WordNodeMember]
        public string Name => _source.Value.DispName;

        [WordNodeMember]
        public string Path => _source.Value.Path;

        [WordNodeMember]
        public string Type
        {
            get
            {
                switch(_source.Value)
                {
                    case Pagemark _:
                        return "Pagemark";
                    case PagemarkFolder _:
                        return "Book";
                    default:
                        return null;
                }
            }
        }
    }
}
