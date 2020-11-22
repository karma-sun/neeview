using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class FolderPanelModel : BindableBase
    {
        static FolderPanelModel() => Current = new FolderPanelModel();
        public static FolderPanelModel Current { get; }


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public bool IsVisiblePanelList { get; set; }

            [DataMember]
            public string GridLength0 { get; set; }
            [DataMember]
            public string GridLength2 { get; set; }


            public void RestoreConfig(Config config)
            {
            }
        }

        #endregion
    }
}
