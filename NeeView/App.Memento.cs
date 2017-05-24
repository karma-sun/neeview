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

        // フルスクリーン状態を復元する
        public bool IsSaveFullScreen { get; set; }

        // ウィンドウ座標を復元する
        public bool IsSaveWindowPlacement { get; set; }

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsMultiBootEnabled { get; set; }
            [DataMember]
            public bool IsSaveFullScreen { get; set; }
            [DataMember]
            public bool IsSaveWindowPlacement { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsMultiBootEnabled = this.IsMultiBootEnabled;
            memento.IsSaveFullScreen = this.IsSaveFullScreen;
            memento.IsSaveWindowPlacement = this.IsSaveWindowPlacement;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsMultiBootEnabled = memento.IsMultiBootEnabled;
            this.IsSaveFullScreen = memento.IsSaveFullScreen;
            this.IsSaveWindowPlacement = memento.IsSaveWindowPlacement;
        }
        #endregion

    }
}
