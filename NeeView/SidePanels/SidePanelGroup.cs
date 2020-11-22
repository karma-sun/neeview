using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    // [Obsolete] V1 compatible
    public class SidePanelGroup : BindableBase
    {
        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public List<string> PanelTypeCodes { get; set; }

            [DataMember]
            public string SelectedPanelTypeCode { get; set; }

            [DataMember]
            public double Width { get; set; }
        }

        #endregion

    }
}
