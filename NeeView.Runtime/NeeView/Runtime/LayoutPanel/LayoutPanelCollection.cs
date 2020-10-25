using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeeView.Runtime.LayoutPanel
{
    public class LayoutPanelCollection : ObservableCollection<LayoutPanel>
    {
        public LayoutPanelCollection() : base()
        {
        }

        public LayoutPanelCollection(IEnumerable<LayoutPanel> collection) : base(collection)
        {
        }


        internal bool IsStandAlone(LayoutPanel panel)
        {
            return this.Count == 1 && this.First() == panel;
        }
    }
}
