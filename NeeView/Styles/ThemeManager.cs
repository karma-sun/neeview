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
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using NeeLaboratory.Collection;

namespace NeeView
{
    public class ThemeManager : BindableBase
    {
        static ThemeManager() => Current = new ThemeManager();
        public static ThemeManager Current { get; }

        private const string _themeProtocolHeader = "themes://";

        private static readonly string _darkThemeContentPath = "Libraries/Themes/DarkTheme.json";
        private static readonly string _darkMonochromeThemeContentPath = "Libraries/Themes/DarkMonochromeTheme.json";
        private static readonly string _lightThemeContentPath = "Libraries/Themes/LightTheme.json";
        private static readonly string _lightMonochromeThemeContentPath = "Libraries/Themes/LightMonochromeTheme.json";
        private static readonly string _highContrastThemeContentPath = "Libraries/Themes/HighContrastTheme.json";
        private static readonly string _customThemeTemplateContentPath = "Libraries/Themes/CustomThemeTemplate.json";

        private ThemeProfile _themeProfile;
        private string _selectedItem;


        private ThemeManager()
        {
            _selectedItem = Config.Current.Theme.ThemeType.ToString();

            RefreshThemeColor();

            Config.Current.Theme.AddPropertyChanged(nameof(ThemeConfig.ThemeType),
                (s, e) => RefreshThemeColor());

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


        [PropertyStrings(Name = "@ThemeConfig.ThemeType")]
        public string SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    Config.Current.Theme.ThemeType = TheneSource.Parse(_selectedItem);
                }
            }
        }


        public static KeyValuePairList<string, string> CreateItemsMap()
        {
            var defaultThemes = Enum.GetValues(typeof(ThemeType))
                .Cast<ThemeType>()
                .Where(e => e != ThemeType.Custom)
                .Select(e => new KeyValuePair<string, string>(e.ToString(), e.ToAliasName()));

            var customThemes = CollectCustomThemes()
                .Select(e => new KeyValuePair<string, string>(e.ToString(), Path.GetFileNameWithoutExtension(e.FileName)));

            var map = defaultThemes.Concat(customThemes)
                .ToKeyValuePairList(e => e.Key, e => e.Value);

            return map;
        }

        public static List<TheneSource> CollectCustomThemes()
        {
            if (!string.IsNullOrEmpty(Config.Current.Theme.CustomThemeFolder))
            {
                try
                {
                    var directory = new DirectoryInfo(Config.Current.Theme.CustomThemeFolder);
                    if (directory.Exists)
                    {
                        return directory.GetFiles("*.json").Select(e => new TheneSource(ThemeType.Custom, e.Name)).ToList();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return new List<TheneSource>();
        }

        public void Touch()
        {
            // NOTE: シングルトンインスタンスの保証
        }

        public void RefreshThemeColor()
        {
            var themeProfile = GeThemeProfile(Config.Current.Theme.ThemeType, true);

            foreach (var key in ThemeProfile.Keys)
            {
                App.Current.Resources[key] = new SolidColorBrush(themeProfile.GetColor(key, 1.0));
            }

            // special theme color
            App.Current.Resources["BottomBar.Background.Color"] = themeProfile.GetColor("BottomBar.Background", 1.0);

            // window border thickness
            App.Current.Resources["Window.BorderThickness"] = themeProfile.GetColor("Window.Border", 1.0).A > 0x00 ? new Thickness(1.0) : default;

            // dialog border thickness
            App.Current.Resources["Window.Dialog.BorderThickness"] = themeProfile.GetColor("Window.Dialog.Border", 1.0).A > 0x00 ? new Thickness(1.0) : default;

            if (themeProfile.GetColor("Button.Background", 1.0).A > 0x00)
            {
                App.Current.Resources["Button.Accent.Background"] = new SolidColorBrush(themeProfile.GetColor("Control.Accent", 1.0));
                App.Current.Resources["Button.Accent.Foreground"] = new SolidColorBrush(themeProfile.GetColor("Control.AccentText", 1.0));
                App.Current.Resources["Button.Accent.Border"] = new SolidColorBrush(themeProfile.GetColor("Control.Accent", 1.0));
            }
            else
            {
                App.Current.Resources["Button.Accent.Background"] = new SolidColorBrush(themeProfile.GetColor("Button.Background", 1.0));
                App.Current.Resources["Button.Accent.Foreground"] = new SolidColorBrush(themeProfile.GetColor("Button.Foreground", 1.0));
                App.Current.Resources["Button.Accent.Border"] = new SolidColorBrush(themeProfile.GetColor("Control.Accent", 1.0));
            }

            ThemeProfile = themeProfile;
            ThemeProfileChanged?.Invoke(this, null);
        }


        private ThemeProfile GeThemeProfile(TheneSource theneId, bool isShowExceptionToast)
        {
            try
            {
                var themeProfile = LoadThemeProfile(theneId);
                themeProfile.Verify();
                return themeProfile.Validate();
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.ThemeErrorDialog_Title, ToastIcon.Error));

                if (theneId.Type is ThemeType.Custom)
                {
                    return GeThemeProfile(new TheneSource(ThemeType.Dark), false);
                }
                else
                {
                    return ThemeProfile.Default.Validate();
                }
            }
        }

        private ThemeProfile LoadThemeProfile(TheneSource themeId)
        {
            switch (themeId.Type)
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
                        return LoadThemeProfile(new TheneSource(ThemeType.HighContrast));
                    }
                    else if (Windows10Tools.IsWindows10_OrGreater)
                    {
                        ThemeProfile themeProfile;
                        switch (SystemVisualParameters.Current.Theme)
                        {
                            case SystemThemeType.Dark:
                                themeProfile = LoadThemeProfile(new TheneSource(ThemeType.Dark));
                                break;

                            case SystemThemeType.Light:
                                themeProfile = LoadThemeProfile(new TheneSource(ThemeType.Light));
                                break;

                            default:
                                throw new NotSupportedException();
                        }
                        themeProfile.Colors["Control.Accent"] = new ThemeColor(SystemVisualParameters.Current.AccentColor, 1.0);
                        return themeProfile;
                    }
                    else
                    {
                        return LoadThemeProfile(new TheneSource(ThemeType.Dark));
                    }

                case ThemeType.Custom:
                    if (string.IsNullOrEmpty(Config.Current.Theme.CustomThemeFolder))
                    {
                        ToastService.Current.Show(new Toast(Properties.Resources.ThemeErrorDialog_FolderIsNotSet, Properties.Resources.ThemeErrorDialog_Title, ToastIcon.Error));
                    }
                    else
                    {
                        try
                        {
                            var path = Path.Combine(Config.Current.Theme.CustomThemeFolder, themeId.FileName);
                            return ValidateBasedOn(ThemeProfileTools.LoadFromFile(path), Path.GetDirectoryName(path));
                        }
                        catch (Exception ex)
                        {
                            ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.ThemeErrorDialog_Title, ToastIcon.Error));
                        }
                    }
                    return LoadThemeProfile(new TheneSource(ThemeType.Dark));

                default:
                    throw new NotSupportedException();
            }
        }

        private ThemeProfile ValidateBasedOn(ThemeProfile themeProfile, string currentPath, IEnumerable<string> nests = null)
        {
            if (string.IsNullOrWhiteSpace(themeProfile.BasedOn))
            {
                return themeProfile;
            }

            if (themeProfile.BasedOn.StartsWith(_themeProtocolHeader))
            {
                var path = themeProfile.BasedOn.Substring(_themeProtocolHeader.Length);
                var baseTheme = ThemeProfileTools.LoadFromContent("Libraries/Themes/" + path);
                return ThemeProfileTools.Merge(baseTheme, themeProfile);
            }
            else
            {
                var path = Path.IsPathRooted(themeProfile.BasedOn) ? themeProfile.BasedOn : Path.Combine(currentPath, themeProfile.BasedOn);
                if (nests != null && nests.Contains(path)) throw new FormatException($"Circular reference: {path}");
                nests = nests is null ? new List<string>() { path } : nests.Append(path);
                var baseTheme = ValidateBasedOn(ThemeProfileTools.LoadFromFile(path), Path.GetDirectoryName(path), nests);
                return ThemeProfileTools.Merge(baseTheme, themeProfile);
            }
        }

        public void OpenCustomThemeFolder()
        {
            if (string.IsNullOrEmpty(Config.Current.Theme.CustomThemeFolder))
            {
                new MessageDialog(Properties.Resources.ThemeErrorDialog_FolderIsNotSet, Properties.Resources.Word_Error).ShowDialog();
                return;
            }

            try
            {
                var directory = new DirectoryInfo(Config.Current.Theme.CustomThemeFolder);
                if (!directory.Exists)
                {
                    directory.Create();
                    ThemeProfileTools.SaveFromContent(_customThemeTemplateContentPath, Path.Combine(directory.FullName, "Sample.json"));
                }
                ExternalProcess.Start("explorer.exe", $"\"{directory.FullName}\"", new ExternalProcessOptions() { IsThrowException = true });
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.Resources.Word_Error).ShowDialog();
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
                config.Theme.ThemeType = new TheneSource(PanelColor);
            }
        }

        #endregion
    }
}
