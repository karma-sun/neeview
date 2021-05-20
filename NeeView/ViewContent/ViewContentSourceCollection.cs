using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class ViewContentSourceCollection
    {
        public ViewContentSourceCollection()
        {
            Range = new PageRange();
            Collection = new List<ViewContentSource>();
        }

        public ViewContentSourceCollection(PageRange range, List<ViewContentSource> collection)
        {
            Range = range;
            Collection = collection;
        }


        public PageRange Range { get; }

        public List<ViewContentSource> Collection { get; }

        public bool IsValid => Collection.Count > 0 && Collection.All(e => e.IsValid);

        // 読込中ページではない、完全なページであるか
        public bool IsFixedContents() => Collection?.All(x => x.GetContentType() != ViewContentType.Reserve) ?? false;
    }
}
