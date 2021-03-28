using NeeLaboratory.ComponentModel;
using System.Windows.Media;

namespace NeeView
{
    public class ThemeBrushProvider : BindableBase
    {
        static ThemeBrushProvider() => Current = new ThemeBrushProvider();
        public static ThemeBrushProvider Current { get; }

        private SolidColorBrush _foregroundBrush;
        private SolidColorBrush _backgroundBrush;
        private SolidColorBrush _backgroundBrushRaw;
        private SolidColorBrush _baseBrush;
        private SolidColorBrush _iconBackgroundBrush;


        public ThemeBrushProvider()
        {
            ThemeProfile.Current.ThemeColorChanged +=
                (s, e) => RefreshBrushes();
            
            MainWindowModel.Current.CanHidePanelChanged +=
                (s, e) => RefreshBrushes();

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.Opacity),
                (s, e) => RefreshBrushes());

            RefreshBrushes();
        }


        public SolidColorBrush ForegroundBrush
        {
            get { return _foregroundBrush; }
            set { SetProperty(ref _foregroundBrush, value); }
        }

        public SolidColorBrush BackgroundBrushRaw
        {
            get { return _backgroundBrushRaw; }
            set { SetProperty(ref _backgroundBrushRaw, value); }
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
            var opacity = MainWindowModel.Current.CanHidePanel ? Config.Current.Panels.Opacity : 1.0;

            ForegroundBrush = (SolidColorBrush)App.Current.Resources["Dfault.Foreground"];
            BackgroundBrushRaw = (SolidColorBrush)App.Current.Resources["Window.Background"];
            BackgroundBrush = CreatePanelBrush(BackgroundBrushRaw, opacity);
            BaseBrush = CreatePanelBrush((SolidColorBrush)App.Current.Resources["SidePanel.Background"], opacity);
            IconBackgroundBrush = CreatePanelBrush((SolidColorBrush)App.Current.Resources["SideBar.Background"], opacity);
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

    }

}
