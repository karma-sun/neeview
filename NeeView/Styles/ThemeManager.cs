using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Text.Json;
using System.Collections;
using System.Text.Json.Serialization;
using System.IO;

namespace NeeView
{
    public enum ThemeType
    {
        Dark,
        Light,
        HighContrast,
        Custom,
    }


    public class ThemeManager : BindableBase
    {
        static ThemeManager() => Current = new ThemeManager();
        public static ThemeManager Current { get; }


        private ThemeProfile _themeProfile;


        private ThemeManager()
        {
            RefreshThemeColor();

            Config.Current.Theme.AddPropertyChanged(nameof(ThemeConfig.ThemeType), (s, e) =>
            {
                RefreshThemeColor();
            });
        }


        public event EventHandler ThemeProfileChanged;


        public ThemeProfile ThemeProfile
        {
            get { return _themeProfile; }
            private set { SetProperty(ref _themeProfile, value); }
        }


        public void RefreshThemeColor()
        {
            if (App.Current == null) return;

            ThemeProfile themeProfile;

            try
            {
                themeProfile = LoadThemeProfile(Config.Current.Theme.ThemeType);
                themeProfile.Verify();
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.ThemeErrorDialog_Title, ToastIcon.Error));
                themeProfile = ThemeProfile.Default;
            }

            foreach (var key in ThemeProfile.Keys)
            {
                var color = themeProfile.GetColor(key, 1.0);
                App.Current.Resources[key] = new SolidColorBrush(color);
            }

            ThemeProfile = themeProfile;

            ThemeProfileChanged?.Invoke(this, null);
        }

        private ThemeProfile LoadThemeProfile(ThemeType themeType)
        {
            switch (themeType)
            {
                case ThemeType.Dark:
                    return ThemeProfileTools.LoadFromContent("Themes/DarkTheme.json");

                case ThemeType.Light:
                    return ThemeProfileTools.LoadFromContent("Themes/LightTheme.json");

                case ThemeType.HighContrast:
                    return ThemeProfileTools.LoadFromContent("Themes/HighContrastTheme.json");

                case ThemeType.Custom:
                    if (File.Exists(Config.Current.Theme.CustomThemeFilePath))
                    {
                        try
                        {
                            return ThemeProfileTools.LoadFromFile(Config.Current.Theme.CustomThemeFilePath);
                        }
                        catch (Exception ex)
                        {
                            ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.ThemeErrorDialog_Title, ToastIcon.Error));
                        }
                    }
                    return LoadThemeProfile(ThemeType.Dark);

                default:
                    throw new NotSupportedException();
            }
        }

        public void OpenCustomThemeFile()
        {
            try
            {
                if (!File.Exists(Config.Current.Theme.CustomThemeFilePath))
                {
                    ThemeProfileTools.Save(ThemeProfile, Config.Current.Theme.CustomThemeFilePath);
                }

                ExternalProcess.Start(Config.Current.Theme.CustomThemeFilePath, null, ExternalProcessAtrtibute.ThrowException);
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.Resources.Word_Error);
            }
        }


        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember, DefaultValue(ThemeType.Dark)]
            public ThemeType PanelColor { get; set; }

            [DataMember, DefaultValue(ThemeType.Light)]
            public ThemeType MenuColor { get; set; }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                config.Theme.ThemeType = PanelColor;
            }
        }

        #endregion
    }
}
