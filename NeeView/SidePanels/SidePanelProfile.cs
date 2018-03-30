using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class SidePanelProfile
    {
        public static SidePanelProfile Current { get; private set; }

        public SidePanelProfile()
        {
            Current = this;
        }

        [PropertyMember("@ParamSidePanelIsLeftRightKeyEnabled", Tips = "@ParamSidePanelIsLeftRightKeyEnabledTips")]
        public bool IsLeftRightKeyEnabled { get; set; } = true;

        [PropertyMember("@ParamSidePanelHitTestMargin")]
        public double HitTestMargin { get; set; } = 32.0;

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(true)]
            public bool IsLeftRightKeyEnabled { get; set; }
            [DataMember, DefaultValue(32.0)]
            public double HitTestMargin { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsLeftRightKeyEnabled = this.IsLeftRightKeyEnabled;
            memento.HitTestMargin = this.HitTestMargin;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsLeftRightKeyEnabled = memento.IsLeftRightKeyEnabled;
            this.HitTestMargin = memento.HitTestMargin;
        }

        #endregion
    }
}
