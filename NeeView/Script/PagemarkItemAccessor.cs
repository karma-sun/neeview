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

        public bool IsExpanded
        {
            get { return _source.IsExpanded; }
            set { _source.IsExpanded = value; }
        }

        public string Name => _source.Value.DispName;
      
        public string Path => _source.Value.Path;

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
