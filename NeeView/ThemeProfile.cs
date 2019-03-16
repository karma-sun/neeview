using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ThemeProfile : BindableBase
    {
        static ThemeProfile() => Current = new ThemeProfile();
        public static ThemeProfile Current { get; }

        private PanelColor _menuColor = PanelColor.Light;


        private ThemeProfile()
        {
        }

        /// <summary>
        /// テーマカラー：メニュー
        /// </summary>
        [PropertyMember("@ParamMenuColor")]
        public PanelColor MenuColor
        {
            get { return _menuColor; }
            set { SetProperty(ref _menuColor, value); }
        }

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(PanelColor.Light)]
            public PanelColor MenuColor { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.MenuColor = this.MenuColor;

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.MenuColor = memento.MenuColor;
        }

        #endregion

    }
}
