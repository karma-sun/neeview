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
        // マルチブートを許可する
        public bool IsMultiBootEnabled { get; set; }

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsMultiBootEnabled { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsMultiBootEnabled = this.IsMultiBootEnabled;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsMultiBootEnabled = memento.IsMultiBootEnabled;
        }
        #endregion

    }
}
