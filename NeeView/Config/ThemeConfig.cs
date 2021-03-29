using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ThemeConfig : BindableBase
    {
        private ThemeType _theme = ThemeType.Dark;

        [JsonInclude, JsonPropertyName(nameof(CustomThemeFilePath))]
        public string _customThemeFilePath;


        // テーマ
        [PropertyMember]
        public ThemeType ThemeType
        {
            get { return _theme; }
            set { SetProperty(ref _theme, value); }
        }

        // カスタムテーマの保存場所
        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.SaveFile, Filter = "JSON|*.json")]
        public string CustomThemeFilePath
        {
            get { return _customThemeFilePath ?? SaveData.DefaultCustomThemeFilePath; }
            set { SetProperty(ref _customThemeFilePath, (string.IsNullOrWhiteSpace(value) || value.Trim() == SaveData.DefaultCustomThemeFilePath) ? null : value.Trim()); }
        }

        #region Obsolete

        [Obsolete("Use ThemeType instead.")] // ver.39
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PanelColor
        {
            get { return null; }
            set { ThemeType = value == "Light" ? ThemeType.Light : ThemeType.Dark; }
        }

        [Obsolete] // ver.39
        [JsonIgnore]
        public ThemeType MenuColor
        {
            get { return default; }
            set { }
        }

        #endregion Obsolete

    }
}