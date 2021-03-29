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
        Custom,
    }


    public class ThemeManager : BindableBase
    {
        static ThemeManager() => Current = new ThemeManager();
        public static ThemeManager Current { get; }


        private ThemeManager()
        {
            RefreshThemeColor();

            Config.Current.Theme.AddPropertyChanged(nameof(ThemeConfig.ThemeType), (s, e) =>
            {
                RefreshThemeColor();
            });
        }


        public event EventHandler ThemeProfileChanged;


        public ThemeProfile ThemeProfile { get; private set; }


        public void RefreshThemeColor()
        {
            if (App.Current == null) return;

            try
            {
                ThemeProfile = LoadThemeProfile(Config.Current.Theme.ThemeType);
                ThemeProfile.Verify();
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.ThemeErrorDialog_Title, ToastIcon.Error));
                ThemeProfile = ThemeProfile.Default;
            }

            foreach (var key in ThemeProfile.Keys)
            {
                var color = ThemeProfile.GetColor(key);
                App.Current.Resources[key] = new SolidColorBrush(color);
            }

            ThemeProfileChanged?.Invoke(this, null);
        }

        private ThemeProfile LoadThemeProfile(ThemeType themeType)
        {

            if (themeType == ThemeType.Custom)
            {
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
            }

            if (themeType == ThemeType.Light)
            {
                return ThemeProfileTools.LoadFromContent("Themes/LightTheme.json");
            }

            return ThemeProfileTools.LoadFromContent("Themes/DarkTheme.json");
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
