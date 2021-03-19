using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ThemeConfig : BindableBase
    {
        private PanelColor _panelColor = PanelColor.Dark;

        /// <summary>
        /// テーマカラー：パネル
        /// </summary>
        [PropertyMember]
        public PanelColor PanelColor
        {
            get { return _panelColor; }
            set { SetProperty(ref _panelColor, value); }
        }

        #region Obsolete

        [Obsolete] // ver.39
        [JsonIgnore]
        public PanelColor MenuColor
        {
            get { return PanelColor; }
            set { }
        }

        #endregion Obsolete

    }
}