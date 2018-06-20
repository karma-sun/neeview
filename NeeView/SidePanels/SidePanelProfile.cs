using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace NeeView
{
    public class SidePanelProfile : BindableBase
    {
        public static SidePanelProfile Current { get; private set; }


        private double _opacity = 0.95;
        private SolidColorBrush _backgroundBrush;
        private SolidColorBrush _baseBrush;
        private SolidColorBrush _iconBackgroundBrush;


        public SidePanelProfile()
        {
            Current = this;

            MainWindowModel.Current.ThemeColorChanged += (s, e) => RefreshBrushes();
            MainWindowModel.Current.CanHidePanelChanged += (s, e) => RefreshBrushes();

            RefreshBrushes();
        }


        [PropertyMember("@ParamSidePanelIsLeftRightKeyEnabled", Tips = "@ParamSidePanelIsLeftRightKeyEnabledTips")]
        public bool IsLeftRightKeyEnabled { get; set; } = true;

        [PropertyMember("@ParamSidePanelHitTestMargin")]
        public double HitTestMargin { get; set; } = 32.0;

        [PropertyRange(0.0, 1.0, Name = "@ParamSidePanelOpacity")]
        public double Opacity
        {
            get { return _opacity; }
            set
            {
                if (SetProperty(ref _opacity, value))
                {
                    RefreshBrushes();
                }
            }
        }

        public SolidColorBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { SetProperty(ref _backgroundBrush, value); }
        }

        public SolidColorBrush BaseBrush
        {
            get { return _baseBrush; }
            set { SetProperty(ref _baseBrush, value); }
        }

        public SolidColorBrush IconBackgroundBrush
        {
            get { return _iconBackgroundBrush; }
            set { SetProperty(ref _iconBackgroundBrush, value); }
        }


        private void RefreshBrushes()
        {
            var opacity = MainWindowModel.Current.CanHidePanel ? _opacity : 1.0;

            BackgroundBrush = CreatePanelBrush((SolidColorBrush)App.Current.Resources["NVBackground"], opacity);
            BaseBrush = CreatePanelBrush((SolidColorBrush)App.Current.Resources["NVBaseBrush"], opacity);
            IconBackgroundBrush = CreatePanelBrush((SolidColorBrush)App.Current.Resources["NVPanelIconBackground"], opacity);
        }

        private SolidColorBrush CreatePanelBrush(SolidColorBrush source, double opacity)
        {
            if (opacity < 1.0)
            {
                var color = source.Color;
                color.A = (byte)NeeLaboratory.MathUtility.Clamp((int)(opacity * 0xFF), 0x00, 0xFF);
                return new SolidColorBrush(color);
            }
            else
            {
                return source;
            }
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(true)]
            public bool IsLeftRightKeyEnabled { get; set; }
            [DataMember, DefaultValue(32.0)]
            public double HitTestMargin { get; set; }
            [DataMember, DefaultValue(0.95)]
            public double Opacity { get; set; }

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
            memento.Opacity = this.Opacity;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsLeftRightKeyEnabled = memento.IsLeftRightKeyEnabled;
            this.HitTestMargin = memento.HitTestMargin;
            this.Opacity = memento.Opacity;
        }

        #endregion
    }
}
