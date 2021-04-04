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
        DarkMonochrome,
        Light,
        LightMonochrome,
        HighContrast,
        System,
        Custom,
    }


    public class ThemeManager : BindableBase
    {
        static ThemeManager() => Current = new ThemeManager();
        public static ThemeManager Current { get; }

        private const string _themeProtocolHeader = "themes://";

        private static readonly string _darkThemeContentPath = "Themes/DarkTheme.json";
        private static readonly string _darkMonochromeThemeContentPath = "Themes/DarkMonochromeTheme.json";
        private static readonly string _lightThemeContentPath = "Themes/LightTheme.json";
        private static readonly string _lightMonochromeThemeContentPath = "Themes/LightMonochromeTheme.json";
        private static readonly string _highContrastThemeContentPath = "Themes/HighContrastTheme.json";
        private static readonly string _customThemeTemplateContentPath = "Themes/CustomThemeTemplate.json";

        private ThemeProfile _themeProfile;


        private ThemeManager()
        {
            RefreshThemeColor();

            Config.Current.Theme.AddPropertyChanged(nameof(ThemeConfig.ThemeType),
                (s, e) => RefreshThemeColor());

            Config.Current.Theme.AddPropertyChanged(nameof(ThemeConfig.CustomThemeFilePath),
                (s, e) =>
                {
                    if (Config.Current.Theme.ThemeType == ThemeType.Custom)
                    {
                        RefreshThemeColor();
                    }
                });

            SystemVisualParameters.Current.AddPropertyChanged(nameof(SystemVisualParameters.IsHighContrast),
                (s, e) => RefreshThemeColor());

            SystemVisualParameters.Current.AddPropertyChanged(nameof(SystemVisualParameters.Theme),
                (s, e) => RefreshThemeColor());

            SystemVisualParameters.Current.AddPropertyChanged(nameof(SystemVisualParameters.AccentColor),
                (s, e) => RefreshThemeColor());
        }


        public event EventHandler ThemeProfileChanged;


        public ThemeProfile ThemeProfile
        {
            get { return _themeProfile; }
            private set { SetProperty(ref _themeProfile, value); }
        }


        public void RefreshThemeColor()
        {
            var themeProfile = GeThemeProfile(Config.Current.Theme.ThemeType, true);

            foreach (var key in ThemeProfile.Keys)
            {
                App.Current.Resources[key] = new SolidColorBrush(themeProfile.GetColor(key, 1.0));
            }

            // NOTE: special theme color
            App.Current.Resources["PageSelectionBar.Background.Color"] = themeProfile.GetColor("PageSelectionBar.Background", 1.0);

            ThemeProfile = themeProfile;

            ThemeProfileChanged?.Invoke(this, null);
        }

        private ThemeProfile GeThemeProfile(ThemeType themeType, bool isShowExceptionToast)
        {
            try
            {
                var themeProfile = LoadThemeProfile(themeType);
                themeProfile.Verify();
                return themeProfile.Validate();
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.ThemeErrorDialog_Title, ToastIcon.Error));

                if (themeType is ThemeType.Custom)
                {
                    return GeThemeProfile(ThemeType.Dark, false);
                }
                else
                {
                    return ThemeProfile.Default.Validate();
                }
            }
        }

        private ThemeProfile LoadThemeProfile(ThemeType themeType)
        {
            switch (themeType)
            {
                case ThemeType.Dark:
                    return ThemeProfileTools.LoadFromContent(_darkThemeContentPath);

                case ThemeType.DarkMonochrome:
                    return ThemeProfileTools.LoadFromContent(_darkMonochromeThemeContentPath);

                case ThemeType.Light:
                    return ThemeProfileTools.LoadFromContent(_lightThemeContentPath);

                case ThemeType.LightMonochrome:
                    return ThemeProfileTools.LoadFromContent(_lightMonochromeThemeContentPath);

                case ThemeType.HighContrast:
                    return ThemeProfileTools.LoadFromContent(_highContrastThemeContentPath);

                case ThemeType.System:
                    if (SystemVisualParameters.Current.IsHighContrast)
                    {
                        return LoadThemeProfile(ThemeType.HighContrast);
                    }
                    else
                    {
                        ThemeProfile themeProfile;
                        switch (SystemVisualParameters.Current.Theme)
                        {
                            case SystemThemeType.Dark:
                                themeProfile = LoadThemeProfile(ThemeType.Dark);
                                break;

                            case SystemThemeType.Light:
                                themeProfile = LoadThemeProfile(ThemeType.Light);
                                break;

                            default:
                                throw new NotSupportedException();
                        }
                        themeProfile.Colors["Control.Accent"] = new ThemeColor(SystemVisualParameters.Current.AccentColor, 1.0);
                        return themeProfile;
                    }

                case ThemeType.Custom:
                    if (File.Exists(Config.Current.Theme.CustomThemeFilePath))
                    {
                        try
                        {
                            return ValidateBasedOn(ThemeProfileTools.LoadFromFile(Config.Current.Theme.CustomThemeFilePath), Path.GetDirectoryName(Config.Current.Theme.CustomThemeFilePath));
                        }
                        catch (Exception ex)
                        {
                            ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.ThemeErrorDialog_Title, ToastIcon.Error));
                        }
                    }
                    return ValidateBasedOn(ThemeProfileTools.LoadFromContent(_customThemeTemplateContentPath), Environment.AssemblyFolder);

                default:
                    throw new NotSupportedException();
            }
        }

        private ThemeProfile ValidateBasedOn(ThemeProfile themeProfile, string currentPath)
        {
            if (string.IsNullOrWhiteSpace(themeProfile.BasedOn))
            {
                return themeProfile;
            }

            if (themeProfile.BasedOn.StartsWith(_themeProtocolHeader))
            {
                var path = themeProfile.BasedOn.Substring(_themeProtocolHeader.Length);
                var baseTheme = ThemeProfileTools.LoadFromContent("Themes/" + path + ".json");
                return ThemeProfileTools.Merge(baseTheme, themeProfile);
            }
            else
            {
                var path = Path.IsPathRooted(themeProfile.BasedOn) ? themeProfile.BasedOn : Path.Combine(currentPath, themeProfile.BasedOn);
                var baseTheme = ValidateBasedOn(ThemeProfileTools.LoadFromFile(path), Path.GetDirectoryName(path));
                return ThemeProfileTools.Merge(baseTheme, themeProfile);
            }
        }

        public void OpenCustomThemeFile()
        {
            try
            {
                if (!File.Exists(Config.Current.Theme.CustomThemeFilePath))
                {
                    ThemeProfileTools.SaveFromContent(_customThemeTemplateContentPath, Config.Current.Theme.CustomThemeFilePath);
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
