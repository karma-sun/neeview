using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public partial class App : Application
    {
        // オートGC
        public bool IsAutoGC
        {
            get { return MemoryControl.Current.IsAutoGC; }
            set { MemoryControl.Current.IsAutoGC = value; }
        }


        #region Memento
        [DataContract]
        public class Memento
        {

        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
        }
        #endregion

    }
}
