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
            Range = new PageRange();
            Collection = new List<ViewPage>();
        }

        public ViewPageCollection(PageRange range, List<ViewPage> collection)
        {
            Range = range;
            Collection = collection;
        }

        #endregion

        #region Properties

        public PageRange Range { get; }
        public List<ViewPage> Collection { get; }

        internal bool IsValid => Collection.Count > 0 && Collection.All(e => e.IsValid);

        #endregion
    }
}
