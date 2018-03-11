using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    // 表示ページコンテキスト
    public class ViewPageCollection
    {
        #region Constructors

        public ViewPageCollection()
        {
            Range = new PageDirectionalRange();
            Collection = new List<ViewPage>();
        }

        public ViewPageCollection(PageDirectionalRange range, List<ViewPage> collection)
        {
            Range = range;
            Collection = collection;
        }

        #endregion

        #region Properties

        public PageDirectionalRange Range { get; }
        public List<ViewPage> Collection { get; }

        internal bool IsValid => Collection.Count > 0 && Collection.All(e => e.IsValid);

        #endregion
    }
}
