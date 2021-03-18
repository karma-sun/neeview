using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class FontsConfig : BindableBase
    {
        private double _fontScale;
        private double _menuFontScale;
        private double _folderTreeFontScale;
        private double _panelFontScale;

        [JsonPropertyName(nameof(FontName))]
        [JsonInclude]
        public string _fontName;

        [JsonPropertyName(nameof(PanelFontName))]
        [JsonInclude]
        public string _panelFontName;


        /// <summary>
        /// 標準フォント名
        /// </summary>
        [PropertyMember]
        [JsonIgnore]
        public string FontName
        {
            get { return _fontName ?? VisualParameters.SystemFontName; }
            set { SetProperty(ref _fontName, (string.IsNullOrWhiteSpace(value) || value == VisualParameters.SystemFontName) ? null : value); }
        }

        /// <summary>
        /// 標準フォントスケール
        /// </summary>
        [PropertyPercent(1.0, 2.0, TickFrequency = 0.05, IsEditable = true)]
        public double FontScale
        {
            get { return _fontScale <= 0.0 ? 15.0 / VisualParameters.SystemMessageFontSize : _fontScale ; }
            set { SetProperty(ref _fontScale, value); }
        }

        /// <summary>
        /// メニューフォントスケール
        /// </summary>
        [PropertyPercent(1.0, 2.0, TickFrequency = 0.05, IsEditable = true)]
        public double MenuFontScale
        {
            get { return _menuFontScale <= 0.0 ? 1.0 : _menuFontScale; }
            set { SetProperty(ref _menuFontScale, value); }
        }

        /// <summary>
        /// フォルダーツリーのフォントスケール
        /// </summary>
        [PropertyPercent(1.0, 2.0, TickFrequency = 0.05, IsEditable = true)]
        public double FolderTreeFontScale
        {
            get { return _folderTreeFontScale <= 0.0 ? 12.0 / VisualParameters.SystemMessageFontSize : _folderTreeFontScale; }
            set { SetProperty(ref _folderTreeFontScale, value); }
        }

        /// <summary>
        /// パネルフォント名
        /// </summary>
        [PropertyMember]
        [JsonIgnore]
        public string PanelFontName
        {
            get { return _panelFontName ?? VisualParameters.SystemFontName; }
            set { SetProperty(ref _panelFontName, (string.IsNullOrWhiteSpace(value) || value == VisualParameters.SystemFontName) ? null : value); }
        }

        /// <summary>
        /// パネルフォントスケール
        /// </summary>
        [PropertyPercent(1.0, 2.0, TickFrequency = 0.05, IsEditable = true)]
        public double PanelFontScale
        {
            get { return _panelFontScale <= 0.0 ? 15.0 / VisualParameters.SystemMessageFontSize : _panelFontScale; }
            set { SetProperty(ref _panelFontScale, value); }
        }
    }
}