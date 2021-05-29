using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ThemeConfig : BindableBase
    {
        private TheneSource _themeType = new TheneSource(NeeView.ThemeType.Dark);

        [JsonInclude, JsonPropertyName(nameof(CustomThemeFolder))]
        public string _customThemeFolder;


        // テーマ
        [PropertyMapIgnore]
        public TheneSource ThemeType
        {
            get { return _themeType; }
            set { SetProperty(ref _themeType, value); }
        }

        // テーマ (スクリプトアクセス用)
        [JsonIgnore]
        [ObjectMergeIgnore]
        [PropertyMapName(nameof(ThemeType))]
        public string ThemeString
        {
            get { return ThemeType.ToString(); }
            set { ThemeType = TheneSource.Parse(value); }
        }

        // カスタムテーマの保存場所
        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string CustomThemeFolder
        {
            get { return _customThemeFolder ?? SaveData.DefaultCustomThemeFolder; }
            set { SetProperty(ref _customThemeFolder, (string.IsNullOrWhiteSpace(value) || value.Trim() == SaveData.DefaultCustomThemeFolder) ? null : value.Trim()); }
        }


        #region Obsolete

        [Obsolete, Alternative(nameof(ThemeType), 39)] // ver.39
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PanelColor
        {
            get { return null; }
            set { ThemeType = new TheneSource(value == "Light" ? NeeView.ThemeType.Light : NeeView.ThemeType.Dark); }
        }

        [Obsolete, Alternative(null, 39)] // ver.39
        [JsonIgnore]
        public ThemeType MenuColor
        {
            get { return default; }
            set { }
        }

        #endregion Obsolete

    }
}